using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SFACalendar;

namespace SFACalcEngine
{
    public interface IBAFASAdjustmentAllocator
    {
        IBADeprScheduleItem schedule { get; set; }
        IBACalendar Calendar { get; set; }
        IBAFiscalYear FiscalYear { get; set; }

        IBAPeriodDeprItem BeginInformation { get; set; }

        DateTime DeemedEndDate { get; set; }
        double AdjRemainingLife { get; set; }
        string AdjDeprFlags { get; set; }

        bool CalculateAdjustment(DateTime CalcStart, ref double adjAmount, ref string flag, ref DateTime AdjPeriodStart);
        bool AdjustmentStillNeeded(DateTime CalcStart, out bool pVal);

    }
}
