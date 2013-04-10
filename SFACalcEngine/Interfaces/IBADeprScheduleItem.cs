using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFACalendar;
using SFABusinessTypes;

namespace SFACalcEngine
{
    public interface IBADeprScheduleItem
    {
        short PropertyType { get; set; }
        double AcquisitionValue { get; set; }
        double ADSLife { get; set; }

        bpblBookTypeEnum BookType { get; set; }

        bool Bonus911Flag { get; set; }
        double Stored168KAmount { get; set; }


        double Section179 { get; set; }
        double Section179A { get; set; }
        DateTime PlacedInServiceDate { get; set; }
        DateTime DispDate { get; set; }
        DateTime DeemStartDate { get; set; }
        DateTime DeemEndDate { get; set; }
        double DeprLife { get; set; }
        short DeprPercent { get; set; }
        string DeprMethod { get; set; }
        string AvgConvention { get; set; }
        IBACalendar Calendar { get; set; }
        double AdjustedCost { get; }
        double PostUsageDeduction { get; }
        double SalvageDeduction { get; set; }
        double Bonus911Amount { get; set; }
        short Bonus911Percent { get; set; }
        short ZoneCode { get; set; }
        double BasisAdjustment { get; set; }
        double ElectricCar { get; set; }
        double ITCBasisReduction { get; set; }
        bool BonusFlag { get; set; }
        double BonusAmount { get; set; }
        DateTime LastCalcDate { get; set; }
        bool isMidQtrUsedDefault { get; set; }
        bool isMidQtr { get; set; }
        double ITCAmount { get; set; }
        double ITCReduce { get; set; }

        bool VintageAccountFlag { get; }
        bool LowIncomeHousingFlag { get; }
        bool PublicUtilityFlag { get; }
        bool PersonalPropertyFlag { get; }

        LUXURYLIMIT_TYPE GetLuxuryFlag { get; }
        bool UseACEHandling { get; }
        
        bool BuildAllPDItems { get; set; }
        bool GetBusinessUse(DateTime fyEnd, ref double dBusinessUsePct, ref double dInvestmentUsePct);
        bool AddBusinessUseEntry(DateTime fyBegin, double dBusinessUsePct, double dInvestmentUsePct);
        void GetPeriodDeprMgr();
        void ReleaseDeprMethod();
        void ReleaseAvgConvention();

        decimal ACEBasis { get; }
        double ACELife { get; }
        bool UsedOutsideTheUS { get; set; }
        double CalculateBonus911Amount { get; }
        double CalculateITCBasisReductionFactor();
        double CalculateITCBasisReductionAmount(bool AceSwitch);
       
        bestAssetImportCodeEnum ImportCode{ get; set; }
        bestAssetITCTypeEnum ITCCode{ get; set; }
        bestAssetDeprAdjustmentConventionEnum ApplyAdjustment { get; set; }
    }
}
