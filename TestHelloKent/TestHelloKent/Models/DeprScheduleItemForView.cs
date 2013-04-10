using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TestHelloKent.Models
{
    public class DeprScheduleItemForView
    {
        public string URL { get; set; }

        public string PropertyType { get; set; }
        [DataType(DataType.Date)]
        public DateTime PlaceInServiceDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        public decimal AcquisitionValue { get; set; }
        public string DepreciationMethod { get; set; }
        public string DepreciationPercent { get; set; }
        public int EstimatedLife { get; set; }
        public double Section179 { get; set; }
        public short Bonus911Percent { get; set; }
        public double SalvageDeduction { get; set; }
        public decimal ITCAmount { get; set; }
        public decimal ITCReduce { get; set; }
        public string Convention { get; set; }
        [DataType(DataType.Date)]
        public DateTime RunDate { get; set; }
    } 
}