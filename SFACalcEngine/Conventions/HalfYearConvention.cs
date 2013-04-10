using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFACalendar;

namespace SFACalcEngine
{
    public class HalfYearConvention : IBAAvgConvention
    {
        IBACalendar m_pObjCalendar;
        DateTime m_dtStartDate;
        DateTime m_dtEndDate;
        DateTime m_dtPISDate;
        double m_dblLife;
        MonthlyYear m_pMonthlyYear;
        short m_qtrNumber;

        public HalfYearConvention()
        {
        }

        public bool Initialize(IBACalendar calendar, DateTime PlacedInService, double Life)
        {
            int		iYear;
            int		iMonth;
            int		iDay;
	        DateTime   dtTmpEndDate;
	        DateTime   dtTmpStartDate;
	        IBAFiscalYear FY;

            if ( calendar == null || PlacedInService <= DateTime.MinValue || Life < 1 )
		        return false;

	        m_pObjCalendar = null;
	        m_pObjCalendar = calendar;
	        m_dtPISDate = PlacedInService;
	        m_dblLife = Life;

	        m_pObjCalendar.GetFiscalYear(m_dtPISDate, out FY);
	        dtTmpStartDate = FY.YRStartDate;
 	        dtTmpEndDate = FY.YREndDate;
            //calc the deemed start date
	         FY.GetMidYearDate(out m_dtStartDate);
    
            //calc the deemed end date
            iYear = m_dtStartDate.Year + ((int)(Life));
            iMonth = m_dtStartDate.Month + ((int)((Life - ((int)(Life))) * 12));
            iDay = m_dtStartDate.Day;
    
	        if ( iMonth > 12 )
	        {
		        iMonth -= 12;
		        iYear ++;
	        }
            m_dtEndDate = new DateTime(iYear, iMonth, iDay).AddDays(- 1);
            m_dtStartDate = m_dtPISDate;
	        return true;
        }

        public bool GetFirstYearFactor(DateTime dtDate, out double pVal)
        {
            short	iTolWeight;
            short	iAnuWeight;
            DateTime dtTmpEndDate;
            DateTime dtTmpStartDate;
	        IBAFiscalYear FY;
	        bool hr;
            pVal = 0.0;

	        if ( m_pObjCalendar == null )
                throw new Exception("Avg Convention not initialized.");
            if (dtDate <= DateTime.MinValue)
                throw new Exception("Avg Convention not initialized.");

            if (!(hr = m_pObjCalendar.GetFiscalYear(dtDate, out FY)))
            {
                return false;
            }
	        dtTmpStartDate = FY.YRStartDate;
 	        dtTmpEndDate = FY.YREndDate;
	        FY.GetTotalFiscalYearPeriodWeights(out iTolWeight);
            FY.GetTotalAnnualPeriodWeights(out iAnuWeight);
    
            pVal = 0.5 * iTolWeight / iAnuWeight;

	        return true;
        }

        public bool GetLastYearFactor(double RemainingLife, DateTime dtDate, out double pVal)
        {
            IBACalcPeriod pObjIPd;
            DateTime dtTmpEndDate;
            DateTime dtTmpStartDate;
            DateTime dtPEndDate;
	        IBAFiscalYear FY;
	        bool hr;
            pVal = 0.0;

            if (m_pObjCalendar == null)
                throw new Exception("Avg Convention not initialized.");

            if( dtDate <= DateTime.MinValue )
	        {
                dtDate = m_dtEndDate;
            }
    
            if( dtDate < m_dtStartDate )
	        {
                dtDate = m_dtStartDate;
            }

	        if ( !(hr = m_pObjCalendar.GetFiscalYear(dtDate, out FY)) )
                return false;
	        dtTmpStartDate = FY.YRStartDate;
            dtTmpEndDate = FY.YREndDate;

	        if( dtDate >= dtTmpStartDate && dtDate <= dtTmpEndDate &&
                m_dtStartDate >= dtTmpStartDate && m_dtStartDate <= dtTmpEndDate )
	        {
                //	  in the first year
                if ( !(hr = FY.GetPeriod(m_dtStartDate, out pObjIPd)))
                    return false;
                dtPEndDate = pObjIPd.PeriodEnd;
                if( dtDate < dtPEndDate )
		        {
                    return GetFirstYearFactor(dtDate, out pVal);
                }
		        else
		        {
                    pVal = 0;
                }
	        }
            else if( dtDate >= dtTmpStartDate && dtDate <= dtTmpEndDate &&
                m_dtEndDate >= dtTmpStartDate && m_dtEndDate <= dtTmpEndDate )
	        {
                // in the last year
                pVal = RemainingLife * 0.5;
            }
	        else
	        {
                return GetFirstYearFactor(dtDate, out pVal);
            }

	        return true;
        }

        public bool GetDisposalYearFactor(double RemainingLife, DateTime dtDate, out double pVal)
        {
            IBACalcPeriod pObjIPd;
            DateTime dtTmpEndDate;
            DateTime dtTmpStartDate;
	        IBAFiscalYear FY;
	        bool hr;
            pVal = 0.0;

	        if ( m_pObjCalendar == null )
                throw new Exception("Avg Convention not initialized.");

            if( dtDate <= DateTime.MinValue )
	        {
                dtDate = m_dtEndDate;
            }
    
            if( dtDate < m_dtStartDate )
	        {
                dtDate = m_dtStartDate;
            }

	        if ( !(hr = m_pObjCalendar.GetFiscalYear(dtDate, out FY)) )
                return false;
            dtTmpStartDate = FY.YRStartDate;
            dtTmpEndDate = FY.YREndDate;

	        if( dtDate >= dtTmpStartDate && dtDate <= dtTmpEndDate &&
                m_dtStartDate >= dtTmpStartDate && m_dtStartDate <= dtTmpEndDate )
	        {
                    pVal = 0;
	        }
            else if( dtDate >= dtTmpStartDate && dtDate <= dtTmpEndDate &&
                m_dtEndDate >= dtTmpStartDate && m_dtEndDate <= dtTmpEndDate )
	        {
                // in the last year
		        if ( dtDate > m_dtEndDate )
			        pVal = RemainingLife;
		        else
		        {
			        pVal = RemainingLife * 0.5;
		        }
            }
	        else
	        {
                return GetFirstYearFactor(dtDate, out pVal);
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
            get { return false; }
        }

        public short DetermineTablePeriod
        {
            get { return 0; }
        }
    }
}
