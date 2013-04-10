using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFACalcEngine
{
    public interface IBAACEInformation
    {
        bool UseACEHandling { get; }
        decimal ACEBasis { get; set; }
        double ACELife { get; set; }
    }
}
