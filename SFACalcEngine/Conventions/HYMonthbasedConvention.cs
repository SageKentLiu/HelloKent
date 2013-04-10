using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFACalendar;

namespace SFACalcEngine
{
    public class HYMonthbasedConvention : IBAAvgConvention
    {
        IBACalendar m_pObjCalendar;
        DateTime m_dtStartDate;
        DateTime m_dtEndDate;
        DateTime m_dtPISDate;
        double m_dblLife;
        MonthlyYear m_pMonthlyYear;

        public HYMonthbasedConvention()
        {
        }

        public bool Initialize(IBACalendar calendar, DateTime PlacedInService, double Life)
        {
            int					iYear;
            int					iMonth;
            int					iDay;
            DateTime            dtSDate;
            DateTime            dtEDate;
	        IBAFiscalYear       FY;
	        bool				hr;
    
            if ( calendar == null || PlacedInService <= DateTime.MinValue || Life < 1 )
		        return false;

	        m_pObjCalendar = null;
	        m_pObjCalendar = calendar;
	        m_dtPISDate = PlacedInService;
	        m_dblLife = Life;

	        m_pObjCalendar.GetFiscalYear(m_dtPISDate, out FY);
	        dtSDate = FY.YRStartDate;
 	        dtEDate = FY.YREndDate;

	        if ( m_pMonthlyYear == null )
		        m_pMonthlyYear = new MonthlyYear();
            m_pMonthlyYear.FiscalYearInfo = FY;
            m_pMonthlyYear.DeemedFYDates();
    
            //calc the deemed start date
            FY.GetMidYearDate(out m_dtStartDate);
    
            //calc the deemed end date
            iYear = m_dtStartDate.Year + ((int)(m_dblLife));
            iMonth = m_dtStartDate.Month + Convert.ToInt32((m_dblLife - ((int)(m_dblLife))) * 12);
            iDay = m_dtStartDate.Day;
    
            m_dtEndDate = new DateTime(iYear, iMonth, iDay).AddDays(- 1);
   
            //    m_dtStartDate = m_dtPISDate;
	        return true;
        }

        public bool GetFirstYearFactor(DateTime dtDate, out double pVal)
        {
            DateTime dtSDate;
            DateTime dtEDate;
            DateTime dtMidDate;
	        IBAFiscalYear FY;
	        bool hr;
            pVal = 0;

            if (m_pObjCalendar == null)
                throw new Exception("Avg Convention not initialized.");
            if (dtDate <= DateTime.MinValue)
                throw new Exception("Avg Convention not initialized."); 

	        if( !(hr = m_pObjCalendar.GetFiscalYear(dtDate, out FY)) )
                return false;
	        dtEDate = FY.YREndDate;
		    dtSDate = FY.YRStartDate;

            m_pMonthlyYear.FiscalYearInfo = FY;
	        m_pMonthlyYear.DeemedFYDates();
	        dtMidDate = m_pMonthlyYear.GetMidYearDate();
            pVal = m_pMonthlyYear.GetFirstYearFactor(dtMidDate);

	        return true;
        }


        public bool GetLastYearFactor(double RemainingLife, DateTime dtDate, out double pVal)
        {
	        DateTime			dtSDate;
	        DateTime			dtEDate;
	        DateTime			dtPEDate;
	        bool			    hr;
            IBACalcPeriod		pObjIPd;
	        IBAFiscalYear       FY;
            pVal = 0;

            if (m_pObjCalendar == null)
                throw new Exception("Avg Convention not initialized.");

            if( dtDate <= DateTime.MinValue )
                dtDate = m_dtEndDate;
    
            if( dtDate < m_dtStartDate )
                dtDate = m_dtStartDate;
    
	        if( !(hr = m_pObjCalendar.GetFiscalYear(dtDate, out FY)) )
                return hr;
	        dtEDate = FY.YREndDate;
            dtSDate = FY.YRStartDate;

            if( dtDate >= dtSDate && dtDate <= dtEDate &&
                m_dtStartDate >= dtSDate && m_dtStartDate <= dtEDate )
	        {
		        if( !(hr = FY.GetPeriod(m_dtStartDate, out pObjIPd)))
                    return hr;
                dtPEDate = pObjIPd.PeriodEnd;

                if( dtDate < dtPEDate )
                    return GetFirstYearFactor(dtDate, out pVal);
		        else
                    pVal = 0;
            }
	        else
                return GetFirstYearFactor(dtDate, out pVal);

	        return true;
        }

        public bool GetDisposalYearFactor(double RemainingLife, DateTime dtDate, out double pVal)
        {
	        DateTime						dtSDate;
	        DateTime						dtEDate;
        //	DateTime						dtPEDate;
	        bool						    hr;
            IBACalcPeriod           		pObjIPd;
	        IBAFiscalYear                   FY;

            pVal = 0;

            if (m_pObjCalendar == null)
                throw new Exception("Avg Convention not initialized.");

            if( dtDate <= DateTime.MinValue )
                dtDate = m_dtEndDate;
    
            if( dtDate < m_dtStartDate )
                dtDate = m_dtStartDate;
    
	        if( !(hr = m_pObjCalendar.GetFiscalYear(dtDate, out FY)))
                return hr;
	        dtEDate = FY.YREndDate;
            dtSDate = FY.YRStartDate;

            if( dtDate >= dtSDate && dtDate <= dtEDate &&
                m_dtStartDate >= dtSDate && m_dtStartDate <= dtEDate )
	        {
        //RDBJ 		if( FAILED(hr = FY.GetPeriod(m_dtStartDate, out pObjIPd)) ||
        //RDBJ 			FAILED(hr = pObjIPd.get_PeriodEnd(&dtPEDate)) )
        //RDBJ 			return hr;
        //RDBJ 
        //RDBJ         if( dtDate < dtPEDate )
        //RDBJ             return GetFirstYearFactor(dtDate, pVal);
        //RDBJ 		else
                    pVal = 0;
            }
	        else
	        {
		        DateTime                        dtMidDate;

        //		if( FAILED(hr = FY.GetMidYearDate(&dtMidDate)) )
        //			return hr;
        //
		        m_pMonthlyYear.FiscalYearInfo = FY;
		        m_pMonthlyYear.DeemedFYDates();
		        dtMidDate = m_pMonthlyYear.GetMidYearDate();
		        if ( dtDate > m_dtEndDate && m_dtEndDate >= dtSDate && m_dtEndDate <= dtEDate )
			        // last year and after deemed end date
			        pVal = m_pMonthlyYear.GetLastYearFactor(dtEDate.AddDays (1));
		        else if ( m_dtEndDate >= dtSDate && m_dtEndDate <= dtEDate )
			        // last year before or on deemed end date
			        pVal = RemainingLife * 0.5;
		        else
			        // middle year
			        pVal = m_pMonthlyYear.GetLastYearFactor(dtMidDate);

		        return true;
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
            get { return 0; }
        }
    }
}
