using System;
using System.Collections.Generic;
using System.Text;

namespace SFACalcEngine
{
    public enum DISPOSALOVERRIDETYPE
    {
		disposaloverride_Normal,
		disposaloverride_FullYear,
		disposaloverride_NoneInYear
	} ;
    public enum LUXURYLIMIT_TYPE
    {
        LUXURYLIMITTYPE_NOTAPPLY = 0,
        LUXURYLIMITTYPE_LUXURYCAR = 1,
        LUXURYLIMITTYPE_ELECTRICCAR = 2,
        LUXURYLIMITTYPE_LTTRUCKSANDVANS = 3,
    } ;


    public interface IBADeprMethod
    {
        double AdjustedCost { get; set; }
        double PostUsageDeduction { get; set; }
        double SalvageDeduction { get; set; }
        double PriorAccum { get; set; }
        double YearElapsed { get; set; }
        short YearNum { get; set; }
        double Life { get; set; }
        double DBPercent { get; set; }
        double FiscalYearFraction { get; set; }
        DateTime DeemedStartDate { get; set; }
        DateTime DeemedEndDate { get; set; }
        string ParentFlags { get; set; }

        bool IsFiscalYearBased { get; }
        double Basis { get; }
        double RemainingDeprAmt { get; }
        string BaseShortName { get; }
        string BaseLongName { get; }
        bool UseFirstYearFactor { get; }
        double TotalDepreciationAllowed { get; }
        DISPOSALOVERRIDETYPE DisposalOverride { get; }

        bool Initialize(IBADeprScheduleItem schedule, IBAAvgConvention convention);
        double CalculateAnnualDepr();
        bool GetAvgConvention(IBADeprScheduleItem schedule, ref string pVal);
            


    }
}
