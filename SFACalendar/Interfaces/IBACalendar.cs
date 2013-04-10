using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFACalendar
{
    public interface IBACalendar
    {
        bool GetFiscalYear(DateTime dtDate, out IBAFiscalYear objIFY);
        bool GetFiscalYearByNum(short iYearNum, out IBAFiscalYear objIFY);
        bool GetFiscalYearNum(DateTime dtDate, out short pVal);
        bool GetPeriod(DateTime dtDate, out IBACalcPeriod pVal);
        bool FirstShortYear(out DateTime pVal);
        bool LastShortYear(out DateTime pVal);
        bool ShortYearList(out List<IBAFiscalYear> pVal);
    };

    public interface IBACalendarManager
    {
        bool GetPeriod(DateTime dtDate, out IBACalcPeriod objPeriod);
        bool GetPeriodByNum(short iYearNum, short iPeriodNum, out IBACalcPeriod objPeriod);
        bool FiscalYearEndDate(int lYear, ECALENDARCYCLE_CYCLETYPE eCycleTypeEnum, short eFYEndMonthEnum, ECALENDARCYCLE_YEARENDELECTION eYREndElectionEnum, ECALENDARCYCLE_DATEOFWEEK ePdDayWeekEnum, out DateTime pVal);
        bool FiscalYearStartDate(int lYear, ECALENDARCYCLE_CYCLETYPE eCycleTypeEnum, short eFYEndMonthEnum, ECALENDARCYCLE_YEARENDELECTION eYREndElectionEnum, ECALENDARCYCLE_DATEOFWEEK ePdDayWeekEnum, out DateTime pVal);
        bool AddCycleEntry(DateTime dtEffectiveDate, ECALENDARCYCLE_CYCLETYPE eCycleType, short eFYEndMonth, ECALENDARCYCLE_DATEOFWEEK ePeriodDayOfWeek, ECALENDARCYCLE_YEARENDELECTION eYearEndElection, ECALENDARCYCLE_PDCOUNTING ePdCounting);
        bool ClearCycleEntries();
        bool Clone(out IBACalendar pVal);

        List<IBACycleObject> CycleList { get; set; }
        bool IsDirty { get; set; }

        DateTime StartOfBusiness { get; set; }
    };
}
