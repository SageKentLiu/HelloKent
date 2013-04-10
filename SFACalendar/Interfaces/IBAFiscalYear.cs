using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFACalendar
{
    public interface IBAFiscalYear
    {
        DateTime YRStartDate { get; set; }
        DateTime YREndDate { get; set; }
        short FYNum { get; set; }
        bool IsShortYear { get; set; }
        short FYEndMonth { get; set; }
        ECALENDARCYCLE_CYCLETYPE CycleType { get; }
        ECALENDARCYCLE_DATEOFWEEK DateOfWeek { get; }
        ECALENDARCYCLE_PDCOUNTING PDCounting { get; }
        ECALENDARCYCLE_YEARENDELECTION YearEndElect { get; }

        bool GetRemainingPeriodWeights(DateTime dtDate, out short pVal);
        bool GetPreviousPeriodWeights(DateTime dtDate, out short pVal);
        bool GetCurrentPeriodWeight(DateTime dtDate, out short pVal);
        bool GetPeriodWeights(DateTime dtStartDate, DateTime dtEndData, out short pVal);
        bool GetTotalAnnualPeriodWeights(out short pVal);
        bool GetTotalFiscalYearPeriodWeights(out short pVal);
        bool GetFiscalYearFraction(out double pVal);
        bool GetNumPeriods(out short pVal);
        bool GetPeriod(DateTime dtDate, out IBACalcPeriod objPeriod);
        bool GetPeriodByNum(short iPeriodNum, out IBACalcPeriod objPeriod);
        bool GetMidPeriodDate(DateTime dtDate, out DateTime pVal);
        bool FiscalYearEndDate(long iYear, out DateTime pVal);
        bool GetMidYearDate(out DateTime pVal);

        bool InitializeWithCycleObject(IBACycleObject newVal, short iYearNumber);

    }
}
