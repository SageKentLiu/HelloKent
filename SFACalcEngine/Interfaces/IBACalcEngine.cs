using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFACalcEngine
{
    public interface IBACalcEngine
    {
        IBACalcLookUp CalcLookUp { get; set; }

        bool CalculateProjection(IBADeprScheduleItem pObjSch, out List<IBAPeriodDetailDeprInfo> objPDItems);
        bool CalculateFASDeprToDate(IBADeprScheduleItem objSch, DateTime dtEndDate, out List<IBAPeriodDeprItem> objPDItems);
        IBAPeriodDetailDeprInfo CalculateDepreciation(IBADeprScheduleItem pObjSch, DateTime dtRunDate);

        double CalculateBonus168KAmount(IBADeprScheduleItem pObjSch);
        double CalculateFullCostBasis(IBADeprScheduleItem pObjSch);
        bool ComputeITCRecap(IBADeprScheduleItem schedule, DateTime RunDate, out double TablePct, out double Recap, out double AddBack);
        bool ComputeFullCostBasis(IBADeprScheduleItem schedule, bool ForMidQtr, out bool InLastQtr, out double Basis);
    }
}
