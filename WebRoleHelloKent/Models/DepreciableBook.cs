using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebRoleHelloKent.Models
{
    public class DepreciableBook
    {
        public string PropertyType { get; set; }
        public DateTime PlaceInServiceDate { get; set; }
        public double AcquiredValue { get; set; }
        public string DepreciateMethod { get; set; }
        public int DepreciatePercent { get; set; }
        public int EstimatedLife { get; set; }
        public double Section179 { get; set; }
        public double ITCAmount { get; set; }
        public double ITCReduce { get; set; }
        public double SalvageDeduction { get; set; }
        public short Bonus911Percent { get; set; }
        public string Convention { get; set; }
        public DateTime RunDate { get; set; }
    }


}