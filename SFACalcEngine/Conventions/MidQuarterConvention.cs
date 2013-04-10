using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFACalendar;

namespace SFACalcEngine
{
    public class MidQuarterConvention : IBAAvgConvention
    {
        IBACalendar m_pObjCalendar;
        DateTime m_dtStartDate;
        DateTime m_dtEndDate;
        DateTime m_dtPISDate;
        double m_dblLife;
        MonthlyYear m_pMonthlyYear;
        short m_qtrNumber;

        public MidQuarterConvention()
        {

        }

        public bool Initialize(SFACalendar.IBACalendar calendar, DateTime PlacedInService, double Life)
        {
            int iYear;
            int iMonth;
            int iDay;
            IBAFiscalYear pObjIFY;
            bool hr;

            if (calendar == null || PlacedInService <= DateTime.MinValue || Life <= 1)
                return false;

            m_pObjCalendar = null;
            m_pObjCalendar = calendar;
            m_dtPISDate = PlacedInService;
            m_dblLife = Life;

            if (m_pMonthlyYear == null)
                m_pMonthlyYear = new MonthlyYear();

            m_pObjCalendar.GetFiscalYear(m_dtPISDate, out pObjIFY);

            m_pMonthlyYear.FiscalYearInfo = (pObjIFY);
            m_pMonthlyYear.DeemedFYDates();

            //calc the start date
            m_dtStartDate = m_pMonthlyYear.GetMidQuarterDate(m_dtPISDate);
            m_qtrNumber = m_pMonthlyYear.GetMidQuarterNumber(m_dtPISDate);

            //calc the deemed end date
            iYear = (m_dtStartDate.Year) + (int)(m_dblLife);
            iMonth = (m_dtStartDate.Month) + (int)((m_dblLife - (int)(m_dblLife)) * 12);
            iDay = (m_dtStartDate.Day);

            m_dtEndDate = new DateTime(iYear, iMonth, iDay).AddDays(-1);
            return true;
        }

        public bool GetFirstYearFactor(DateTime dtDate, out double pVal)
        {
            DateTime dtSDate;
            DateTime dtEDate;
            IBAFiscalYear pObjIFY;
            bool hr;
            pVal = 0.0;

            if (m_pObjCalendar == null)
                throw new Exception("Avg Convention not initialized.");
            if (dtDate <= DateTime.MinValue)
                throw new Exception("Avg Convention not initialized.");


            if (!(hr = m_pObjCalendar.GetFiscalYear(dtDate, out pObjIFY)))
            {
                return false;
            }
            dtSDate = pObjIFY.YRStartDate;
            dtEDate = pObjIFY.YREndDate;

            m_pMonthlyYear.FiscalYearInfo = (pObjIFY);
            m_pMonthlyYear.DeemedFYDates();
            //    *pVal = m_pMonthlyYear.GetFirstYearFactor(m_pMonthlyYear.GetMidQuarterDate(dtDate));
            pVal = m_pMonthlyYear.GetFirstYearFactor(dtDate);
            return true;
        }

        public bool GetLastYearFactor(double RemainingLife, DateTime dtDate, out double pVal)
        {
            IBAFiscalYear pObjIFY;
            IBACalcPeriod pObjIPd;
            DateTime dtEDate;
            DateTime dtSDate;
            DateTime dtPEDate = DateTime.MinValue;
            bool hr;
            pVal = 0.0;

            if (m_pObjCalendar == null)
                throw new Exception("Avg Convention not initialized.");

            if (dtDate <= DateTime.MinValue)
            {
                dtDate = m_dtEndDate;
            }

            if (dtDate < m_dtPISDate)
            {
                dtDate = m_dtPISDate;
            }

            if (!(hr = m_pObjCalendar.GetFiscalYear(dtDate, out pObjIFY)))
                return hr;

            dtSDate = pObjIFY.YRStartDate;
            dtEDate = pObjIFY.YREndDate;

            m_pMonthlyYear.FiscalYearInfo = (pObjIFY);
            m_pMonthlyYear.DeemedFYDates();

            if (dtDate >= dtSDate && dtDate <= dtEDate &&
                m_dtStartDate >= dtSDate && m_dtStartDate <= dtEDate)
            {
                if (!(hr = pObjIFY.GetPeriod(m_dtPISDate, out pObjIPd)))
                    return hr;
                dtPEDate = pObjIPd.PeriodEnd;

                //in the same year
                if (dtDate <= dtPEDate)
                {
                    pVal = m_pMonthlyYear.GetFirstYearFactor(m_pMonthlyYear.GetMidQuarterDate(dtDate));
                }
                else
                {
                    pVal = 0;
                }

            }
            else
            {
                //in diff year
                pVal = m_pMonthlyYear.GetLastYearFactor(m_pMonthlyYear.GetMidQuarterDate(dtDate));
            }

            return true;
        }

        public bool GetDisposalYearFactor(double RemainingLife, DateTime dtDate, out double pVal)
        {
            DateTime dtSDate;
            DateTime dtEDate;
            DateTime dtPEDate;
            IBACalcPeriod pObjIPd;
            IBAFiscalYear pObjIFY;
            bool hr;

            pVal = 0;
            if (m_pObjCalendar == null)
                throw new Exception ("Avg Convention not initialized.");

            if (dtDate <= DateTime.MinValue)
                dtDate = m_dtEndDate;

            if (dtDate < m_dtPISDate)
                dtDate = m_dtPISDate;

            if (!(hr = m_pObjCalendar.GetFiscalYear(dtDate, out pObjIFY)) )
                return hr;
            dtEDate = pObjIFY.YREndDate;
            dtSDate = pObjIFY.YRStartDate;

            m_pMonthlyYear.FiscalYearInfo = pObjIFY;
            m_pMonthlyYear.DeemedFYDates();

            if (dtDate >= dtSDate && dtDate <= dtEDate &&
                m_dtStartDate >= dtSDate && m_dtStartDate <= dtEDate)
            {
                if (!(hr = pObjIFY.GetPeriod(m_dtPISDate, out pObjIPd)) )
                    return hr;    
                dtPEDate = pObjIPd.PeriodEnd;

                //in the same year

                if (dtEDate < new DateTime(1991, 1, 30))
                {
                    if (dtDate > dtPEDate)
                    {
                        pVal = m_pMonthlyYear.GetFirstYearFactor(m_pMonthlyYear.GetMidQuarterDate(m_dtPISDate)) -
                            m_pMonthlyYear.GetFirstYearFactor(m_pMonthlyYear.GetMidQuarterDate(dtDate));
                    }
                    else
                    {
                        pVal = 0;
                    }
                }
                else
                {
                    pVal = 0;
                }
            }
            else if (m_dtEndDate >= dtSDate && m_dtEndDate <= dtEDate)
            {
                // In last year
                if (dtDate > m_dtEndDate)
                    pVal = RemainingLife;
                else
                    pVal = m_pMonthlyYear.GetLastYearFactor(m_pMonthlyYear.GetMidQuarterDate(dtDate)) *
                        RemainingLife;
            }
            else
            {
                //in diff year
                pVal = m_pMonthlyYear.GetLastYearFactor(m_pMonthlyYear.GetMidQuarterDate(dtDate));
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
            int iDays;
            int iTotalDays;
            DateTime dtSDate;
            DateTime dtEDate;
            DateTime dtPSDate;
            DateTime dtPEDate;
            IBACalcPeriod pObjPeriod;
            IBAFiscalYear pObjIFY;
            bool hr;
            ECALENDARCYCLE_CYCLETYPE eCType;
            pVal = false;

            if (m_pObjCalendar == null)
                throw new Exception("Avg Convention not initialized.");

            if (!(hr = m_pObjCalendar.GetFiscalYear(m_dtPISDate, out pObjIFY)) ||
                !(hr = pObjIFY.GetPeriod(m_dtPISDate, out pObjPeriod)))
                return false;

            dtEDate = pObjIFY.YREndDate;
            dtSDate = pObjIFY.YRStartDate;
            eCType = pObjIFY.CycleType;

            dtPSDate = pObjPeriod.PeriodStart;
            dtPEDate = pObjPeriod.PeriodEnd;

            if (eCType != ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_MONTHLY)
            {
                iDays = (int)((dtPEDate - m_dtPISDate).TotalDays + 1);
                iTotalDays = (int)((dtPEDate - dtPSDate).TotalDays + 1);

                pObjIFY.GetRemainingPeriodWeights(m_dtPISDate, out iRemSegTPWeight);
                pObjIFY.GetCurrentPeriodWeight(m_dtPISDate, out iFraSegTPWeight);

                dblFraction = ((double)(iFraSegTPWeight) * (double)(iDays) / (double)(iTotalDays)) /
                               ((double)(iRemSegTPWeight) + (double)(iFraSegTPWeight) * (double)(iDays) /
                               (double)(iTotalDays));
                dtFraSegStartDate = dtPSDate;
                dtFraSegEndDate = dtPEDate;
                dtRemSegStartDate = dtPEDate.AddDays(1);
                dtRemSegEndDate = dtEDate;
            }
            else
            {
                short iPdWeights;
                pObjIFY.GetPeriodWeights(dtPSDate, dtEDate, out iPdWeights);

                iRemSegTPWeight = 0;
                iFraSegTPWeight = iPdWeights;
                dblFraction = 1.0;
                dtFraSegStartDate = dtPSDate;
                dtFraSegEndDate = dtEDate;
                dtRemSegStartDate = DateTime.MinValue;
                dtRemSegEndDate = DateTime.MinValue;
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
            IBACalcPeriod pObjPeriod;
            IBAFiscalYear pObjIFY;
            bool hr;
            ECALENDARCYCLE_CYCLETYPE eCType;
            pVal = false;

            if (m_pObjCalendar == null)
                throw new Exception("Avg Convention not initialized.");

            if (!(hr = m_pObjCalendar.GetFiscalYear(m_dtEndDate, out pObjIFY)) ||
                !(hr = pObjIFY.GetPeriod(m_dtEndDate, out pObjPeriod)))
                return false;

            dtEDate = pObjIFY.YREndDate;
            dtSDate = pObjIFY.YRStartDate;
            eCType = pObjIFY.CycleType;

            dtPSDate = pObjPeriod.PeriodStart;
            dtPEDate = pObjPeriod.PeriodEnd;

            if (eCType != ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_MONTHLY)
            {
                iDays = (int)((m_dtEndDate - dtPSDate).TotalDays + 1);
                iTotalDays = (int)((dtPEDate - dtPSDate).TotalDays + 1);

                pObjIFY.GetPreviousPeriodWeights(m_dtEndDate, out iRemSegTPWeight);
                pObjIFY.GetCurrentPeriodWeight(m_dtEndDate, out iFraSegTPWeight);

                dblFraction = ((double)(iFraSegTPWeight) * (double)(iDays) / (double)(iTotalDays)) /
                               ((double)(iRemSegTPWeight) + (double)(iFraSegTPWeight) * (double)(iDays) /
                               (double)(iTotalDays));
                dtFraSegStartDate = dtPSDate;
                dtFraSegEndDate = dtPEDate;
                dtRemSegStartDate = dtSDate;
                dtRemSegEndDate = dtFraSegStartDate.AddDays(-1);
            }
            else
            {
                short iPdWeights;
                short iLastWeight;

                if ((m_dtEndDate.Day) > 13 && dtPSDate > dtSDate)
                {
                    pObjIFY.GetPeriodWeights(dtSDate, dtPSDate.AddDays(-1), out iPdWeights);
                    iLastWeight = pObjPeriod.Weight;

                    dblFraction = ((double)(iLastWeight + 0)) / ((iPdWeights << 1) + iLastWeight);
                    iRemSegTPWeight = iPdWeights;
                    iFraSegTPWeight = iLastWeight;
                    dtRemSegStartDate = dtSDate;
                    dtRemSegEndDate = dtPSDate.AddDays(-1);
                    dtFraSegStartDate = dtPSDate;
                    dtFraSegEndDate = dtPEDate;
                }
                else
                {
                    if (!(hr = pObjIFY.GetPeriodWeights(dtSDate, dtPEDate, out iPdWeights)))
                        return hr;

                    iRemSegTPWeight = 0;
                    iFraSegTPWeight = iPdWeights;
                    dblFraction = 1.0;
                    dtFraSegStartDate = dtPSDate;
                    dtFraSegEndDate = dtEDate;
                    dtRemSegStartDate = dtPSDate;
                    dtRemSegEndDate = dtEDate;
                }

            }
            return true;
        }

        public bool MonthBased
        {
            get { return true; }
        }

        public short DetermineTablePeriod
        {
            get { return m_qtrNumber; }
        }
    }
}
