using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFACalcEngine
{
    public interface IBAPeriodDetailDeprInfo
    {
        DateTime FiscalYearStartDate  { get; set; }
        DateTime FiscalYearEndDate    { get; set; }

        decimal  FiscalYearBeginAccum { get; set; }
        decimal  FiscalYearEndAccum   { get; set; }
        decimal  FiscalYearDeprAmount { get; set; }

        DateTime PeriodStartDate      { get; set; }
        DateTime PeriodEndDate        { get; set; }
                     
        decimal  PeriodBeginAccum     { get; set; }   // refer to the fiscal year start date
        decimal  PeriodEndAccum       { get; set; }
        decimal  PeriodDeprAmount     { get; set; }
        string   CalcFlags             { get; set; }
    }
}
