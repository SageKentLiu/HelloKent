using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFACalcEngine
{
    public class BADefaultCalcLookup : IBADefaultCalcLookup, IBACalcLookUp
    {

        public bool GetLuxuryLimits(DateTime dtDate, LUXURYLIMIT_TYPE eLuxuryFlag, ref double dYear1Val, ref double dYear2Val, ref double dYear3Val, ref double dYear4Val, out bool pVal)
        {
            int Date_Index = 0;
            double DateComp;

            double[,] YearMax = new double[,]
            {
                { 2660,   /* Max constants for MACRS purchased */
                  4200,   /* between 01/01/89 and 12/31/90.    */
                  2550,
                  1475 },

                { 2560,   /* Max constants for declining balance */
                  4100,   /* purchased after 12/31/86 */
                  2450,
                  1475 },

                { 3200,   /* Max constants for ACRS purchased after */
                  4800,
                  4800,
                  4800 },  /* 04/02/85  */

                { 4100,   /* Max constants for ACRS purchased after */
                  6200,
                  6200,
                  6200 }, /* 12/31/84  */

                { 4000,   /* Max constants for ACRS purchased after */
                  6000,
                  6000,
                  6000}, /* 06/18/84  */

                { 2660,   /* Max constants when purchased after */
                  4300,   /* 12/31/90                           */
                  2550,
                  1575 },

                { 2760,   /* Max constants when purchased after */
                  4400,   /* 12/31/90                           */
                  2650,
                  1575 },

                { 2860,   /* Max constants when purchased after 12/31/92 */
                  4600,
                  2750,
                  1675 },

                { 2960,   /* Max constants when purchased after */
                  4700,   /* 12/31/93 RWH 95.1                  */
                  2850,
                  1675 },

                { 3060,   /* Max constants when purchased after */
                  4900,   /* 12/31/94 RWH 95.1                  */
                  2950,
                  1775 },

                { 3160,   /* Max constants when purchased after */
                  5000,   /* 12/31/96 MJB 97.1 Tax Update       */
                  3050,
                  1775 },

                { 3160,   /* Max constants when purchased after */
                  5000,   /* 12/31/97 MJB 98.1 Tax Update       */
                  2950,
                  1775 },

                { 3060,   /* Max constants when purchased after */
                  5000,   /* 12/31/98 MJB 99.1 Tax Update       */
                  2950,
                  1775 },

                { 3060,   /* Max constants when purchased after    */
                  4900,   /* 12/31/99 RDBJ 2000.1 Tax Update       */
                  2950,
                  1775 },

		        { 2960,	  /* KENT */
		          4800,
		          2850,
		          1675 },

 		        { 2960,	  /* KENT for 2005 */
		          4700,
		          2850,
		          1675 },

		        { 2960,	  /* 2006 */
		          4800,
		          2850,
		          1775 },

		        { 3060,	  /* 2007 */
		          4900,
		          2850,
		          1775 },

		        { 2960,	  /* 2008 */
		          4800,
		          2850,
		          1775 },

		        { 2960,	  /* 2009 */
		          4800,
		          2850,
		          1775 },

		        { 3060,	  /* 2010 */
		          4900,
		          2950,
		          1775 },

		        { 3060,	  /* 2011 */
		          4900,
		          2950,
		          1775 },

		        { 3160,	  /* 2012 */
		          5100,
		          3050,
		          1875 },

		        { 3160,	  /* after 2012 */
		          5100,
		          3050,
		          1875 }	
	        };

           double[,] YearMaxLtTV = new double[,]
           {
                { 3360,   // for 2003
                  5400,   
                  3250,
                  1975 },

                { 3260,   // for 2004
                  5300,   
                  3150,
                  1875 },

                { 3260,   // for 2005
                  5200,   
                  3150,
                  1875 },

                { 3260,   // for 2006
                  5200,   
                  3150,
                  1875 },

		        { 3260,
		          5200,
		          3050,
		          1875 },  // for 2007

		        { 3160,
		          5100,
		          3050,
		          1875 },  // for 2008

		        { 3060,
		          4900,
		          2950,
		          1775 },  // for 2009

		        { 3160,
		          5100,
		          3050,
		          1875 },  // for 2010

		        { 3260,
		          5200,
		          3150,
		          1875 },  // for 2011

                { 3360,   
                  5300,   
                  3150,
                  1875 }, // for 2012

                 { 3360,   
                  5400,   
                  3250,
                  1975 } // after 2012   
	        };


           if (dYear1Val == null || dYear2Val == null || dYear3Val == null || dYear4Val == null )
               throw new Exception("Error.");

           dYear1Val = 0;
           dYear2Val = 0;
           dYear3Val = 0;
           dYear4Val = 0;
           pVal = true;

           if (eLuxuryFlag != LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_LUXURYCAR && eLuxuryFlag != LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_NOTAPPLY &&
                 eLuxuryFlag != LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_LTTRUCKSANDVANS)
                return false;

           if (eLuxuryFlag == LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_NOTAPPLY)
            {
                return true;
            }

            if ( pVal == null )
            {
                return false;
            }
            DateComp = ( dtDate.Year * 10000) + (dtDate.Month * 100) + dtDate.Day;

	        if ( DateComp > (double) 20021231L)
	        {
                if (eLuxuryFlag == LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_LUXURYCAR)
		        {
			        if ( DateComp > (double) 20121231L)
				        Date_Index = 23;
			        else if ( DateComp > (double) 20111231L)
				        Date_Index = 22;
			        else if ( DateComp > (double) 20101231L)
				        Date_Index = 21;
			        else if ( DateComp > (double) 20091231L)
				        Date_Index = 20;
			        else if ( DateComp > (double) 20081231L)
				        Date_Index = 19;
			        else if ( DateComp > (double) 20071231L)
				        Date_Index = 18;
			        else if ( DateComp > (double) 20061231L)
				        Date_Index = 17;
			        else if ( DateComp > (double) 20051231L)
				        Date_Index = 16;
			        else if ( DateComp > (double) 20041231L)
				        Date_Index = 15;
			        else if ( DateComp > (double) 20031231L)
				        Date_Index = 14;
			        else if ( DateComp > (double) 20021231L)
				        Date_Index = 13;
		        }
                else if (eLuxuryFlag == LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_LTTRUCKSANDVANS)
		        {
			        if ( DateComp > (double) 20121231L)
				        Date_Index = 10;
			        else if ( DateComp > (double) 20111231L)
				        Date_Index = 9;
			        else if ( DateComp > (double) 20101231L)
				        Date_Index = 8;
			        else if ( DateComp > (double) 20091231L)
				        Date_Index = 7;
			        else if ( DateComp > (double) 20081231L)
				        Date_Index = 6;
			        else if ( DateComp > (double) 20071231L)
				        Date_Index = 5;
			        else if ( DateComp > (double) 20061231L)
				        Date_Index = 4;
			        else if ( DateComp > (double) 20051231L)
				        Date_Index = 3;
			        else if ( DateComp > (double) 20041231L)
				        Date_Index = 2;
			        else if ( DateComp > (double) 20031231L)
				        Date_Index = 1;
			        else if ( DateComp > (double) 20021231L)
				                Date_Index = 0;
			        dYear1Val = YearMaxLtTV[Date_Index,0];
			        dYear2Val = YearMaxLtTV[Date_Index,1];
			        dYear3Val = YearMaxLtTV[Date_Index,2];
			        dYear4Val = YearMaxLtTV[Date_Index,3];
			        return true;
		        }
	        }
            else if ( DateComp > (double) 19991231L )      // RDBJ - 2000.1 Tax Update - Autos PIS in 2000
                Date_Index = 13;
            else if ( DateComp > (double) 19981231L )      // MJB - 99.1 Tax Update - Autos PIS in 99
                Date_Index = 12;
            else if ( DateComp > (double) 19971231L )      // MJB - 98.1 Tax Update - Autos PIS in 98
                Date_Index = 11;
            else if ( DateComp > (double) 19961231L )      // MJB - 97.1 Tax Update - Autos PIS in 97
                Date_Index = 10;
            else if ( DateComp > (double) 19941231L )      // RWH - Tax changes 95.1 - Autos PIS in 95
                Date_Index = 9;
            else if ( DateComp > (double) 19931231L )      // RWH - Tax changes 95.1 - Autos PIS in 94
                Date_Index = 8;
            else if ( DateComp > (double) 19921231L )
                Date_Index = 7;
            else if (DateComp > (double) 19911231L)
                Date_Index = 6;
            else if (DateComp > (double) 19901231L)
                Date_Index = 5;
            else if (DateComp > (double) 19881231L)
                Date_Index = 0;
            else if(DateComp > (double) 19861231L)
                Date_Index = 1;
            else if (DateComp > (double) 19850402L)
                Date_Index = 2;
            else if (DateComp > (double) 19841231L)
                Date_Index = 3;
            //else                                  // AJB 06/05/91
            //  Date_Index = 4;                     // This was ignoring the limits
            else if (DateComp > (double) 19840618L)   // for 06/18/84
                Date_Index = 4;                     // Any date prior to 06/19/84
            else
            {
                return true;;
            }
            dYear1Val = YearMax[Date_Index,0];
            dYear2Val = YearMax[Date_Index,1];
            dYear3Val = YearMax[Date_Index,2];
            dYear4Val = YearMax[Date_Index,3];
            return true;        
        }

        public bool GetSection179Limits(out bool pVal)
        {
            pVal = true;
            return true;
        }
    }
}
