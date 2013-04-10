using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFACalcEngine
{
    public interface IBASwitchDepr
    {
        bool SwitchRequired { get; set; }
        short SwitchYearNum { get; set; }
        bool IsShortYear { get; set; }
        IBADeprMethod SwitchMethod { get; set; }

        bool CheckForSwitch { get; }
        string SwitchMethodName { get; set; }
        string SwitchFlag { get; set; }


	    bool GetCurrentMethod(string flags, out IBADeprMethod pVal);
    }
}
