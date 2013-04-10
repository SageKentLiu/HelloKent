using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFACalendar;

namespace SFACalcEngine
{
    class FYMonthBasedConvention : IBAAvgConvention
    {
        IBACalendar m_pObjCalendar;
        DateTime m_dtStartDate;
        DateTime m_dtEndDate;
        DateTime m_dtPISDate;
        double m_dblLife;
        MonthlyYear m_pMonthlyYear;

        public FYMonthBasedConvention()
        {
        }

        public bool Initialize(IBACalendar calendar, DateTime PlacedInService, double Life)
        {
            int iYear = 0;
            int iMonth = 0;
            int iDay = 0;
            DateTime dtSDate;
            DateTime dtEDate;
            IBAFiscalYear FY;
            short iFYNum;

            if (calendar == null || PlacedInService <= DateTime.MinValue || Life < 1)
                return false;

            m_pObjCalendar = null;
            m_pObjCalendar = calendar;
            m_dtPISDate = PlacedInService;
            m_dblLife = Life;

            m_pObjCalendar.GetFiscalYear(m_dtPISDate, out FY);
            dtSDate = FY.YRStartDate;
            dtEDate = FY.YREndDate;
            iFYNum = FY.FYNum;

            if (m_pMonthlyYear == null)
                m_pMonthlyYear = new MonthlyYear();
 	        //
	        // Take the first day of the year and make it the deemed start date.
	        // But first true it up to a month start.
	        //
	        m_dtStartDate = dtSDate;
	        if ( dtSDate.Day > 7 )
	        {
		        iYear = m_dtStartDate.Year;
		        iMonth = m_dtStartDate.Month;
		        if ( iMonth > 11 )
		        {
			        iMonth = 1;
			        iYear = iYear + 1;
		        }
		        else
			        iMonth++;
		        m_dtStartDate = new DateTime(iYear, iMonth, 1);
	        }
	        else
	        {
		        m_dtStartDate = new DateTime(m_dtStartDate.Year, m_dtStartDate.Month, 1);
	        }

            //calc the deemed end date
            iYear = m_dtStartDate.Year + ((int)(m_dblLife));
            iMonth = m_dtStartDate.Month + ((int)(m_dblLife - (int)(m_dblLife)) * 12);
            iDay = m_dtStartDate.Day;

	        // adjust for special start of business case where deemed start is before start of bus.
	        if ( iFYNum == 1 && m_dtStartDate < dtSDate )
		        m_dtStartDate = dtSDate;

            m_dtEndDate = new DateTime(iYear, iMonth, iDay).AddDays (- 1);
            return true;
        }

        public bool GetFirstYearFactor(DateTime dtDate, out double pVal)
        {
            IBAFiscalYear pObjIFY;
            double dFraction;
            bool hr;

            pVal = 0;

            if (!(hr = m_pObjCalendar.GetFiscalYear(dtDate, out pObjIFY)) ||
                 !(hr = pObjIFY.GetFiscalYearFraction(out dFraction)))
                return hr;
            pVal = dFraction;

            return true;
        }


        public bool GetLastYearFactor(double RemainingLife, DateTime dtDate, out double pVal)
        {
            IBAFiscalYear pObjIFY;
            double dFraction;
            bool hr;

            pVal = 0;

            if ( !(hr = m_pObjCalendar.GetFiscalYear(dtDate, out pObjIFY)) ||
                 !(hr = pObjIFY.GetFiscalYearFraction(out dFraction)))
                return hr;
            pVal = dFraction;
            return true;
        }

        public bool GetDisposalYearFactor(double RemainingLife, DateTime dtDate, out double pVal)
        {
            pVal = 0;
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
            pVal = false;
            return true;
        }


        public bool GetFirstYearSegmentInfo(ref double dblFraction, ref DateTime dtFraSegStartDate, ref DateTime dtFraSegEndDate, ref short iFraSegTPWeight, ref DateTime dtRemSegStartDate, ref DateTime dtRemSegEndDate, ref short iRemSegTPWeight, out bool pVal)
        {
            pVal = false;
            return false;
        }

        public bool GetLastYearSegmentInfo(ref double dblFraction, ref DateTime dtFraSegStartDate, ref DateTime dtFraSegEndDate, ref short iFraSegTPWeight, ref DateTime dtRemSegStartDate, ref DateTime dtRemSegEndDate, ref short iRemSegTPWeight, out bool pVal)
        {
            pVal = false;
            return false;
        }

        public bool MonthBased
        {
            get { return true; }
        }

        public short DetermineTablePeriod
        {
            get { return 1; }
        }
    }
}
