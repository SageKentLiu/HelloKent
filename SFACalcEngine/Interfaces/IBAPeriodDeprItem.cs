using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFACalcEngine
{
    public enum PERIODDEPRITEM_ENTRYTYPE
    {
        PERIODDEPRITEM_NORMAL = 0,
        PERIODDEPRITEM_ADJIMMEDIATE = 1,
        PERIODDEPRITEM_ADJADJPERIOD = 2,
        PERIODDEPRITEM_ADJRATABLY = 3,
        PERIODDEPRITEM_ADJPOSTRECOVERY = 4,
        PERIODDEPRITEM_ADJRATABLY4YEARS = 5,
        PERIODDEPRITEM_ADJESTIMATE = 6,
        PERIODDEPRITEM_ADJIMPAIRMENT = 7,
        PERIODDEPRITEM_ADJNOADJUSTMENT = 8,
        PERIODDEPRITEM_FIRSTPERIOD = 9,
        PERIODDEPRITEM_LASTPERIOD = 10,
        PERIODDEPRITEM_DISPOSEDPERIOD = 11,
        PERIODDEPRITEM_CRITICALCHANGE = 12,
        PERIODDEPRITEM_ADJUSTMENTONLY = 13
    } ;
    public enum ustblDeprTableType
    {
        ustblACRS,
        ustblMACRS
    } ;

    public interface IBAPeriodDeprItem
    {
        DateTime StartDate { get; set; }
        DateTime EndDate { get; set; }
        short FYNum { get; set; }
        short TotalPeriodWeights { get; set; }

        decimal BeginYearAccum { get; set; }
        decimal BeginYTDExpense { get; set; }
        decimal DeprAmount { get; set; }
        decimal AdjustAmount { get; set; }
        decimal Section179Change { get; set; }

        PERIODDEPRITEM_ENTRYTYPE EntryType { get; set; }

        double PeriodExpense { get; }
        double BeginPeriodAccum { get; }
        double EndPeriodAccum { get; }
        double YTDExpense { get; }
        double EndDateYTDExpense { get; }

        decimal CalcOverride { get; set; }
        string CalcFlags { get; set; }
        decimal EndDateBeginYearAccum { get; set; }

        //rdbj
        bool Clear();

        decimal EndDateDeferredAccum { get; set; }
        decimal EndDateYTDDeferred { get; set; }
        decimal EndDatePersonalUseAccum { get; set; }
        decimal EndDateYTDPersonalUse { get; set; }
        double RemainingLife { get; set; }
        double PersonalUseAmount { get; set; }

        //HRESULT Split2ways([in]DateTime rightStartDate, [in]IBACalendar *pObjCalendar, [in]DateTime PISDate, [in]DateTime DeemedEndDate, [in,out]IBAPeriodDeprItem**left, [in,out]IBAPeriodDeprItem **right);
        //HRESULT Split3ways([in]DateTime middleStartDate, [in]DateTime rightStartDate, [in]IBACalendar *pObjCalendar, [in]DateTime PISDate, [in]DateTime DeemedEndDate, [in,out]IBAPeriodDeprItem**left, [in,out]IBAPeriodDeprItem **middle, [in,out]IBAPeriodDeprItem **right);
        bool Clone(out IBAPeriodDeprItem pVal);

    }
}