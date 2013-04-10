using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFACalcEngine
{
    public interface IBACalcLookUp
    {
        bool GetLuxuryLimits(
                         DateTime dtDate,
                         LUXURYLIMIT_TYPE eLuxuryFlag,
                        ref double dYear1Val,
                        ref double dYear2Val,
                        ref double dYear3Val,
                        ref double dYear4Val,
                        out bool pVal);
        bool GetSection179Limits(out bool pVal);
    }

    public interface IBADefaultCalcLookup
    {

    }
}
