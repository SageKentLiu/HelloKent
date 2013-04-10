using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SFABusinessTypes;
using SFACalcEngine;
using SFACalendar;
using WebRoleHelloKent.Models;


namespace WebRoleHelloKent.Controllers
{
    public class DepreciationController : ApiController
    {
        // GET api/depreciation
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/depreciation/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/depreciation/Calc168KAmount
        [HttpPost]
        public double Calc168KAmount(DepreciableBook DeprBook)
        {
            IBACalcEngine CalcEngine = new CalcEngine();
            IBADeprScheduleItem Schedule = new BAFASDeprScheduleItem();

            SetUpCalcSchedule(DeprBook, ref Schedule);
            return CalcEngine.CalculateBonus168KAmount(Schedule);
        }

        // POST api/depreciation/CalcFullCostBasis
        [HttpPost]
        public double CalcFullCostBasis(DepreciableBook DeprBook)
        {
            IBACalcEngine CalcEngine = new CalcEngine();
            IBADeprScheduleItem Schedule = new BAFASDeprScheduleItem();

            SetUpCalcSchedule(DeprBook, ref Schedule);
            return CalcEngine.CalculateFullCostBasis(Schedule);
        }

        // POST api/depreciation/CalcDepreciation
        [HttpPost]
        public PeriodDeprItemModel CalcDepreciation(DepreciableBook DeprBook)
        {
            IBACalcEngine CalcEngine = new CalcEngine();
            IBADeprScheduleItem Schedule = new BAFASDeprScheduleItem();
            IBAPeriodDetailDeprInfo pdi;
            DateTime RunDate = DeprBook.RunDate;

            SetUpCalcSchedule(DeprBook, ref Schedule);
            pdi = CalcEngine.CalculateDepreciation(Schedule, RunDate);
            var r2 = new PeriodDeprItemModel
            {
                StartDate = pdi.PeriodStartDate ,
                EndDate = pdi.PeriodEndDate,
                BeginYearAccum = (double)pdi.FiscalYearBeginAccum,
                BeginYearYTDExpense = (double)pdi.PeriodBeginAccum,
                DeprAmount = (double)pdi.PeriodDeprAmount,
                CurrntYTDDepr = (double)pdi.PeriodEndAccum,
                CurrentAccumDepr = (double)pdi.FiscalYearEndAccum,
                CalcFlags = pdi.CalcFlags
            };
            return r2;
        }

        // POST api/depreciation/CalcProjection
        [HttpPost]
        public IEnumerable<PeriodDeprItemModel> CalcProjection(DepreciableBook DeprBook)
        {
            IBACalcEngine CalcEngine = new CalcEngine();
            IBADeprScheduleItem Schedule = new BAFASDeprScheduleItem();
            List<IBAPeriodDetailDeprInfo> pTmpList;

            SetUpCalcSchedule(DeprBook, ref Schedule);

            //CalcEngine.CalculateFASDeprToDate(Schedule, RunDate, out pTmpList);
            CalcEngine.CalculateProjection(Schedule, out pTmpList);

            var r2 = from IBAPeriodDetailDeprInfo pdi in pTmpList
                     select new PeriodDeprItemModel
                     {
                         StartDate = pdi.PeriodStartDate,
                         EndDate = pdi.PeriodEndDate,
                         BeginYearAccum = (double)pdi.FiscalYearBeginAccum ,
                         BeginYearYTDExpense = (double)pdi.PeriodBeginAccum,
                         DeprAmount = (double)pdi.PeriodDeprAmount,
                         CurrntYTDDepr = (double)pdi.PeriodEndAccum,
                         CurrentAccumDepr = (double)pdi.FiscalYearEndAccum,
                         CalcFlags = pdi.CalcFlags
                     };
            return r2.ToList();                    
        }

        // PUT api/depreciation/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/depreciation/5
        public void Delete(int id)
        {
        }

        private void SetUpCalcSchedule(DepreciableBook DeprBook, ref IBADeprScheduleItem Schedule)
        {
            Schedule.PropertyType = ConvertPropType(DeprBook.PropertyType);
            Schedule.BookType = bpblBookTypeEnum.bpblBookTaxBook;
            Schedule.DispDate = DateTime.MinValue;

            Schedule.AcquisitionValue = Convert.ToDouble(DeprBook.AcquiredValue);
            Schedule.PlacedInServiceDate = DeprBook.PlaceInServiceDate;
            Schedule.DeprLife = DeprBook.EstimatedLife;
            Schedule.DeprMethod = ConvertDeprMethod(DeprBook.DepreciateMethod, (short)DeprBook.DepreciatePercent);
            Schedule.DeprPercent = (short)DeprBook.DepreciatePercent;
            Schedule.Section179 = DeprBook.Section179;
            Schedule.SalvageDeduction = DeprBook.SalvageDeduction;
            Schedule.ITCAmount = DeprBook.ITCAmount;
            Schedule.ITCReduce = DeprBook.ITCReduce;
            Schedule.Bonus911Percent = DeprBook.Bonus911Percent;
            Schedule.AvgConvention = DeprBook.Convention;

            Schedule.AddBusinessUseEntry(new DateTime(2013, 1, 1), 0.7, 0);
            Schedule.AddBusinessUseEntry(new DateTime(2013, 1, 1), 0.7, 0);
            Schedule.AddBusinessUseEntry(new DateTime(2011, 1, 1), 0.8, 0);
            Schedule.AddBusinessUseEntry(new DateTime(2014, 1, 1), 0.6, 0);
            Schedule.AddBusinessUseEntry(new DateTime(2013, 1, 1), 0.7, 0);

        }

        private string ConvertDeprMethod(string method, short pct)
        {
            if (method == "MA")
                method = "MF";
            if (method == "MR")
                method = "MI";
            if (method == "AA")
                method = "AD";
            if (method == "SB")
                method = "SF";

            if (method == "MF" || method == "MI"/*bpDeprMethod::MacrsFormula*/ )
            {
                if (pct == 100)
                    return "MAF";
                else
                    return "MF";
            }
            else if (method == "MT" /*bpDeprMethod::MacrsTable*/ )
            {
                return "MT";
            }
            else if (method == "AD" /*bpDeprMethod::AdsSlMacrs:                     */ )
            {
                return "MAF";
            }
            else if (method == "AT" /*bpDeprMethod::AcrsTable:                      */ )
            {
                return "AT";
            }
            else if (method == "SA" /*bpDeprMethod::StraightLineAltAcrsFormula:     */ )
            {
                return "ASF";
            }
            else if (method == "ST" /*bpDeprMethod::StraightLineAltAcrsTable:       */ )
            {
                return "AST";
            }
            else if (method == "SL" /*bpDeprMethod::StraightLine:                   */ )
            {
                return "SL";
            }
            else if (method == "SF" /*bpDeprMethod::StraightLineFullMonth:          */ )
            {
                return "SL";
            }
            else if (method == "SH" /*bpDeprMethod::StraightLineHalfYear:           */ )
            {
                return "SL";
            }
            else if (method == "SD" /*bpDeprMethod::StraightLineModHalfYear:        */ )
            {
                return "SL";
            }
            else if (method == "DC" /*bpDeprMethod::DeclBal:                        */ )
            {
                return "DBn";
            }
            else if (method == "DI" /*bpDeprMethod::DeclBalHalfYear:                */ )
            {
                return "DBn";
            }
            else if (method == "DE" /*bpDeprMethod::DeclBalModHalfYear:             */ )
            {
                return "DBn";
            }
            else if (method == "DB" /*bpDeprMethod::DeclBalSwitch:                  */ )
            {
                return "DBs";
            }
            else if (method == "DH" /*bpDeprMethod::DeclBalHalfYearSwitch:          */ )
            {
                return "DBs";
            }
            else if (method == "DD" /*bpDeprMethod::DeclBalModHalfYearSwitch:       */ )
            {
                return "DBs";
            }
            else if (method == "YS" /*bpDeprMethod::SumOfTheYearsDigits:            */ )
            {
                return "SYD";
            }
            else if (method == "YH" /*bpDeprMethod::SumOfTheYearsDigitsHalfYear:    */ )
            {
                return "SYD";
            }
            else if (method == "YD" /*bpDeprMethod::SumOfTheYearsDigitsModHalfYear: */ )
            {
                return "SYD";
            }
            else if (method == "RV" /*bpDeprMethod::RemValueOverRemLife:            */ )
            {
                return "RV";
            }
            // Canadian BEGIN !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            else if (method == "DM" /*bpDeprMethod::CdnDeclBal         :            */ )
            {
                return "cdnDBn";
            }
            else if (method == "DY" /*bpDeprMethod::CdnDeclBalHalfYear :            */ )
            {
                return "cdnDBn";
            }
            else if (method == "DL" /*bpDeprMethod::CdnDeclBalFullMonth:            */ )
            {
                return "cdnDBn";
            }
            // Canadian END ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            else if (method == "OC" /*bpDeprMethod::OwnDepreciationCalculation:     */ )
            {
                return "~FAS~OC";
            }
            else if (method == "NO" /*bpDeprMethod::DoNotDepreciate:                */ )
            {
                return "NON";
            }
            // QinZ: new 5 methods
            else if (method == "MA" /*bpDeprMethod::DoNotDepreciate:                */ )
            {
                return "MA";
            }
            else if (method == "MB" /*bpDeprMethod::DoNotDepreciate:                */ )
            {
                return "MB";
            }
            else if (method == "MR" /*bpDeprMethod::DoNotDepreciate:                */ )
            {
                return "MR";
            }
            else if (method == "AA" /*bpDeprMethod::DoNotDepreciate:                */ )
            {
                return "AA";
            }
            else if (method == "SB" /*bpDeprMethod::DoNotDepreciate:                */ )
            {
                return "SB";
            }
            return "";
        }

        public short ConvertPropType(string aType)
        {
            switch (aType)
            {
                case "P"/*PropType::PersonalGeneral*/:
                    return (short)PropType.PersonalGeneral;
                case "Q"/*PropType::PersonalListed*/:
                    return (short)PropType.PersonalListed;
                case "V"/*PropType::VintageAccount*/:
                    return (short)PropType.VintageAccount;
                case "R"/*PropType::RealGeneral*/:
                    return (short)PropType.RealGeneral;
                case "S"/*PropType::RealListed*/:
                    return (short)PropType.RealListed;
                case "C"/*PropType::RealConservation*/:
                    return (short)PropType.RealConservation;
                case "E"/*PropType::RealEnergy*/:
                    return (short)PropType.RealEnergy;
                case "F"/*PropType::RealFarms*/:
                    return (short)PropType.RealFarms;
                case "A"/*PropType::Automobile*/:
                    return (short)PropType.Automobile;
                case "Z"/*PropType::Amortizable*/:
                    return (short)PropType.Amortizable;
                case "H"/*PropType::RealLowIncomeHousing*/:
                    return (short)PropType.RealLowIncomeHousing;
                case "D"/*PropType::PersonalGeneral*/:
                    return (short)PropType.Depreciable;
                case "N"/*PropType::PersonalListed*/:
                    return (short)PropType.NonDepreciable;
                case "T"/* KENT PropType::LtTrucksAndVans*/:
                    return (short)PropType.LtTrucksAndVans;
            }
            return 0;  // Default return value.
        }
    }
}
