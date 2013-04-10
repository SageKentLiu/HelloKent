using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFACalcEngine
{
    public static class Currency
    {
        private static int    g_lDecimalPlaces = 2;
        private static double g_dblRoundingFactor = 0.501;
        private static double g_dblScaledRoundingFactor = 100.0;

        /////////////////////////////////////////////////////////////////////////////
        // Format a double to the globally set number of decimal places
        public static double FormatCurrency(double value)
        {
            double intpart;

            if (value < 0)
            {
                intpart = ((-value) * g_dblScaledRoundingFactor) + g_dblRoundingFactor;
                intpart = (long)intpart;
                intpart = -intpart;
            }
            else
            {
                intpart = (value * g_dblScaledRoundingFactor) + g_dblRoundingFactor;
                intpart = (long)intpart;
            }
            return intpart / g_dblScaledRoundingFactor;
        }


    }
}
