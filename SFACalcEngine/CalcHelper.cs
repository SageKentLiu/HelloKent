using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFABusinessTypes;
using SFACalendar;


namespace SFACalcEngine
{
    // calculation in bpFASCalcEngineAPP
    public class CalcHelper
    {
        bool ITC_Recapture(IBADeprScheduleItem schedule, IBACalendar calendar, double ITCAmount, double ITCFactor, out double Recap, out double RecapYTD, out double Addbak, out double tablePct)
        {
            double[] table_ptr;
            double[] table1 = new double[] { 100, 100, 100, 0, 0, 0, 0 };
            double[] table2 = new double[] { 100, 100, 100, 50, 50, 0, 0 };
            double[] table3 = new double[] { 100, 100, 100, 66.6, 66.6, 33.3, 33.3 };
            double[] table4 = new double[] { 100, 66, 33, 0, 0, 0, 0 };
            double[] table5 = new double[] { 100, 80, 60, 40, 20, 0, 0 };
            bool hr;
            string meth;
            double estLife;

            Recap = 0;
            RecapYTD = 0;
            Addbak = 0;
            tablePct = 0;
            if (schedule == null || calendar == null)
            {
                return false;
            }

            meth = schedule.DeprMethod;
            estLife = schedule.DeprLife;

            if (string.Compare(meth, "MF") == 0 ||
                 string.Compare(meth, "MT") == 0 ||
                 string.Compare(meth, "MAF") == 0 ||
                 string.Compare(meth, "MAT") == 0 ||
                 string.Compare(meth, "MSF") == 0 ||
                 string.Compare(meth, "MST") == 0 ||
                 string.Compare(meth, "AT") == 0 ||
                 string.Compare(meth, "AST") == 0 ||
                 string.Compare(meth, "ASF") == 0 ||
                 string.Compare(meth, "~FAS~OC") == 0)
            {
                if (((decimal)(estLife) - 3) == 0)
                    table_ptr = table4;
                else
                    table_ptr = table5;
            }
            else
            {
                if (((decimal)(estLife) - 3) >= 0 && ((decimal)(estLife) - 5) < 0)
                    table_ptr = table1;
                else if (((decimal)(estLife) - 5) >= 0 && ((decimal)(estLife) - 7) < 0)
                    table_ptr = table2;
                else
                    table_ptr = table3;
            }
            /* find when this asset is disposed use for index into recap tables or 0 if out of range */

            long yearCount;
            if (!(YearDisposed(schedule, calendar, true, out yearCount)))
                yearCount = 0;

            if (yearCount > 6)
                tablePct = 0;
            else
            {
                tablePct = table_ptr[yearCount];
            }

            RecapYTD = ITCAmount * (tablePct / 100);
            Recap = RecapYTD;
            Addbak = Recap * ITCFactor;
            return true;
        }

        public bool ComputeITCRecap(IBADeprScheduleItem schedule, IBACalendar calendar, DateTime RunDate,
                                    double ITCAmount, out double baseITCFactor, out double ITCFactor, out double TablePct, out double Recap, out double AddBack)
        {
            double RecapYTD;
            //RDBJ     double pct;

            baseITCFactor = 0;
            ITCFactor = 0;
            TablePct = 0;
            Recap = 0;
            AddBack = 0;

            if (schedule == null || calendar == null)
                return false;

            if (schedule.BookType == bpblBookTypeEnum.bpblBookTaxBook && ITCAmount >= 0.0)
            {
                baseITCFactor = schedule.CalculateITCBasisReductionFactor();
                ITCFactor = baseITCFactor;
                //RDBJ         if ( FAILED(hr = GetBusinessUsePct(m_schedule, m_calendar, RunDate, &pct)) )
                //RDBJ             return hr;
                ITC_Recapture(schedule, calendar, ITCAmount, ITCFactor, out Recap, out RecapYTD, out AddBack, out TablePct);
                Round1Number(ref Recap);
                Round1Number(ref AddBack);
            }
            AddBack = Recap * ITCFactor;
            return true;
        }

        public bool IsCustomMethod (string DeprMeth)
        {
            if ( string.Compare(DeprMeth, "MF") == 0  ||
                 string.Compare(DeprMeth, "MT") == 0  ||
                 string.Compare(DeprMeth, "MAT") == 0 ||
                 string.Compare(DeprMeth, "MAF") == 0 ||
                 string.Compare(DeprMeth, "MST") == 0 ||
                 string.Compare(DeprMeth, "MSF") == 0 ||
                 string.Compare(DeprMeth, "AT") == 0  ||
                 string.Compare(DeprMeth, "AST") == 0 ||
                 string.Compare(DeprMeth, "ASF") == 0 ||
                 string.Compare(DeprMeth, "SL") == 0  ||
                 string.Compare(DeprMeth, "DBn") == 0 ||
                 string.Compare(DeprMeth, "DBs") == 0 ||
                 string.Compare(DeprMeth, "SYD") == 0 ||
                 string.Compare(DeprMeth, "RV") == 0 ||
                 string.Compare(DeprMeth, "~FAS~OC") == 0 ||
                 string.Compare(DeprMeth, "UOP") == 0 ||
                 string.Compare(DeprMeth, "AMZ") == 0 ||
                 string.Compare(DeprMeth, "NON") == 0 )
                return false;
	        //since in the ui the user can make only 2 letter custom methods, we are adding 
	        //the canadian methods as a keyword always
	        //we cannot use registry entries anymore, and to pass the app config to this dll is 
	        //too complicated
        // Canadian BEGIN !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //if (bpCdnManager::isCanadian()) {
                if ( string.Compare(DeprMeth, "cdnDBn") == 0 ) {
                    return false;
                //}
            }
        // Canadian END ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            return true;
        }


        public bool CalculateBasis(IBADeprScheduleItem schedule, bool AceFlag, out double ITCReductionAmount, out bool basisIsReduced, out double finalBasis, out double pVal)
        {
            double cyITCAmount;
            double bonus;
            bool bonusFlag;
            bool hr;
            string meth;
            short propType;
            double adjValue;
            double salvage;
            bpblBookTypeEnum bookType;
            double s179;

            ITCReductionAmount = 0;
            pVal = 0;
            basisIsReduced = false;
            finalBasis = 0;
            
            if (schedule == null)
            {
                return false;
            }

            bonusFlag = schedule.BonusFlag;
            bonus = schedule.BonusAmount;
            meth = schedule.DeprMethod;
            propType = (short)schedule.PropertyType;
            adjValue = schedule.AdjustedCost;
            salvage = schedule.SalvageDeduction;
            bookType = schedule.BookType;
            s179 = schedule.Section179;

            if ( string.Compare(meth, "~FAS~OC") == 0 )
            {
                bonusFlag = false;
                bonus = 0;
            }
            ITCReductionAmount = 0;
            cyITCAmount = schedule.CalculateITCBasisReductionAmount(AceFlag);
            ITCReductionAmount = (double)(cyITCAmount);

            /* COST BASE = (ACQ VAL * BUS % USE)  -  SECT 179  - ITC RED AMT */
            /* ADJUST THE COSTBASE VALUE FOR METHODS S,H,Y,X AND CUSTOM CODES. KAZEMI */

            if ( string.Compare(meth, "SL") == 0 ||
                 string.Compare(meth, "SYD") == 0 ||
                 string.Compare(meth, "RV") == 0 )
            {
                if ( propType == (short)PropType.VintageAccount )
                    pVal = adjValue - ITCReductionAmount - bonus;
                else
                    pVal = adjValue - ITCReductionAmount - salvage - bonus - s179; //added s179 for 30% handling

                /* reset for switch for ace */
                //if(intarray[DEPRMETH] == RV_FULLMONTH&&realarray[SECTN179]&&intarray[BOOKNUM]==ACEBOOKNUM)
                //  realarray[COSTBASE] = (realarray[ACQVALUE] * (intarray[PRCNTBUS]/100.0)) - realarray[SECTN179] -  realarray[ITCRAMT];
                //  AJB - 05/20/91
                if ( string.Compare(meth, "RV") == 0 && AceFlag && bookType == bpblBookTypeEnum.bpblBookACEBook )
                    pVal = adjValue - s179 -  ITCReductionAmount - bonus;
            }
            else
            {
                if ( IsCustomMethod(meth) )
                    pVal = adjValue - ITCReductionAmount - salvage - s179 - bonus;
                else
                    pVal = adjValue - s179 -  ITCReductionAmount - bonus;
            }

            /* DETERMINE IF THERE HAS BEEN A COST BASIS REDUCTION  */
            /* ACQ VALUE <> COST BASIS                             */

            basisIsReduced = false;

            if ( pVal != adjValue - bonus )
                basisIsReduced = true;

            finalBasis = pVal;

            if (bookType == bpblBookTypeEnum.bpblBookEandPBook )
            {
                finalBasis += s179;
            }
            return true;
        }

        public bool isMidQtrAsset(IBADeprScheduleItem schedule)
        {
            short propType;
            string meth;
            double life;
	        short ddbPct;

            bool pVal = false;
            if ( schedule == null )
            {
                return false;
            }

            propType = schedule.PropertyType;
            meth = schedule.DeprMethod;
            life = schedule.DeprLife;
	        ddbPct = schedule.DeprPercent;

            switch(propType)
            {
                case (short)PropType.RealGeneral:
                case (short)PropType.RealListed:
                case (short)PropType.RealConservation:
                case (short)PropType.RealEnergy:
                case (short)PropType.RealFarms:
                    if ( (meth == "MF" && ddbPct == 100 && life == 5) ||
				         (meth == "AD" && life == 9) )
				         return true;
                    if ( string.Compare (meth, "MF") == 0 )
                    {
                        if ((int)(life) == 5 || (int)(life) == 10 || (int)(life) == 15 || (int)(life) == 20)
                        {
                            pVal = true;
                        }
                    }
                    return pVal;

        //RDBJ         case LEASE:
        //RDBJ         case NON_CAPITAL:
                case (short)PropType.RealLowIncomeHousing:
                case (short)PropType.Amortizable:
                case (short)PropType.VintageAccount:
                    return pVal;

                default:
                    break;
            }

            if ( string.Compare (meth, "MF") == 0 ||
                 string.Compare (meth, "MT") == 0 ||
                 string.Compare (meth, "MAF") == 0 )
            {
                pVal = true;
            }
            return pVal;
        }

        public bool ComputeFullCostBasis(IBADeprScheduleItem schedule, IBACalendar cal, bool AceFlag, bool ForMidQtr, out bool InLastQtr, out double Basis)
        {
            bool hr;
            double ITCReductionAmount;
            bool basisIsReduced;
            double finalBasis;
            double costBasis;
            bool isMidQtr;
            DateTime PIS = schedule.PlacedInServiceDate;
            IBAFiscalYear FY;
            DateTime start, end;

            Basis = 0;
            InLastQtr = false;
            if (schedule == null || cal == null)
            {
                return false;
            }

            InLastQtr = false;
            Basis = 0;

            if (!(hr = CalculateBasis(schedule, AceFlag, out ITCReductionAmount, out basisIsReduced, out finalBasis, out costBasis)) ||
                !(hr = cal.GetFiscalYear(PIS, out FY)) )
             return hr;
            isMidQtr = isMidQtrAsset(schedule);
            start = FY.YRStartDate;
            end = FY.YREndDate;

            if (ForMidQtr)
            {
                Basis = costBasis;
                InLastQtr = false;

                if (isMidQtr)
                {
                    long dayCount;
                    long RemDays;
                    long QuarterDays;

                    dayCount = (long)((end - start).TotalDays);
                    RemDays = (int)(dayCount - ((dayCount / 4) * 4));
                    QuarterDays = (int)(dayCount / 4);

                    if (RemDays == 2 || RemDays == 3)
                    {
                        QuarterDays++;
                    }
                    if (PIS >= end.AddDays ( -QuarterDays + 1))
                    {
                        InLastQtr = true;
                    }
                }
                else
                    Basis = 0.0;
            }
            else
            {
                Basis = costBasis;
                InLastQtr = false;
            }

            schedule.ITCBasisReduction = ITCReductionAmount;


            return true;
        }

        void Round1Number(ref double dbl)
        {
            double intpart;

            if (dbl < 0.0)
            {
                if (dbl < -999999999.99)
                    dbl = 0;
                dbl = -(((0 - dbl) + 0.0059) * 100.0);
            }
            else
                dbl = ((dbl + 0.0059) * 100.0);

            intpart = dbl;
            intpart = (long)intpart;
            dbl = (double)(intpart / 100.0);
        }

        bool YearDisposed(IBADeprScheduleItem deprScheduleItem, IBACalendar calendar, bool UseITCRules, out long pVal)
        {
            pVal = 0;
            return true;
        }
    }
}
