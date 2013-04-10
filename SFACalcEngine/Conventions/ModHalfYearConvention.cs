using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFACalendar;

namespace SFACalcEngine
{
    class ModHalfYearConvention: IBAAvgConvention
    {
        IBACalendar m_pObjCalendar;
        DateTime m_dtStartDate;
        DateTime m_dtEndDate;
        DateTime m_dtPISDate;
        double m_dblLife;
        bool m_bFirstHalf;

        public ModHalfYearConvention()
        {
        }

        public bool Initialize(IBACalendar calendar, DateTime PlacedInService, double Life)
        {
            int		    iYear;
            int		    iMonth;
            int		    iDay;
	        DateTime    dtTmpEndDate;
	        DateTime    dtTmpStartDate;
            DateTime    dtMidDate;
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
            FY.GetMidYearDate(out dtMidDate);

            //if in the first half get full year else move to next year
            if (m_dtPISDate < dtMidDate)
            {
                m_dtStartDate = dtTmpStartDate;
                m_bFirstHalf = true;
            }
            else
            {
                m_dtStartDate = dtTmpEndDate.AddDays (+ 1);
                m_bFirstHalf = false;
            }
    
            //calc the deemed end date
            iYear = m_dtStartDate.Year + ((int)(m_dblLife));
            iMonth = m_dtStartDate.Month + ((int)((m_dblLife - ((int)(m_dblLife))) * 12));
            iDay = m_dtStartDate.Day;
    
	        if ( iMonth > 12 )
	        {
		        iMonth -= 12;
		        iYear ++;
	        }
            m_dtEndDate = new DateTime(iYear, iMonth, iDay).AddDays(- 1);
	        return true;
        }

        public bool GetFirstYearFactor(DateTime dtDate, out double pVal)
        {
            short	iTolWeight;
            short	iAnuWeight;
            DateTime dtTmpEndDate;
            DateTime dtTmpStartDate;
            DateTime dtMidDate;
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
            FY.GetMidYearDate(out dtMidDate);
    
            if( dtDate < dtMidDate )
	        {
                //if the PIS is in the first half of the year, then 100%
                pVal = (double)(iTolWeight) / iAnuWeight;
            }
	        else
	        {
                //if( the PIS is in the second half of the year, then no disposed
                pVal = 0;
            }

	        return true;
        }

        public bool GetLastYearFactor(double RemainingLife, DateTime dtDate, out double pVal)
        {
            IBACalcPeriod pObjIPd;
            DateTime dtTmpEndDate;
            DateTime dtTmpStartDate;
            DateTime dtMidDate;
            DateTime dtPEndDate;
	        IBAFiscalYear FY;
            double dFYFraction;
            double dFYFactor; 
            bool hr;
            pVal = 0.0;

            if (m_pObjCalendar == null)
                throw new Exception("Avg Convention not initialized.");

            if( dtDate <= DateTime.MinValue )
	        {
                dtDate = m_dtEndDate;
            }


            if (!(hr = m_pObjCalendar.GetFiscalYear(dtDate, out FY)) ||
                !(hr = FY.GetMidYearDate(out dtMidDate)))
                return hr;
            dtTmpStartDate = FY.YRStartDate;
            dtTmpEndDate = FY.YREndDate;


            //    mobjIFY.GetPeriod mdtStartDate, objIPd
            if (m_bFirstHalf)
            {
                //if PIS is in the first half of the year
                if (dtDate >= dtTmpStartDate && dtDate <= dtTmpEndDate &&
                    m_dtStartDate >= dtTmpStartDate && m_dtStartDate <= dtTmpEndDate)
                {
                    if (!(hr = FY.GetPeriod(m_dtStartDate, out pObjIPd)) )
                        return hr;
                    dtPEndDate = pObjIPd.PeriodEnd;
                  
                    if (dtDate < dtPEndDate)
                    {
                        if (!(hr = GetFirstYearFactor(dtDate, out pVal)))
                            return hr;
                    }
                    else
                    {
                        if (!(hr = FY.GetFiscalYearFraction(out dFYFraction)) ||
                            !(hr = GetFirstYearFactor(dtDate, out dFYFactor)))
                            return hr;
                        pVal = 0.5 * (dFYFraction - dFYFactor);
                    }
                }
                else if (dtDate >= dtTmpStartDate && dtDate <= dtTmpEndDate &&
                    m_dtEndDate >= dtTmpStartDate && m_dtEndDate <= dtTmpEndDate)
                {
                    // in the last year
                    if (!(hr = FY.GetMidYearDate(out dtMidDate)))
                        return hr;
                    if (dtDate < dtMidDate)
                    {
                        pVal = 0;
                    }
                    else
                    {
                        pVal = RemainingLife * 0.5;
                    }
                }
                else
                {
                    if (!(hr = FY.GetFiscalYearFraction(out dFYFraction)) ||
                         !(hr = GetFirstYearFactor(dtDate, out dFYFactor)))
                        return hr;
                    pVal = 0.5 * (dFYFraction - dFYFactor);
                }
            }
            else
            {
                //if( PIS is in the second half of the year
                if (dtDate >= dtTmpStartDate && dtDate <= dtTmpEndDate &&
                    m_dtStartDate >= dtTmpStartDate && m_dtStartDate <= dtTmpEndDate)
                {
                    if (dtDate > dtMidDate)
                    {
                        if (!(hr = FY.GetFiscalYearFraction(out dFYFraction)) ||
                             !(hr = GetFirstYearFactor(dtMidDate, out dFYFactor)))
                            return hr;
                        pVal = dFYFraction - 0.5 * dFYFactor;
                    }
                    else
                        pVal = 0;
                }
                else if (dtDate >= dtTmpStartDate && dtDate <= dtTmpEndDate &&
                    m_dtEndDate >= dtTmpStartDate && m_dtEndDate <= dtTmpEndDate)
                {
                    // in the last year
                    if (!(hr = FY.GetMidYearDate(out dtMidDate)))
                        return hr;

                    if (dtDate < dtMidDate)
                    {
                        pVal = RemainingLife * 0.5;
                    }
                    else
                    {
                        pVal = RemainingLife;
                    }
                }
                else
                {
                    if (!(hr = FY.GetFiscalYearFraction(out dFYFraction)) ||
                        !(hr = GetFirstYearFactor(dtDate, out dFYFactor)))
                        return hr;
                    pVal = 0.5 * (dFYFraction - dFYFactor);
                }
            }

	        return true;
        }

        public bool GetDisposalYearFactor(double RemainingLife, DateTime dtDate, out double pVal)
        {
            IBACalcPeriod pObjIPd;
            IBAFiscalYear FY;
            DateTime dtTmpEndDate;
            DateTime dtTmpStartDate;
            DateTime dtMidDate;
            DateTime dtPEndDate;
            double dFYFraction;
            double dFYFactor;
	        bool hr;
            pVal = 0.0;

	        if ( m_pObjCalendar == null )
                throw new Exception("Avg Convention not initialized.");

            if( dtDate <= DateTime.MinValue )
	        {
                dtDate = m_dtEndDate;
            }

            if (!(hr = m_pObjCalendar.GetFiscalYear(dtDate, out FY)) ||
                !(hr = FY.GetMidYearDate(out dtMidDate)))
                return hr;
             dtTmpStartDate = FY.YRStartDate;
             dtTmpEndDate = FY.YREndDate;


            //    mobjIFY.GetPeriod mdtStartDate, objIPd
            if (m_bFirstHalf)
            {
                //if PIS is in the first half of the year
                if (dtDate >= dtTmpStartDate && dtDate <= dtTmpEndDate &&
                    m_dtStartDate >= dtTmpStartDate && m_dtStartDate <= dtTmpEndDate)
                {
                    // In first year
                    if (!(hr = FY.GetPeriod(m_dtStartDate, out pObjIPd)))
                          return hr;
                    dtPEndDate = pObjIPd.PeriodEnd;

                    if (dtDate < dtPEndDate)
                    {
                        if (!(hr = GetFirstYearFactor(dtDate, out pVal)))
                            return hr;
                    }
                    else
                    {
                        if (!(hr = FY.GetFiscalYearFraction(out dFYFraction)) ||
                            !(hr = GetFirstYearFactor(dtDate, out dFYFactor)))
                            return hr;
                        pVal = 0.5 * (dFYFraction - dFYFactor);
                    }
                }
                else //if( dtDate >= dtTmpStartDate && dtDate <= dtTmpEndDate && 
                //m_dtEndDate >= dtTmpStartDate && m_dtEndDate <= dtTmpEndDate )
                {
                    // in the last year
                    if (!(hr = FY.GetMidYearDate(out dtMidDate)))
                        return hr;
                    if (dtDate < dtMidDate)
                    {
                        pVal = 0;
                    }
                    else
                    {
                        if (!(hr = FY.GetFiscalYearFraction(out dFYFraction)))
                            return hr;
                        if (dFYFraction > RemainingLife)
                            pVal = RemainingLife * 0.5;
                        else
                            pVal = dFYFraction * 0.5;
                    }
                }
                //        else
                //		{
                //			if ( FAILED(hr = FY.GetFiscalYearFraction(out dFYFraction)) ||
                //				 FAILED(hr = GetFirstYearFactor(dtDate, out dFYFactor)) )
                //				return hr;
                //            pVal = 0.5 * (dFYFraction - dFYFactor);
                //        }
            }
            else // PIS in second half of the year.
            {
                if (dtDate < m_dtStartDate)
                {
                    // In PIS year
                    //			if ( dtDate > dtMidDate )
                    //			{
                    //				if ( FAILED(hr = FY.GetFiscalYearFraction(out dFYFraction)) ||
                    //					 FAILED(hr = GetFirstYearFactor(dtMidDate, out dFYFactor)) )
                    //					return hr;
                    //				pVal = dFYFraction - 0.5 * dFYFactor;
                    //			}
                    //			else
                    pVal = 0;
                }
                else //if( dtDate >= dtTmpStartDate && dtDate <= dtTmpEndDate && 
                //m_dtEndDate >= dtTmpStartDate && m_dtEndDate <= dtTmpEndDate )
                {
                    // in the last year
                    if ( !(hr = FY.GetFiscalYearFraction(out dFYFraction)) ||
                         !(hr = FY.GetMidYearDate(out dtMidDate)))
                        return hr;

                    if (dFYFraction > RemainingLife)
                    {
                        if (dtDate < dtMidDate)
                        {
                            pVal = RemainingLife * 0.5;
                        }
                        else
                        {
                            pVal = RemainingLife;
                        }
                    }
                    else
                    {
                        if (dtDate < dtMidDate)
                        {
                            pVal = dFYFraction * 0.5;
                        }
                        else
                        {
                            pVal = dFYFraction;
                        }
                    }
                }
                //else
                //{
                //	if ( FAILED(hr = FY.GetFiscalYearFraction(out dFYFraction)) ||
                //		 FAILED(hr = GetFirstYearFactor(dtDate, out dFYFactor)) )
                //		return hr;
                //    pVal = 0.5 * (dFYFraction - dFYFactor);
                //}
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
