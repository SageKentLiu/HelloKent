using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFACalendar;

namespace SFACalcEngine
{
    public class ModMidPeriodConvention : IBAAvgConvention
    {
        IBACalendar m_pObjCalendar;
        DateTime m_dtStartDate;
        DateTime m_dtEndDate;
        DateTime m_dtPISDate;
        double m_dblLife;

        public ModMidPeriodConvention()
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
            DateTime dtMidDate;
            bool bFirstHalf = false;

            if (calendar == null || PlacedInService <= DateTime.MinValue || Life <= 0)
                return false;

            m_pObjCalendar = null;
            m_pObjCalendar = calendar;
            m_dtPISDate = PlacedInService;
            m_dblLife = Life;

            m_pObjCalendar.GetFiscalYear(m_dtPISDate, out FY);
            dtTmpStartDate = FY.YRStartDate;
            dtTmpEndDate = FY.YREndDate;
            FY.GetPeriod(m_dtPISDate, out pObjPeriod);
            FY.GetMidPeriodDate(m_dtPISDate, out dtMidDate);

            //determine the pis in fisrt or second half of a month
            if (m_dtPISDate < dtMidDate)
            {
                bFirstHalf = true;
            }

            //calc the deemed start date
            if (bFirstHalf)
            {
                //place in first half(full month)
                m_dtStartDate = pObjPeriod.PeriodStart;
            }
            else
            {
                //place in second half(next month)
                m_dtStartDate = pObjPeriod.PeriodEnd;
                m_dtStartDate = m_dtStartDate.AddDays(1);
            }

            //calc the deemed end date
            iYear = (m_dtStartDate.Year) + (int)(m_dblLife);
            iMonth = (m_dtStartDate.Month) + (int)((m_dblLife - (int)(m_dblLife)) * 12);
            iDay = (m_dtStartDate.Day);

            //the deemed end date
            if (iMonth > 12)
            {
                iMonth -= 12;
                iYear++;
            }
            m_dtEndDate = new DateTime(iYear, iMonth, iDay).AddDays(-1);

            //    m_dtStartDate = m_dtPISDate;

            return true;
        }

        public bool GetFirstYearFactor(DateTime dtDate, out double pVal)
        {
            short iPeriods;
            short iRemPeriods = 0;
            short iCurWeight;
            short iAnuWeight;
            bool bFirstHalf = false;
            DateTime dtTmpEndDate;
            DateTime dtTmpStartDate;
            DateTime dtMidDate;
            IBAFiscalYear FY;
            bool hr;
            pVal = 0.0;

            if (m_pObjCalendar == null)
                throw new Exception("Avg Convention not initialized.");
            if (dtDate <= DateTime.MinValue)
                throw new Exception("Avg Convention not initialized.");


            if (!(hr = m_pObjCalendar.GetFiscalYear(dtDate, out FY)))
            {
                return false;
            }
            dtTmpStartDate = FY.YRStartDate;
            dtTmpEndDate = FY.YREndDate;


            FY.GetMidPeriodDate(dtDate, out dtMidDate);
            FY.GetRemainingPeriodWeights(dtDate, out iPeriods);
            FY.GetCurrentPeriodWeight(dtDate, out iCurWeight);
            FY.GetTotalAnnualPeriodWeights(out iAnuWeight);

            if (dtDate < dtMidDate)
            {
                bFirstHalf = true;
            }

            //if deemed start date and deemed end date fall in the same year 
            //(which happens only when life < 1year)
            //we need to fix first year factor by subtracting period weights
            //after deemed end period

            if (m_dtStartDate >= dtTmpStartDate && m_dtEndDate < dtTmpEndDate)
            {
                if (!(hr = FY.GetRemainingPeriodWeights(m_dtEndDate, out  iRemPeriods)))
                    return hr;
            }

            if (bFirstHalf)
            {
                pVal = ((double)(iPeriods) + (double)(iCurWeight) - (double)(iRemPeriods)) / iAnuWeight;
            }
            else
            {
                pVal = ((double)(iPeriods) - (double)(iRemPeriods)) / iAnuWeight;
            }
            return true;
        }

        public bool GetLastYearFactor(double RemainingLife, DateTime dtDate, out double pVal)
        {
            IBAFiscalYear FY;
            IBACalcPeriod pObjIPd1;
            IBACalcPeriod pObjIPd2;
            DateTime dtTmpEndDate;
            DateTime dtTmpStartDate;
            DateTime dtMidDate;
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

            if (!(hr = m_pObjCalendar.GetFiscalYear(dtDate, out FY)))
                return hr;

            dtTmpStartDate = FY.YRStartDate;
            dtTmpEndDate = FY.YREndDate;

            if (dtDate >= dtTmpStartDate && dtDate <= dtTmpEndDate &&
                m_dtStartDate >= dtTmpStartDate && m_dtStartDate <= dtTmpEndDate)
            {
                if (!(hr = FY.GetPeriod(dtDate, out pObjIPd2)) ||
                     !(hr = FY.GetPeriod(m_dtStartDate, out pObjIPd1)) ||
                     !(hr = FY.GetMidPeriodDate(m_dtStartDate, out dtMidDate)))
                    return hr;

                iPeriodNum1 = pObjIPd1.PeriodNum;
                iPeriodNum2 = pObjIPd2.PeriodNum;
                if (iPeriodNum1 == iPeriodNum2 ||
                   (iPeriodNum1 + 1 == iPeriodNum2 && m_dtStartDate >= dtMidDate))
                {
                    if (!(hr = FY.GetMidPeriodDate(dtDate, out dtMidDate)))
                        return hr;

                    if (dtDate < dtMidDate)
                    {
                        if (!(hr = GetFirstYearFactor(m_dtStartDate, out pVal)))
                            return hr;
                    }
                    else
                    {
                        dtMidDate = pObjIPd2.PeriodEnd;
                        if (!(hr = GetFirstYearFactor(dtMidDate.AddDays(+1), out pVal)))
                            return hr;
                    }
                }
                else
                {
                    if (!(hr = GetFirstYearFactor(m_dtStartDate, out dFYFactor1)) ||
                        !(hr = GetFirstYearFactor(dtDate, out dFYFactor2)))
                        return hr;

                    pVal = dFYFactor1 - dFYFactor2;
                }
            }
            else
            {
                if (!(hr = FY.GetFiscalYearFraction(out dFYFactor1)) ||
                    !(hr = GetFirstYearFactor(dtDate, out dFYFactor2)))
                    return hr;

                pVal = dFYFactor1 - dFYFactor2;
            }

            return true;
        }

        public bool GetDisposalYearFactor(double RemainingLife, DateTime dtDate, out double pVal)
        {
	        IBAFiscalYear           FY;
            IBACalcPeriod           pObjIPd1;
            IBACalcPeriod           pObjIPd2;
	        DateTime		     	dtTmpEndDate;
	        DateTime				dtTmpStartDate;
	        DateTime				dtMidDate;
	        DateTime				dtMidDate2;
	        double					dFYFactor1;
	        double					dFYFactor2;
	        short					iPeriodNum1;
	        short					iPeriodNum2;
            short					iPeriods1;
            short					iCurWeight1;
            short					iPeriods2;
            short					iCurWeight2;
            short					iAnuWeight;
	        bool					hr;

        //	if ( RemainingLife > 0.99 && RemainingLife < 0.01 )
        //		return GetLastYearFactor(RemainingLife, dtDate, pVal);
            pVal = 0;

	        if (  m_pObjCalendar == null )
		        throw new Exception("Avg Convention not initialized.");

            if( dtDate <= DateTime.MinValue )
	        {
                dtDate = m_dtEndDate;
            }
    
            if( dtDate < m_dtStartDate )
	        {
                dtDate = m_dtStartDate;
            }
    
            if ( !(hr = m_pObjCalendar.GetFiscalYear(dtDate, out FY)))
		        return hr;
		    dtTmpStartDate = FY.YRStartDate;
 		    dtTmpEndDate = FY.YREndDate;
    
            if( dtDate >= dtTmpStartDate && dtDate <= dtTmpEndDate && 
                m_dtStartDate >= dtTmpStartDate && m_dtStartDate <= dtTmpEndDate )
	        {
		        //
		        // We are in the first year
		        //
                if ( !(hr = FY.GetPeriod(dtDate, out pObjIPd2)) ||
        	         !(hr = FY.GetPeriod(m_dtStartDate, out pObjIPd1)) ||
			         !(hr = FY.GetMidPeriodDate(m_dtStartDate, out dtMidDate2)) ||
			         !(hr = FY.GetMidPeriodDate(dtDate, out dtMidDate)) )
                    return hr;
		        iPeriodNum1 = pObjIPd1.PeriodNum;
		        iPeriodNum2 = pObjIPd2.PeriodNum;
    	        if ( !(hr = FY.GetRemainingPeriodWeights(dtDate, out iPeriods1)) ||
    		         !(hr = FY.GetCurrentPeriodWeight(dtDate, out iCurWeight1)) ||
    		         !(hr = FY.GetRemainingPeriodWeights(m_dtStartDate, out iPeriods2)) ||
    		         !(hr = FY.GetCurrentPeriodWeight(m_dtStartDate, out iCurWeight2)) ||
    		         !(hr = FY.GetTotalAnnualPeriodWeights(out iAnuWeight)) )
			        return hr;

                if( iPeriodNum1 == iPeriodNum2 || 
		           (iPeriodNum1 + 1 == iPeriodNum2 && m_dtStartDate >= dtMidDate) )
		        {
                    if( dtDate < dtMidDate )
			        {
				        pVal = 0;
			        }
                    else
                    {
				        pVal = ((double)(iCurWeight1)) / iAnuWeight;    
                    }
		        }
                else
		        {
			        if ( dtDate >= dtMidDate )
				        pVal = (double)(iCurWeight2 + iPeriods2 - iPeriods1) / iAnuWeight;
			        else
				        pVal = (double)(iCurWeight2 + iPeriods2 - iPeriods1 - iCurWeight1) / iAnuWeight;
                }
	        }
            else
            {
		        if ( !(hr = FY.GetFiscalYearFraction(out dFYFactor1)) ||
			         !(hr = GetFirstYearFactor(dtDate, out dFYFactor2)) )
			        return hr;
		        pVal = dFYFactor1 - dFYFactor2;
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
                !(hr = pObjIFY.GetPeriod(m_dtPISDate, out pObjPeriod)))
                return false;

            dtEDate = pObjIFY.YREndDate;
            dtSDate = pObjIFY.YRStartDate;

            dtPSDate = pObjPeriod.PeriodStart;
            dtPEDate = pObjPeriod.PeriodEnd;
            iFraSegTPWeight = pObjPeriod.Weight;

            if (m_dtStartDate >= dtSDate && m_dtStartDate <= dtEDate &&
                 m_dtEndDate >= dtSDate && m_dtEndDate <= dtEDate)
            {
                dtEDate = m_dtEndDate;
            }

            // if true, handle this like next month.
            if (m_dtStartDate > dtPEDate)
            {
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
                    dtRemSegStartDate = dtPEDate.AddDays(1);
                    dtRemSegEndDate = dtEDate;
                    pObjIFY.GetPeriodWeights(dtRemSegStartDate, dtRemSegEndDate, out iRemSegTPWeight);
                }
            }
            else
            {
                // handle this like full month.
                dblFraction = 1.0;
                dtFraSegStartDate = dtPSDate;
                dtFraSegEndDate = dtEDate;
                dtRemSegStartDate = DateTime.MinValue;
                dtRemSegEndDate = DateTime.MinValue;
                iRemSegTPWeight = 0;
                pObjIFY.GetPeriodWeights(dtPSDate, dtEDate, out iFraSegTPWeight);
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
                !(hr = pObjIFY.GetPeriod(m_dtEndDate, out pObjPeriod)))
                return false;

            dtEDate = pObjIFY.YREndDate;
            dtSDate = pObjIFY.YRStartDate;

            dtPSDate = pObjPeriod.PeriodStart;
            dtPEDate = pObjPeriod.PeriodEnd;
            iFraSegTPWeight = pObjPeriod.Weight;

            pObjIFY.GetPeriodWeights(dtSDate, dtPEDate, out iFraSegTPWeight);

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
