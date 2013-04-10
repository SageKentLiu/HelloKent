using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SFACalendar;

namespace SFACalcEngine
{
    public interface IBAAvgConvention
    {
        bool Initialize(IBACalendar calendar, DateTime PlacedInService, double Life);

			bool GetFirstYearFactor( DateTime dtDate, out  double pVal);
			bool GetLastYearFactor(double RemainingLife, DateTime dtDate,out  double pVal);
			bool GetDisposalYearFactor(double RemainingLife,  DateTime dtDate,out  double pVal);

        DateTime DeemedStartDate { get; }
        DateTime DeemedEndDate { get; }



			bool IsSplitNeeded(DateTime dtDate, out  bool pVal);
			bool GetFirstYearSegmentInfo(
                        ref double dblFraction,
                        ref DateTime dtFraSegStartDate,
                        ref DateTime dtFraSegEndDate,
                        ref short iFraSegTPWeight,
                        ref DateTime dtRemSegStartDate,
                        ref DateTime dtRemSegEndDate,
                        ref short iRemSegTPWeight,
                        out  bool pVal);
			bool GetLastYearSegmentInfo(
                        ref double dblFraction,
                        ref DateTime dtFraSegStartDate,
                        ref DateTime dtFraSegEndDate,
                        ref short iFraSegTPWeight,
                        ref DateTime dtRemSegStartDate,
                        ref DateTime dtRemSegEndDate,
                        ref short iRemSegTPWeight,
                        out  bool pVal);

        bool MonthBased { get; }
        short DetermineTablePeriod { get; }

    }
}
