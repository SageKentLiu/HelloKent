using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFACalendar;

namespace SFACalcEngine
{
    class MidMonthConvention : IBAAvgConvention
    {
        IBACalendar m_pObjCalendar;
        DateTime m_dtStartDate;
        DateTime m_dtEndDate;
        DateTime m_dtPISDate;
        double m_dblLife;
        MonthlyYear m_pMonthlyYear;

        public MidMonthConvention()
        {

        }

        public bool Initialize(SFACalendar.IBACalendar calendar, DateTime PlacedInService, double Life)
        {
            int iYear;
            int iMonth;
            int iDay;
            IBAFiscalYear FY;
            bool hr;
            short iFYNum;
            DateTime dtStartDate;

            if (calendar == null || PlacedInService <= DateTime.MinValue || Life <= 0)
                return false;

            m_pObjCalendar = null;
            m_pObjCalendar = calendar;
            m_dtPISDate = PlacedInService;
            m_dblLife = Life;

            if ( m_pMonthlyYear == null )
		        m_pMonthlyYear = new MonthlyYear();

	        if ( !(hr = m_pObjCalendar.GetFiscalYear(m_dtPISDate, out FY)))
        		        return hr;
	        iFYNum = FY.FYNum;
	        dtStartDate = FY.YRStartDate;

            m_pMonthlyYear.FiscalYearInfo = FY;
            m_pMonthlyYear.DeemedFYDates();
            m_dtStartDate = m_pMonthlyYear.GetMidMonthDate(m_dtPISDate);
    
            //calc the end date
            iYear = m_dtStartDate.Year + (int)(m_dblLife);
            iMonth = m_dtStartDate.Month + (int)((m_dblLife - (int)(m_dblLife)) * 12);
            iDay = m_dtStartDate.Day;

	        // adjust for special start of business case where deemed start is before start of bus.
	        if ( iFYNum == 1 && m_dtStartDate < dtStartDate )
		        m_dtStartDate = dtStartDate;

	        if (iMonth > 12)
	        {
		        iMonth -= 12;
		        iYear ++;
	        }
            //deemed end date
            m_dtEndDate = new DateTime(iYear, iMonth, iDay).AddDays (- 1);
            return true;
        }

        public bool GetFirstYearFactor(DateTime dtDate, out double pVal)
        {
            DateTime dtSDate;
            DateTime dtEDate;
            IBAFiscalYear FY;
            short iFYNum;
            bool hr;
            pVal = 0.0;

            if (m_pObjCalendar == null)
                throw new Exception("Avg Convention not initialized.");
            if (dtDate <= DateTime.MinValue)
                throw new Exception("Avg Convention not initialized.");

	        if ( !(hr = m_pObjCalendar.GetFiscalYear(dtDate, out FY)) )
                return hr;
            dtEDate = FY.YREndDate;
            dtSDate = FY.YRStartDate;
            iFYNum = FY.FYNum;
 
            //    if( dtDate > dtEDate || dtDate < dtSDate )
            //		return S_FALSE;

            m_pMonthlyYear.FiscalYearInfo = FY;
            m_pMonthlyYear.DeemedFYDates();
            if (iFYNum == 1 && dtDate == dtSDate)
                dtDate = new DateTime(dtDate.Year, dtDate.Month, 15);
            pVal = m_pMonthlyYear.GetFirstYearFactor(dtDate);
            return true;
        }

        public bool GetLastYearFactor(double RemainingLife, DateTime dtDate, out double pVal)
        {
            IBAFiscalYear FY;
            IBACalcPeriod pObjIPd1;
            IBACalcPeriod pObjIPd2;
            DateTime dtSDate;
            DateTime dtEDate;
            short iPNum1;
            short iPNum2;
            double dFactor1;
            double dFactor2;
            bool hr;
            pVal = 0.0;

            if (m_pObjCalendar == null)
                throw new Exception("Avg Convention not initialized.");

            if (!(hr = m_pObjCalendar.GetFiscalYear(dtDate, out FY)))
                return hr;
            dtEDate = FY.YREndDate;
            dtSDate = FY.YRStartDate;
            
            if (dtDate <= DateTime.MinValue)
            {
                dtDate = m_dtEndDate;
            }
            if (dtDate < m_dtStartDate)
                dtDate = m_dtStartDate;

            if (dtDate > dtEDate || dtDate < dtSDate)
                return hr;

            m_pMonthlyYear.FiscalYearInfo = FY;
            m_pMonthlyYear.DeemedFYDates();
            if (dtDate >= dtSDate && dtDate <= dtEDate &&
                m_dtStartDate >= dtSDate && m_dtStartDate <= dtEDate)
            {
                //the dispdate is in the first year
                if (!(hr = FY.GetPeriod(m_dtStartDate, out pObjIPd1)) ||
                    !(hr = FY.GetPeriod(dtDate, out pObjIPd2)) )
                    return hr;
                iPNum1 = pObjIPd1.PeriodNum;
                iPNum2 = pObjIPd2.PeriodNum;

                if (iPNum1 == iPNum2)
                {
                    //if in the same month, the factor should be the same
                    pVal = 0;
                    return true;
                }
                //            return GetFirstYearFactor(m_pMonthlyYear.GetMidMonthDate(dtDate), pVal);
                else
                {
                    //if in the diff month, find the difference
                    //
                    //-+---------------+------------------------------|------.
                    // startdate      dispdate                      year end
                    // |<-- the diff .|
                    // "the dif" x "anual amount" is the amt for this period
                    if (!(hr = GetFirstYearFactor(m_dtStartDate, out dFactor1)) ||
                        !(hr = GetFirstYearFactor(m_pMonthlyYear.GetMidMonthDate(dtDate), out dFactor2)))
                        return hr;
                    pVal = dFactor1 - dFactor2;
                }
            }
            else
            {
                //in diff year
                pVal = m_pMonthlyYear.GetLastYearFactor(m_pMonthlyYear.GetMidMonthDate(dtDate));
            }
            return true;
        }

        public bool GetDisposalYearFactor(double RemainingLife, DateTime dtDate, out double pVal)
        {
            return GetLastYearFactor(RemainingLife, dtDate, out pVal);
        }

        public DateTime DeemedStartDate
        {
            get { return m_dtStartDate; }
        }

        public DateTime DeemedEndDate
        {
            get { return m_dtEndDate; }
        }

        public bool IsSplitNeeded(DateTime dtDate, out bool pVal)
        {
            pVal = true;
            return true;
        }

        public bool GetFirstYearSegmentInfo(ref double dblFraction, ref DateTime dtFraSegStartDate, ref DateTime dtFraSegEndDate, ref short iFraSegTPWeight, ref DateTime dtRemSegStartDate, ref DateTime dtRemSegEndDate, ref short iRemSegTPWeight, out bool pVal)
        {
            int iDays;
            int iTotalDays;
            DateTime dtSDate;
            DateTime dtEDate;
            DateTime dtPSDate;
            DateTime dtPEDate;
            ECALENDARCYCLE_CYCLETYPE eCType;
            IBACalcPeriod pObjPeriod;
            IBAFiscalYear FY;
            bool hr;
            pVal = false;

            if (m_pObjCalendar == null)
                throw new Exception("Avg Convention not initialized.");

	        if( !(hr = m_pObjCalendar.GetFiscalYear(m_dtPISDate, out FY)) ||
		        !(hr = FY.GetPeriod(m_dtPISDate, out pObjPeriod)) )
                return hr;
	        dtEDate = FY.YREndDate;
	        dtSDate = FY.YRStartDate;
	        dtPSDate = pObjPeriod.PeriodStart;
	        dtPEDate = pObjPeriod.PeriodEnd;
	        eCType = FY.CycleType;
    
	        if ( m_dtStartDate >= dtSDate && m_dtStartDate <= dtEDate &&
		         m_dtEndDate >= dtSDate && m_dtEndDate <= dtEDate )
	        {
		        dtEDate = m_dtEndDate;
	        }

        //    if( m_dtPISDate > dtEDate || m_dtPISDate < dtSDate )
        //		return S_FALSE;
    
            if( eCType == ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_MONTHLY )
	        {
		        if( !(hr = FY.GetCurrentPeriodWeight(m_dtPISDate, out iFraSegTPWeight)) )
			        return hr;

		        if ( dtPEDate == dtEDate )
		        {
			        iRemSegTPWeight = 0;

			        dblFraction = 1;

			        dtFraSegStartDate = dtPSDate;
			        dtFraSegEndDate = dtPEDate;
			        dtRemSegStartDate = dtEDate;
			        dtRemSegEndDate = dtEDate;
		        }
		        else
		        {
			        if( !(hr = FY.GetPeriodWeights(dtPEDate.AddDays (+1), dtEDate, out iRemSegTPWeight)) )
				        return hr;
			        dblFraction = 0.5 * (double)(iFraSegTPWeight) / 
							        ((double)(iRemSegTPWeight) + 0.5 * (double)(iFraSegTPWeight));

			        dtFraSegStartDate = dtPSDate;
			        dtFraSegEndDate = dtPEDate;
			        dtRemSegStartDate = dtFraSegEndDate.AddDays (+ 1);
			        dtRemSegEndDate = dtEDate;
		        }
            }
	        else
	        {
                iDays = (int)((dtPEDate - m_dtPISDate).TotalDays) + 1;
                iTotalDays = (int)((dtPEDate - dtPSDate).TotalDays) + 1;
        
		        if( !(hr = FY.GetCurrentPeriodWeight(m_dtPISDate, out iFraSegTPWeight)) )
			        return hr;

		        if ( dtPEDate == dtEDate )
		        {
			        iRemSegTPWeight = 0;

			        dblFraction = 1;

			        dtFraSegStartDate = dtPSDate;
			        dtFraSegEndDate = dtPEDate;
			        dtRemSegStartDate = dtEDate;
			        dtRemSegEndDate = dtEDate;
		        }
		        else
		        {
			        if( !(hr = FY.GetPeriodWeights(dtPEDate.AddDays (+1), dtEDate, out iRemSegTPWeight)) )
				        return hr;
        
			        dblFraction = ((double)(iFraSegTPWeight) * (double)(iDays) / (double)(iTotalDays)) / 
						           ((double)(iRemSegTPWeight) + (double)(iFraSegTPWeight) * (double)(iDays) / 
						           (double)(iTotalDays));

			        dtFraSegStartDate = dtPSDate; 
			        dtFraSegEndDate = dtPEDate;
			        dtRemSegStartDate = dtFraSegEndDate.AddDays (+ 1);
			        dtRemSegEndDate = dtEDate;
		        }
            }
            return true;
        }

        public bool GetLastYearSegmentInfo(ref double dblFraction, ref DateTime dtFraSegStartDate, ref DateTime dtFraSegEndDate, ref short iFraSegTPWeight, ref DateTime dtRemSegStartDate, ref DateTime dtRemSegEndDate, ref short iRemSegTPWeight, out bool pVal)
        {
            int iDays;
            int iTotalDays;
            DateTime dtSDate;
            DateTime dtEDate;
            DateTime dtPSDate;
            DateTime dtPEDate;
            ECALENDARCYCLE_CYCLETYPE eCType;
            IBACalcPeriod pObjPeriod;
            IBAFiscalYear FY;
            bool hr;
            pVal = false;

            if (m_pObjCalendar == null)
                throw new Exception("Avg Convention not initialized.");

	        if( !(hr = m_pObjCalendar.GetFiscalYear(m_dtEndDate, out FY)) ||
		        !(hr = FY.GetPeriod(m_dtEndDate, out pObjPeriod)) )
                return hr;

            dtEDate = FY.YREndDate;
	        dtSDate = FY.YRStartDate;
	        eCType = FY.CycleType;

	        dtPSDate = pObjPeriod.PeriodStart;
	        dtPEDate = pObjPeriod.PeriodEnd;

            //    if( m_dtEndDate > dtEDate || m_dtEndDate < dtSDate )
            //		return S_FALSE;
    
            if( eCType == ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_MONTHLY )
	        {
                if( !(hr = FY.GetPreviousPeriodWeights(m_dtEndDate, out iRemSegTPWeight)) ||
			        !(hr = FY.GetCurrentPeriodWeight(m_dtEndDate, out iFraSegTPWeight)) )
			        return hr;
        
                dblFraction = 0.5 * (double)(iFraSegTPWeight) /
                                ((double)(iRemSegTPWeight) + (double)(iFraSegTPWeight) * 0.5);
                dtFraSegStartDate = dtPSDate;
                dtFraSegEndDate = dtPEDate;
                dtRemSegStartDate = dtSDate;
                dtRemSegEndDate = dtFraSegStartDate.AddDays (- 1);
            }
	        else
	        {
                iDays = (int)((m_dtEndDate - dtPSDate).TotalDays) + 1;
                iTotalDays = (int)((dtPEDate - dtPSDate).TotalDays) + 1;
        
                if( !(hr = FY.GetPreviousPeriodWeights(m_dtEndDate, out iRemSegTPWeight)) ||
			        !(hr = FY.GetCurrentPeriodWeight(m_dtEndDate, out iFraSegTPWeight)) )
			        return hr;
        
                dblFraction = ((double)(iFraSegTPWeight) * (double)(iDays) / (double)(iTotalDays)) / 
                              ((double)(iRemSegTPWeight) + (double)(iFraSegTPWeight) * (double)(iDays) / 
                              (double)(iTotalDays));
                dtFraSegStartDate = dtPSDate;
                dtFraSegEndDate = dtPEDate;
                dtRemSegStartDate = dtSDate;
                dtRemSegEndDate = dtFraSegStartDate.AddDays (- 1);
            }
            return true;
        }

        public bool MonthBased
        {
            get { return true; }
        }

        public short DetermineTablePeriod
        {
            get { throw new NotImplementedException(); }
        }
    }
}
