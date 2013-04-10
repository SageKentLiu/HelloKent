using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFACalendar
{
    public interface IBACycleObject
    {

        ECALENDARCYCLE_CYCLETYPE CycleType { get; set; }
        ECALENDARCYCLE_DATEOFWEEK DateOfWeek { get; set; }
        ECALENDARCYCLE_PDCOUNTING PDCounting { get; set; }
        ECALENDARCYCLE_YEARENDELECTION YearEndElect { get; set; }
        short FYEndMonth { get; set; }
        DateTime EffectiveDate { get; set; }
        DateTime EndDate { get; set; }
        short YearNumberOffset { get; set; }
        short NumberOfYears { get; set; }
        bool IsDirty { get; set; }

    }
}
