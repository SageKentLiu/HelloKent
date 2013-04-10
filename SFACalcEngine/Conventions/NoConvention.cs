using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFACalendar;

namespace SFACalcEngine
{
    class NoConvention : IBAAvgConvention
    {
        IBACalendar m_pObjCalendar;
        DateTime m_dtStartDate;
        DateTime m_dtEndDate;
        DateTime m_dtPISDate;
        double m_dblLife;
        MonthlyYear m_pMonthlyYear;
        short m_qtrNumber;

        public NoConvention()
        {

        }

        public bool Initialize(IBACalendar calendar, DateTime PlacedInService, double Life)
        {
            int iYear;
            int iMonth;
            int iDay;
            DateTime dtTmpEndDate;
            DateTime dtTmpStartDate;
            IBAFiscalYear FY;
            bool hr;

            if (calendar == null || PlacedInService <= DateTime.MinValue || Life <= 0)
                return false;

            m_pObjCalendar = null;
            m_pObjCalendar = calendar;
            m_dtPISDate = PlacedInService;
            m_dblLife = Life;

            if (!(hr = m_pObjCalendar.GetFiscalYear(m_dtPISDate, out FY)))
                return hr;
            dtTmpStartDate = FY.YRStartDate;
            dtTmpEndDate = FY.YREndDate;

            m_dtStartDate = m_dtPISDate;

            iYear = m_dtStartDate.Year + ((int)(m_dblLife));
            iMonth = m_dtStartDate.Month + Convert.ToInt32((m_dblLife - ((int)(m_dblLife))) * 12);
            iDay = m_dtStartDate.Day;

            m_dtEndDate = new DateTime(iYear, iMonth, iDay).AddDays(-1);
            return true;
        }

        public bool GetFirstYearFactor(DateTime dtDate, out double pVal)
        {
            IBAFiscalYear FY;
            int iDays;
            int iTotalDays;
            short iPeriods;
            short iCurWeight;
            short iAnuWeight;
            IBACalcPeriod pObjPeriod;
            DateTime dtTmpEndDate;
            DateTime dtTmpStartDate;
            DateTime dtTmpEDate;
            DateTime dtTmpSDate;
            bool hr;
            pVal = 0.0;

            if (m_pObjCalendar == null)
                throw new Exception("Avg Convention not initialized.");
            if (dtDate <= DateTime.MinValue)
                throw new Exception("Avg Convention not initialized.");

            if (!(hr = m_pObjCalendar.GetFiscalYear(dtDate, out FY)))
                return hr;
            dtTmpStartDate = FY.YRStartDate;
            dtTmpEndDate = FY.YREndDate;

            if (!(hr = FY.GetPeriod(dtDate, out pObjPeriod)))
                return hr;
            dtTmpEDate = pObjPeriod.PeriodEnd;
            dtTmpSDate = pObjPeriod.PeriodStart;

            if (!(hr = FY.GetRemainingPeriodWeights(dtDate, out iPeriods)) ||
                !(hr = FY.GetCurrentPeriodWeight(dtDate, out iCurWeight)) ||
                !(hr = FY.GetTotalAnnualPeriodWeights(out iAnuWeight)))
                return hr;

            iDays = ((int)((dtTmpEDate - dtDate).TotalDays + 1));
            iTotalDays = ((int)((dtTmpEDate - dtTmpSDate).TotalDays + 1));

            pVal = ((double)(iPeriods) + iCurWeight * (iDays) /
                    (double)(iTotalDays)) / (double)(iAnuWeight);

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
                if (!(hr = FY.GetPeriod(m_dtStartDate, out pObjIPd1)) ||
                     !(hr = FY.GetPeriod(dtDate, out pObjIPd2)))
                     return hr;
                iPeriodNum1 = pObjIPd1.PeriodNum;
                iPeriodNum2 = pObjIPd2.PeriodNum;

                //if the dispdate is in the first year
                if (iPeriodNum1 >= iPeriodNum2)
                {
                    //in the same period
                    if (!(hr = GetFirstYearFactor(m_dtStartDate, out dFYFactor1)) ||
                        !(hr = GetFirstYearFactor(dtDate, out dFYFactor2)))
                        return hr;

                    pVal = dFYFactor1 - dFYFactor2;
                }
                else
                {
                    //in different period
                    if (!(hr = GetFirstYearFactor(m_dtStartDate, out dFYFactor1)) ||
                         !(hr = GetFirstYearFactor(dtDate, out dFYFactor2)))
                        return hr;

                    pVal = dFYFactor1 - dFYFactor2;
                }
            }
            else
            {
                //other years
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
            int iDays;
            int iTotalDays;
            IBACalcPeriod pObjPeriod;
            DateTime dtTmpStartDate;
            DateTime dtTmpEndDate;
            DateTime dtTmpEDate;
            DateTime dtTmpSDate;
            IBAFiscalYear FY;
            bool hr;
            pVal = false;

            if (m_pObjCalendar == null)
                throw new Exception("Avg Convention not initialized.");

            if (!(hr = m_pObjCalendar.GetFiscalYear(m_dtPISDate, out FY)))
                return hr;
            dtTmpStartDate = FY.YRStartDate;
            dtTmpEndDate = FY.YREndDate;

            if (!(hr = FY.GetPeriod(m_dtPISDate, out pObjPeriod)))
                return hr;
            dtTmpEDate = pObjPeriod.PeriodEnd;
            dtTmpSDate = pObjPeriod.PeriodStart;

            if (m_dtStartDate >= dtTmpStartDate && m_dtStartDate <= dtTmpEndDate &&
                 m_dtEndDate >= dtTmpStartDate && m_dtEndDate <= dtTmpEndDate)
            {
                dtTmpEndDate = m_dtEndDate;
            }
            if (!(hr = FY.GetPeriodWeights(dtTmpEDate.AddDays(1), dtTmpEndDate, out iRemSegTPWeight)) ||
                 !(hr = FY.GetCurrentPeriodWeight(m_dtPISDate, out iFraSegTPWeight)))
                return hr;

            iDays = (int)((dtTmpEDate - m_dtStartDate).TotalDays + 1);
            iTotalDays = (int)((dtTmpEDate - dtTmpSDate).TotalDays + 1);

            dblFraction = ((double)(iFraSegTPWeight) * (double)(iDays) / (double)(iTotalDays)) /
                          ((double)(iRemSegTPWeight) + (double)(iFraSegTPWeight) * (double)(iDays) /
                           (double)(iTotalDays));
            dtFraSegStartDate = dtTmpSDate;
            dtFraSegEndDate = dtTmpEDate;
            dtRemSegStartDate = dtFraSegEndDate.AddDays(1);
            dtRemSegEndDate = dtTmpEndDate;
            pVal = true;
            return true;
        }

        public bool GetLastYearSegmentInfo(ref double dblFraction, ref DateTime dtFraSegStartDate, ref DateTime dtFraSegEndDate, ref short iFraSegTPWeight, ref DateTime dtRemSegStartDate, ref DateTime dtRemSegEndDate, ref short iRemSegTPWeight, out bool pVal)
        {
            int iDays;
            int iTotalDays;
            IBACalcPeriod pObjPeriod;
            DateTime dtTmpStartDate;
            DateTime dtTmpEndDate;
            DateTime dtTmpEDate;
            DateTime dtTmpSDate;
            IBAFiscalYear FY;
            bool hr;
            pVal = false;

            if (m_pObjCalendar == null)
                throw new Exception("Avg Convention not initialized.");

            if (!(hr = m_pObjCalendar.GetFiscalYear(m_dtEndDate, out FY)) ||
                 !(hr = FY.GetPeriod(m_dtEndDate, out pObjPeriod)))
                return hr;
            dtTmpStartDate = FY.YRStartDate;
            dtTmpEndDate = FY.YREndDate;
            dtTmpSDate = pObjPeriod.PeriodStart;
            dtTmpEDate = pObjPeriod.PeriodEnd;

            if (!(hr = FY.GetPreviousPeriodWeights(m_dtEndDate, out iRemSegTPWeight)) ||
                 !(hr = FY.GetCurrentPeriodWeight(m_dtEndDate, out iFraSegTPWeight)))
                return hr;

            iDays = (int)((m_dtEndDate - dtTmpSDate).TotalDays + 1);
            iTotalDays = (int)((dtTmpEDate - dtTmpSDate).TotalDays + 1);

            dblFraction = ((double)(iFraSegTPWeight) * (double)(iDays) / (double)(iTotalDays)) /
                           ((double)(iRemSegTPWeight) + (double)(iFraSegTPWeight) * (double)(iDays) /
                           (double)(iTotalDays));
            dtFraSegStartDate = dtTmpSDate;
            dtFraSegEndDate = dtTmpEDate;
            dtRemSegStartDate = dtTmpStartDate;
            dtRemSegEndDate = dtFraSegStartDate.AddDays(-1);
            pVal = true;
            return true;
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
