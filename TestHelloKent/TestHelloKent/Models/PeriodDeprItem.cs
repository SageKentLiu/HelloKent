using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestHelloKent.Models
{
    public class PeriodDeprItem
    {
        public short FYNum { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double BeginYearAccum { get; set; }
        public double BeginYearYTDExpense { get; set; }
        public double DeprAmount { get; set; }
        public double CurrntYTDDepr { get; set; }
        public double CurrentAccumDepr { get; set; }
        public string CalcFlags { get; set; }
    }
}