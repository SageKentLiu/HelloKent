using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFACalendar;

namespace SFACalcEngine
{
    class MidPeriodConvention : IBAAvgConvention
    {
        IBACalendar m_pObjCalendar;
        DateTime m_dtStartDate;
        DateTime m_dtEndDate;
        DateTime m_dtPISDate;
        double m_dblLife;

        public MidPeriodConvention()
        {

        }

        public bool Initialize(SFACalendar.IBACalendar calendar, DateTime PlacedInService, double Life)
        {
            int iYear;
            int iMonth;
            int iDay;
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

 	        if ( !(hr = m_pObjCalendar.GetFiscalYear(m_dtPISDate, out FY)) )
		        return hr;
            dtTmpStartDate = FY.YRStartDate;
 	        dtTmpEndDate = FY.YREndDate;

    
            //deemed start date
            if ( !(hr = FY.GetMidPeriodDate(m_dtPISDate, out m_dtStartDate)) )
		        return hr;
   
            //use the deemed start date to calculate the deemed end date
            iYear = m_dtStartDate.Year + (int)(m_dblLife);
            iMonth = m_dtStartDate.Month + (int)((m_dblLife - (int)(m_dblLife)) * 12);
            iDay = m_dtStartDate.Day;
    
            //deemed end date
	        if ( iMonth > 12 )
	        {
		        iMonth -= 12;
		        iYear ++;
	        }
            m_dtEndDate = new DateTime (iYear, iMonth, iDay).AddDays (- 1);
    
            //    m_dtStartDate = m_dtPISDate;
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


	        if ( !(hr = m_pObjCalendar.GetFiscalYear(dtDate, out FY)) )
                return hr;
	        dtTmpStartDate = FY.YRStartDate;
 	        dtTmpEndDate = FY.YREndDate;
            if ( !(hr = FY.GetRemainingPeriodWeights(dtDate, out iPeriods)) ||
    	         !(hr = FY.GetCurrentPeriodWeight(dtDate, out iCurWeight)) ||
    	         !(hr = FY.GetTotalAnnualPeriodWeights(out iAnuWeight)) )
		        return hr;
    
            //the current period get half of its weight
            pVal = ((double)(iPeriods) + 0.5 * (double)(iCurWeight)) / iAnuWeight;

            return true;
        }

        public bool GetLastYearFactor(double RemainingLife, DateTime dtDate, out double pVal)
        {
            IBAFiscalYear FY;
            IBACalcPeriod pObjIPd1;
            IBACalcPeriod pObjIPd2;
            DateTime dtTmpEndDate;
            DateTime dtTmpStartDate;
            double dFYFactor1;
            double dFYFactor2;
            short iPeriodNum1;
            short iPeriodNum2;
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
            if (dtDate < m_dtStartDate)
            {
                dtDate = m_dtStartDate;
            }

            if (!(hr = m_pObjCalendar.GetFiscalYear(dtDate, out FY)) )
                return hr;

            dtTmpStartDate = FY.YRStartDate;
            dtTmpEndDate = FY.YREndDate;

            if (dtDate >= dtTmpStartDate && dtDate <= dtTmpEndDate &&
                m_dtStartDate >= dtTmpStartDate && m_dtStartDate <= dtTmpEndDate)
            {
                //the dispdate is in the first year
                if (!(hr = FY.GetPeriod(m_dtStartDate, out pObjIPd1)) ||
                     !(hr = FY.GetPeriod(dtDate, out pObjIPd2)) )
                    return hr;
                iPeriodNum1 = pObjIPd1.PeriodNum;
                iPeriodNum2 = pObjIPd2.PeriodNum;
                   
                if (iPeriodNum1 == iPeriodNum2)
                {
                    //if in the same m_onth, the factor should be the same
                    pVal = 0;
                    //            if ( FAILED(hr = GetFirstYearFactor(dtDate, pVal)) )
                    //				return hr;
                }
                else
                {
                    //if in the diff m_onth, find the difference
                    //
                    //-+---------------+------------------------------|------.
                    // startdate      dispdate                      year end
                    // |<-- the diff ->|
                    // "the dif" x "anual amount" is the amt for this period
                    if (!(hr = GetFirstYearFactor(m_dtStartDate, out dFYFactor1)) ||
                         !(hr = GetFirstYearFactor(dtDate, out dFYFactor2)))
                        return hr;
                    pVal = dFYFactor1 - dFYFactor2;
                }
            }
            else
            {
                //in diff year
                if (!(hr = FY.GetFiscalYearFraction(out dFYFactor1)) ||
                     !(hr = GetFirstYearFactor(dtDate, out dFYFactor2)))
                    return hr;
                pVal = dFYFactor1 - dFYFactor2;
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
            DateTime dtTmpEndDate;
            DateTime dtTmpStartDate;
            IBACalcPeriod pObjPeriod;
            IBAFiscalYear FY;
            bool hr;
            pVal = false;

            if (m_pObjCalendar == null)
                throw new Exception("Avg Convention not initialized.");

  	        if ( !(hr = m_pObjCalendar.GetFiscalYear(m_dtPISDate, out FY)))
                return hr;
	        dtTmpStartDate = FY.YRStartDate;
 	        dtTmpEndDate = FY.YREndDate;

            if ( !(hr = FY.GetPeriod(m_dtPISDate, out pObjPeriod)) ||
                !(hr = FY.GetCurrentPeriodWeight(m_dtPISDate, out iFraSegTPWeight)) )
                return hr;

	        dtFraSegStartDate = pObjPeriod.PeriodStart;
	        dtFraSegEndDate = pObjPeriod.PeriodEnd;

	        if ( m_dtStartDate >= dtTmpStartDate && m_dtStartDate <= dtTmpEndDate &&
		         m_dtEndDate >= dtTmpStartDate && m_dtEndDate <= dtTmpEndDate )
	        {
		        dtTmpEndDate = m_dtEndDate;
	        }
	        dtRemSegEndDate = dtTmpEndDate;

	        if ( dtFraSegEndDate == dtTmpEndDate )
	        {
		        iRemSegTPWeight = 0;

		        dblFraction = 1;

		        dtRemSegStartDate = dtTmpEndDate;
		        dtRemSegEndDate = dtTmpEndDate;
	        }
	        else
	        {
		        if ( !(hr = FY.GetPeriodWeights(dtFraSegEndDate.AddDays (+1), dtTmpEndDate, out iRemSegTPWeight)) )
			        return hr;

		        dblFraction = 0.5 * (iFraSegTPWeight) / ((double)(iRemSegTPWeight) + 0.5 * (iFraSegTPWeight));
		        dtRemSegStartDate = dtFraSegEndDate.AddDays (1);
	        }
            pVal = true;
            return true;
        }

        public bool GetLastYearSegmentInfo(ref double dblFraction, ref DateTime dtFraSegStartDate, ref DateTime dtFraSegEndDate, ref short iFraSegTPWeight, ref DateTime dtRemSegStartDate, ref DateTime dtRemSegEndDate, ref short iRemSegTPWeight, out bool pVal)
        {
            DateTime dtTmpEndDate;
            DateTime dtTmpStartDate;
            IBACalcPeriod pObjperiod;
            IBAFiscalYear FY;
            bool hr;
            pVal = false;

            if (m_pObjCalendar == null)
                throw new Exception("Avg Convention not initialized.");

	        if ( !(hr = m_pObjCalendar.GetFiscalYear(m_dtEndDate, out FY)) ||
    	         !(hr = FY.GetPeriod(m_dtEndDate, out pObjperiod)) ||
     	         !(hr = FY.GetPreviousPeriodWeights(m_dtEndDate, out iRemSegTPWeight)) ||
    	         !(hr = FY.GetCurrentPeriodWeight(m_dtEndDate, out iFraSegTPWeight)) )
               return hr;
	        dtTmpStartDate = FY.YRStartDate;
 	        dtTmpEndDate = FY.YREndDate;
            dtFraSegStartDate = pObjperiod.PeriodStart;
            dtFraSegEndDate = pObjperiod.PeriodEnd ;
            dtRemSegStartDate = FY.YRStartDate;
    
            dblFraction = 0.5 * (double)(iFraSegTPWeight) / ((double)(iRemSegTPWeight) + (double)(iFraSegTPWeight) * 0.5);
            dtRemSegEndDate = dtFraSegStartDate.AddDays (- 1);
            pVal = true;
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
