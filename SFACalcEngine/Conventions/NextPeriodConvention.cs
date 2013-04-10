using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFACalendar;

namespace SFACalcEngine
{
    class NextPeriodConvention : IBAAvgConvention
    {
        IBACalendar m_pObjCalendar;
        DateTime m_dtStartDate;
        DateTime m_dtEndDate;
        DateTime m_dtPISDate;
        double m_dblLife;
        short m_iPdNum;

        public NextPeriodConvention()
        {

        }

        public bool Initialize(SFACalendar.IBACalendar calendar, DateTime PlacedInService, double Life)
        {
            int iYear;
            int iMonth;
            int iDay;
            IBACalcPeriod pObjPeriod; 
            IBAFiscalYear FY;
            bool hr;
            DateTime dtTmpEndDate;
            DateTime dtTmpStartDate;


            if (calendar == null || PlacedInService <= DateTime.MinValue || Life <= 0)
                return false;

            m_pObjCalendar = null;
            m_pObjCalendar = calendar;
            m_dtPISDate = PlacedInService;
            m_dblLife = Life;

 	        if ( !(hr = m_pObjCalendar.GetFiscalYear(m_dtPISDate, out FY)) ||
		         !(hr = FY.GetPeriod(m_dtPISDate, out pObjPeriod)) )
                return hr;
	        dtTmpStartDate = FY.YRStartDate;
 	        dtTmpEndDate = FY.YREndDate;
            //deemed start date
	        m_dtStartDate = pObjPeriod.PeriodEnd;
            m_dtStartDate = m_dtStartDate.AddDays(1);
    
            //used the deemed start date to calc deemed end date
            iYear = m_dtStartDate.Year + (int)(m_dblLife);
            iMonth = m_dtStartDate.Month + (int)((m_dblLife - (int)(m_dblLife)) * 12);
            iDay = m_dtStartDate.Day;

            m_dtEndDate = new DateTime(iYear, iMonth, iDay).AddDays(-1);

            return true;
        }

        public bool GetFirstYearFactor(DateTime dtDate, out double pVal)
        {
            short iPeriods;
            short iCurWeight;
            short iAnuWeight;
            DateTime dtTmpEndDate;
            DateTime dtTmpStartDate;
            IBAFiscalYear FY;
            bool hr;
            pVal = 0.0;

            if (m_pObjCalendar == null)
                throw new Exception("Avg Convention not initialized.");
            if (dtDate <= DateTime.MinValue)
                throw new Exception("Avg Convention not initialized.");

	        if ( !(hr = m_pObjCalendar.GetFiscalYear(dtDate, out FY)) ||
  		         !(hr = FY.GetRemainingPeriodWeights(dtDate, out iPeriods)) ||
		         !(hr = FY.GetCurrentPeriodWeight(dtDate, out iCurWeight)) ||
		         !(hr = FY.GetTotalAnnualPeriodWeights(out iAnuWeight)) )
              return hr;
            dtTmpStartDate = FY.YRStartDate;
            dtTmpEndDate = FY.YREndDate;

            pVal = ((double)(iPeriods) + (double)(iCurWeight)) / iAnuWeight;

            return true;
        }

        public bool GetLastYearFactor(double RemainingLife, DateTime dtDate, out double pVal)
        {
            IBAFiscalYear FY;
            IBACalcPeriod pObjIPd1;
            IBACalcPeriod pObjIPd2;
            DateTime dtTmpEndDate;
            DateTime dtTmpStartDate;
            short iPeriodNum1;
            short iPeriodNum2;
            double dFYFactor1;
            double dFYFactor2;
            bool hr;
            pVal = 0.0;

            if (m_pObjCalendar == null)
                throw new Exception("Avg Convention not initialized.");

            if (dtDate <= DateTime.MinValue)
            {
                dtDate = m_dtEndDate;
            }

            if (dtDate < m_dtStartDate)
            {
                dtDate = m_dtStartDate;
            }

            if (!(hr = m_pObjCalendar.GetFiscalYear(dtDate, out FY)) )
                return hr;
            dtTmpStartDate = FY.YRStartDate;
            dtTmpEndDate = FY.YREndDate;
    
            if(dtDate >= dtTmpStartDate && dtDate <= dtTmpEndDate && 
                m_dtStartDate >= dtTmpStartDate && m_dtStartDate <= dtTmpEndDate )
	        {
                //in the same year as first year
                if ( !(hr = FY.GetPeriod(m_dtStartDate, out pObjIPd1)) ||
			         !(hr = FY.GetPeriod(dtDate, out pObjIPd2)) )
                    return hr;
		        iPeriodNum1 = pObjIPd1.PeriodNum;
                iPeriodNum2 = pObjIPd2.PeriodNum;
                if(iPeriodNum1 == iPeriodNum2 )
		        {
                    //in the same month as the first month
			        if ( !(hr = GetFirstYearFactor(dtDate, out pVal)) )
				        return hr;
                }
		        else
		        {
                    //in diff month
			        if ( !(hr = GetFirstYearFactor(m_dtStartDate, out dFYFactor1)) ||
				         !(hr = GetFirstYearFactor(dtDate, out dFYFactor2)) )
				        return hr;
                    pVal = dFYFactor1 - dFYFactor2;
                }
	        }
            else
	        {
                IBACalcPeriod pObjPeriod;
                DateTime periodEnd;


                if (!(hr = FY.GetPeriod(dtDate, out pObjPeriod)) ||
                     !(hr = GetFirstYearFactor(dtDate, out dFYFactor1)))
			        return hr;
                periodEnd = pObjPeriod.PeriodEnd;

                if (periodEnd >= dtTmpEndDate)
                {
                    dFYFactor2 = 0;
                }
                else
                {
                    if (!(hr = GetFirstYearFactor(periodEnd.AddDays (+ 1), out dFYFactor2)))
                        return hr;
                }

                pVal =dFYFactor1 - dFYFactor2;
            }
            return true;
        }

        public bool GetDisposalYearFactor(double RemainingLife, DateTime dtDate, out double pVal)
        {
            IBACalcPeriod pObjIPd1;
            IBACalcPeriod pObjIPd2;
            IBAFiscalYear FY;
            DateTime dtTmpEndDate;
            DateTime dtTmpStartDate;
            short iPeriodNum1;
            short iPeriodNum2;
            double dFYFactor1;
            double dFYFactor2;
            bool hr;
            pVal = 0;

            if (m_pObjCalendar == null)
                throw new Exception("Avg Convention not initialized.");

            if (dtDate <= DateTime.MinValue)
            {
                dtDate = m_dtEndDate;
            }

            if (dtDate < m_dtStartDate)
            {
                dtDate = m_dtStartDate;
            }

            if (!(hr = m_pObjCalendar.GetFiscalYear(dtDate, out FY)))
                return hr;
            dtTmpStartDate = FY.YRStartDate;
            dtTmpEndDate = FY.YREndDate;

            if (dtDate >= dtTmpStartDate && dtDate <= dtTmpEndDate &&
                m_dtStartDate >= dtTmpStartDate && m_dtStartDate <= dtTmpEndDate)
            {
                //in the same year as first year
                if (!(hr = FY.GetPeriod(m_dtStartDate, out pObjIPd1)) ||
                    !(hr = FY.GetPeriod(dtDate, out pObjIPd2)) )
                    return hr;
                iPeriodNum1 = pObjIPd1.PeriodNum;
                iPeriodNum2 = pObjIPd2.PeriodNum;
                    
                if (iPeriodNum1 == iPeriodNum2)
                {
                    //in the same month as the first month
                    if (!(hr = GetFirstYearFactor(dtDate, out pVal)))
                        return hr;
                }
                else
                {
                    //in diff month
                    if (m_dtStartDate == dtTmpStartDate)
                        dFYFactor1 = 1;
                    else if (!(hr = GetFirstYearFactor(m_dtStartDate.AddDays (- 1), out dFYFactor1)))
                        return hr;

                    if (!(hr = GetFirstYearFactor(dtDate, out dFYFactor2)))
                        return hr;
                    pVal = dFYFactor1 - dFYFactor2;
                }
            }
            else
            {
                IBACalcPeriod pObjPeriod;
                DateTime periodEnd;

                if ( !(hr = FY.GetPeriod(dtDate, out pObjPeriod)) ||
                     !(hr = FY.GetFiscalYearFraction(out dFYFactor1)))
                    return hr;

                periodEnd = pObjPeriod.PeriodEnd;
               if (periodEnd >= dtTmpEndDate)
                {
                    dFYFactor2 = 0;
                }
                else
                {
                    if (!(hr = GetFirstYearFactor(periodEnd.AddDays(+ 1), out dFYFactor2)))
                        return hr;
                }

                //		if ( dFYFactor1 >= RemainingLife && RemainingLife > 0.01 )
                //		{
                //			pVal = (dFYFactor1 - dFYFactor2) / RemainingLife; 
                //		}
                //		else
                //		{
                pVal = dFYFactor1 - dFYFactor2;
                //		}
            }
            return true;
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
            DateTime dtSDate;
            DateTime dtEDate;
            DateTime dtPSDate;
            DateTime dtPEDate;
            IBACalcPeriod pObjPeriod;
            IBAFiscalYear pObjIFY;
            bool hr;
            pVal = false;

            if (m_pObjCalendar == null)
                throw new Exception("Avg Convention not initialized.");

            if (!(hr = m_pObjCalendar.GetFiscalYear(m_dtPISDate, out pObjIFY)) ||
                !(hr = pObjIFY.GetPeriod(m_dtPISDate, out pObjPeriod)) )
                return hr;
            dtEDate = pObjIFY.YREndDate;
            dtSDate = pObjIFY.YRStartDate;
            dtPSDate = pObjPeriod.PeriodStart;
            dtPEDate = pObjPeriod.PeriodEnd;
            iFraSegTPWeight = pObjPeriod.Weight;
                return hr;

            if (m_dtStartDate >= dtSDate && m_dtStartDate <= dtEDate &&
                 m_dtEndDate >= dtSDate && m_dtEndDate <= dtEDate)
            {
                dtEDate = m_dtEndDate;
            }

            dblFraction = 0;
            dtFraSegStartDate = dtPSDate;
            dtFraSegEndDate = dtPEDate;
            if (m_dtStartDate > dtEDate)
            {
                dtRemSegStartDate = DateTime.MinValue;
                dtRemSegEndDate = DateTime.MinValue;
                iRemSegTPWeight = 0;
            }
            else
            {
                dtRemSegStartDate = dtPEDate.AddDays (+ 1);
                dtRemSegEndDate = dtEDate;
                if (!(hr = pObjIFY.GetPeriodWeights(dtRemSegStartDate, dtRemSegEndDate, out  iRemSegTPWeight)))
                    return hr;
            } 
            return true;
        }

        public bool GetLastYearSegmentInfo(ref double dblFraction, ref DateTime dtFraSegStartDate, ref DateTime dtFraSegEndDate, ref short iFraSegTPWeight, ref DateTime dtRemSegStartDate, ref DateTime dtRemSegEndDate, ref short iRemSegTPWeight, out bool pVal)
        {
            DateTime dtSDate;
            DateTime dtEDate;
            DateTime dtPSDate;
            DateTime dtPEDate;
            IBACalcPeriod pObjPeriod;
            IBAFiscalYear pObjIFY;
            bool hr;
            pVal = false;

            if (m_pObjCalendar == null)
                throw new Exception("Avg Convention not initialized.");

            if (!(hr = m_pObjCalendar.GetFiscalYear(m_dtEndDate, out pObjIFY)) ||
                 !(hr = pObjIFY.GetPeriod(m_dtEndDate, out pObjPeriod)) )
                return hr;
            dtEDate = pObjIFY.YREndDate;
            dtSDate = pObjIFY.YRStartDate;
            dtPSDate = pObjPeriod.PeriodStart;
            dtPEDate = pObjPeriod.PeriodEnd;
            iFraSegTPWeight = pObjPeriod.Weight;

            //    if( m_dtEndDate > dtEDate || m_dtEndDate < dtSDate )
            //		return S_FALSE;

            if (!(hr = pObjIFY.GetPeriodWeights(dtSDate, dtPEDate, out iFraSegTPWeight)))
                return hr;

            iRemSegTPWeight = 0;
            dblFraction = 1.0;
            dtFraSegStartDate = dtSDate;
            dtFraSegEndDate = dtPEDate;
            dtRemSegStartDate = dtSDate;
            dtRemSegEndDate = dtPEDate; 
            return true;
        }

        public bool MonthBased
        {
            get { return false; }
        }

        public short DetermineTablePeriod
        {
            get { throw new NotImplementedException(); }
        }
    }
}
