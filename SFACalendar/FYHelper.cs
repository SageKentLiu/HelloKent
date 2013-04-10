using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFACalendar
{
    class _PeriodInfo
    {
        public DateTime dtStartDate;
        public DateTime dtEndDate;
        public bool IsIdle;
        public short iPeriodNumber;
        public short iWeight;
        public short iRemainWeight;
        public short iFutureWeight;
    };

    abstract class _IFYHelper
    {
        public abstract short MaxWeights();  // Maximum value of weights
        public abstract short TotalFYWeights(short periodCount, List<_PeriodInfo> info, DateTime start, DateTime end, BAFASFiscalYear obj);  // Total Weights for Fiscal Year
        public abstract short PeriodCount(BAFASFiscalYear obj);  // Number of periods in this year
        public abstract List<_PeriodInfo> PeriodInfo(BAFASFiscalYear obj);  // Returns allocated structure of periods.
        public abstract DateTime DeemedDate(DateTime dt, BAFASFiscalYear obj);  // Calculate the Deemed start date for a given date.
        public abstract DateTime GetMidPeriodDate(DateTime dtDate, short periodCount, List<_PeriodInfo> info, BAFASFiscalYear obj);
        public abstract DateTime FiscalYearEndDate(int iYear, int m_iFYEndMonthEnum, BAFASFiscalYear obj);
        public abstract DateTime GetMidYearDate(short periodCount, List<_PeriodInfo> info, BAFASFiscalYear obj);
        public abstract bool IsShortYear(BAFASFiscalYear obj); // Determine if we are in a short year.

    }

    class CMonthlyHelper : _IFYHelper
    {
        public override short MaxWeights() { return 12; }
        public override short TotalFYWeights(short periodCount, List<_PeriodInfo> info, DateTime start, DateTime end, BAFASFiscalYear obj)
        {
            short iBeginMonth;
            short iEndMonth;
            short count;
            bool bShortYear;

            bShortYear = obj.IsShortYear;
            if (bShortYear)
            {
                iBeginMonth = (short)startMonth(obj);			// KENT fix NFaus-00129	Month(start);
                iEndMonth = (short)endMonth(obj);				// KENT fix NFaus-00129	Month(end);

                if (iEndMonth >= iBeginMonth)
                {
                    count = (short)(iEndMonth - iBeginMonth + 1);
                }
                else
                {
                    count = (short)(12 + iEndMonth - iBeginMonth + 1);
                }
                if (count > 12)
                {
                    count = 12;
                }
            }
            else
                count = 12;
            return count;
        }

        public override short PeriodCount(BAFASFiscalYear obj)
        {
            short iBeginMonth;
            short iEndMonth;
            short count;
            bool bShortYear = obj.IsShortYear;
            DateTime dtStartDate = obj.YRStartDate;
            DateTime dtEndDate = obj.YREndDate;

            if (bShortYear)
            {
                iBeginMonth = (short)startMonth(obj);			// KENT fix NFaus-00129	Month(dtStartDate);
                iEndMonth = (short)endMonth(obj);				// KENT fix NFaus-00129	Month(dtEndDate);

                if (iEndMonth >= iBeginMonth)
                {
                    count = (short)(iEndMonth - iBeginMonth + 1);
                }
                else
                {
                    count = (short)(12 + iEndMonth - iBeginMonth + 1);
                }
                if (count > 12)
                {
                    count = 12;
                }
            }
            else
                count = 12;
            return count;
        }

        public override List<_PeriodInfo> PeriodInfo(BAFASFiscalYear obj)
        {
            short i;
            short periodCount = PeriodCount(obj);
            List<_PeriodInfo> info;
            DateTime dtStartDate;
            DateTime dtEndDate;
            //    bool bShortYear;
            int totalWeights;

            info = new List<_PeriodInfo>(periodCount);

            dtStartDate = obj.YRStartDate;
            dtEndDate = obj.YREndDate;

            for (i = 0; i < periodCount; i++)
            {
                _PeriodInfo prdInfo;
                CalcPeriod(out prdInfo, dtStartDate, dtEndDate, (short)(i + 1), obj, periodCount);
                info.Add(prdInfo);
            }
            totalWeights = 0;

            for (i = 0; i < periodCount; i++)
            {
                totalWeights += info[i].iWeight;
            }

            for (i = 0; i < periodCount; i++)
            {
                if (i > 0)
                {
                    (info[i]).iFutureWeight = (short)(info[i - 1].iFutureWeight + info[i - 1].iWeight);
                }
                totalWeights -= info[i].iWeight;
                info[i].iRemainWeight = (short)totalWeights;
            }

            return info;
        }

        public override DateTime DeemedDate(DateTime dt, BAFASFiscalYear obj)
        {
            return dt;
        }

        public override DateTime GetMidPeriodDate(DateTime dtDate, short periodCount, List<_PeriodInfo> info, BAFASFiscalYear obj)
        {
            return new DateTime((dtDate.Year), (dtDate.Month), 16);
        }

        public override DateTime FiscalYearEndDate(int iYear, int m_iFYEndMonthEnum, BAFASFiscalYear obj)
        {
            return DateTimeHelper.GetEndOfMonth(iYear, m_iFYEndMonthEnum);
        }

        public override DateTime GetMidYearDate(short periodCount, List<_PeriodInfo> info, BAFASFiscalYear obj)
        {
            int iYear;
            int iMonth1;
            int iMonth2;
            int iMonth;
            int iMod;
            DateTime dtStartDate;
            DateTime dtEndDate;
            bool hr;
            short iFYEnd;
            bool isShort;
            short iFYNum;

            dtStartDate = obj.YRStartDate;
            dtEndDate = obj.YREndDate;
            iFYEnd = obj.FYEndMonth;
            isShort = obj.IsShortYear;
            iFYNum = obj.FYNum;

            iYear = (dtStartDate.Year);
            iMonth1 = StartMonth(dtStartDate, dtEndDate, (iFYNum == 1), (isShort == true), iFYEnd);
            iMonth2 = iFYEnd;


            if (iMonth2 > iMonth1)
            {
                iMonth = ((iMonth2 + iMonth1 + 1) / 2);
                iMod = ((iMonth2 + iMonth1 + 1) & 1);
            }
            else
            {
                if (iYear == (dtEndDate.Year) && iMonth1 == iMonth2)
                {
                    // One month short year.
                    iMod = 1;
                    iMonth = iMonth1;
                }
                else
                {
                    iMonth = ((iMonth1 + iMonth2 + 1 + 12) / 2);
                    if (iMonth > 12)
                        iMonth = (iMonth - 12);
                    iMod = ((iMonth2 + iMonth1 + 1 + 12) & 1);
                    if (iMonth <= iMonth2 && iYear != (dtEndDate.Year))
                        iYear++;
                }
            }

            if (iMod == 0)
            {
                return new DateTime(iYear, iMonth, 1);
            }
            return new DateTime(iYear, iMonth, 15);
        }

        int StartMonth(DateTime dtStartDate, DateTime dtEndDate, bool IsFirstYear, bool IsShortYear, short iFYEndMonthEnum)
        {
            int iYear;
            int iMonth;
            DateTime dtFirstDate;
            int StartMonth;

            //make 1st date of the month
            iYear = (dtStartDate.Year);
            iMonth = (dtStartDate.Month);
            dtFirstDate = new DateTime(iYear, iMonth, 1);

            StartMonth = 0;
            // if first year is a short year (due to business start date)
            if (IsFirstYear && IsShortYear)
                StartMonth = iMonth;
            else
            {
                // if the fiscal year is a regular year
                if (!IsShortYear)
                    StartMonth = iFYEndMonthEnum + 1;
                else
                {
                    // if any short year other than first year (due to fiscal end month change)
                    if ((dtStartDate - dtFirstDate).TotalDays > 6)
                        StartMonth = iMonth + 1;
                    else
                        StartMonth = iMonth;
                }
            }

            if (StartMonth > 12)
                StartMonth = 1;

            return StartMonth;
        }


        public override bool IsShortYear(BAFASFiscalYear obj)
        {
            DateTime dtStartDate;
            DateTime dtEndDate;
            short iFYNum;

            dtStartDate = obj.YRStartDate;
            dtEndDate = obj.YREndDate;
            iFYNum = obj.FYNum;

            if ((dtStartDate.Month) <= (dtEndDate.Month))
                return (((dtEndDate - dtStartDate).TotalDays < 358 && iFYNum > 1) ||
                    ((dtEndDate.Month) - (dtStartDate.Month) + 1 < 12 && iFYNum == 1)) ? true : false;
            else
                return (((dtEndDate - dtStartDate).TotalDays < 358 && iFYNum > 1) ||
                    ((dtEndDate.Month) - (dtStartDate.Month) + 13 < 12 && iFYNum == 1)) ? true : false;
        }

        public int startMonth(BAFASFiscalYear obj)
        {
            int iYear;
            int iMonth;
            int iStartMonth;
            short iFYNum;
            short iEMonth;
            DateTime dtDate;
            DateTime dtFirstDate;
            bool bRet;

            //make 1st date of the month
            dtDate = obj.YRStartDate;

            iYear = (dtDate.Year);
            iMonth = (dtDate.Month);
            dtFirstDate = new DateTime(iYear, iMonth, 1);

            // if first year is a short year (due to business start date)
            iFYNum = obj.FYNum;
            bRet = obj.IsShortYear;
            iStartMonth = iMonth;
            if (iFYNum == 1 && bRet)
            {
                iStartMonth = iMonth;
            }
            else
            {
                // if the fiscal year is a regular year
                if (!bRet)
                {
                    iEMonth = obj.FYEndMonth;
                    iStartMonth = (int)iEMonth + 1;
                }
                else
                {
                    // if any short year other than first year (due to fiscal end month change)
                    if ((dtDate - dtFirstDate).TotalDays > 15)
                    {
                        iStartMonth = iMonth + 1;
                    }
                }
            }

            if (iStartMonth > 12)
            {
                iStartMonth = 1;
            }

            return iStartMonth;
        }

        public int endMonth(BAFASFiscalYear obj)
        {
            short iEndMonth;

            iEndMonth = obj.FYEndMonth;
            return (int)iEndMonth;
        }

        protected bool CalcPeriod(out _PeriodInfo info, DateTime dtStartDate, DateTime dtEndDate, short iPeriodNum, BAFASFiscalYear obj, short periodCount)
        {
            int lYear;
            int iMonth;
            int iFYNum;

            info = new _PeriodInfo();

            iFYNum = obj.FYNum;

            lYear = (dtStartDate.Year);
            iMonth = ((dtStartDate.Month) + iPeriodNum - 1);

            if (iFYNum > 1)
            {
                if (dtStartDate.AddDays(7).Month != (dtStartDate.Month))
                    iMonth++;
            }
            //if ( Month(dtStartDate) == Month(dtEndDate))
            //{
            //    iMonth++;
            //}
            if (iMonth > 12)
            {
                // move to next year
                iMonth -= (12);
                lYear++;
            }
            if (iPeriodNum == 1)
            {
                info.dtStartDate = dtStartDate;
                info.dtEndDate = DateTimeHelper.GetEndOfMonth(lYear, iMonth);
            }
            else
            {
                info.dtStartDate = new DateTime(lYear, iMonth, 1);
                if (iPeriodNum == periodCount)
                    info.dtEndDate = dtEndDate;
                else
                    info.dtEndDate = DateTimeHelper.GetEndOfMonth(lYear, iMonth);
            }
            info.iPeriodNumber = iPeriodNum;
            info.iWeight = 1;
            info.IsIdle = false;

            if (info.dtEndDate <= dtEndDate)
            {
                return true;
            }
            return false;
        }

    };

    class CAAPHelper : _IFYHelper
    {
        public override short MaxWeights() { return 52; }
        public override short TotalFYWeights(short periodCount, List<_PeriodInfo> info, DateTime start, DateTime end, BAFASFiscalYear obj)
        {
            short weeks;
            short i;

            weeks = 0;
            for (i = 0; i < periodCount; i++)
            {
                if (info[i].dtStartDate <= end && info[i].dtEndDate >= start)
                    weeks = (short)(weeks + info[i].iWeight);
            }
            return weeks;

        /*    dblWeeks = (end - start + 1) / 7;
            weeks = short(dblWeeks);
            if ( (dblWeeks - weeks) * 7 >= 4 )
            {
                weeks++;
            }
            if ( weeks >= 53 )
            {
                weeks = 52;
            }
            return weeks;*/
        }

        public override short PeriodCount(BAFASFiscalYear obj)
        {
            short iNumWeeks;
            short iNumPeriods;
            DateTime dtTDate;
            bool bHas6WeeksPeriod = false;
            ECALENDARCYCLE_PDCOUNTING ePDCounting;
            ECALENDARCYCLE_CYCLETYPE eCycleType;
            DateTime startDate;
            DateTime endDate;
            bool bShortYear;

            ePDCounting = obj.PDCounting;
            startDate   = obj.YRStartDate;
            endDate     = obj.YREndDate;
            bShortYear  = obj.IsShortYear;
            eCycleType  = obj.CycleType;
            
            // Calculate Deemed Date, then calculate the number of weeks
            if ( ePDCounting == ECALENDARCYCLE_PDCOUNTING.PDCOUNT_FORWARD )
            {
                // Refer to the comment in GetPeriodByNum
                dtTDate = DeemedDate(startDate, obj).AddDays (+ 1);
                if ( Math.Abs((dtTDate - startDate).TotalDays) >= 4 )  // in >= equal sign added by KENT to fix NFaus-00130
                {
                    dtTDate = dtTDate.AddDays (+ 7);
                }
                iNumWeeks = (short)(((endDate - dtTDate).TotalDays + 1) / 7);
            }
            else
            {
                // For more detail, refer to the comment in GetPeriodByNum
                // check in this fiscal year upto the dtEndDate if it has a 6 week period
                dtTDate = DateTimeHelper.GetEndOfMonth(endDate.AddDays(- Constants.MAXDAYCHANGE));
                dtTDate = DeemedDate(dtTDate, obj);
                if ( (endDate - dtTDate).TotalDays >= Constants.FIFTYTHREEWEEKYEAR)
                {
                    bHas6WeeksPeriod = true;
                }
                if ( bShortYear )
                {
                    // If it is a short year then calculate the deemed start date
                    dtTDate = DateTimeHelper.GetEndOfMonth(startDate.AddDays(- 4));
                    dtTDate = DeemedDate (dtTDate, obj);
                    if (startDate < dtTDate && (dtTDate - startDate).TotalDays >= 4)
                        dtTDate = startDate;
                }
                else
                    dtTDate = startDate;
                // use this deemed start date and the end date to calculate
                // how many weeks in the fiscal year
                iNumWeeks = (short)(((endDate - dtTDate).TotalDays + 4) / 7);
        // KENT change the iNumWeeks == 5 to iNumWeeks >= 5 to fix NFaus-00130 second part
        // for count backward, it is count from the end date, having 6 weeks means the last
        // period has a extra week then whenever iNumWeeks >= 5, subtract this extra week.
        //		if ( bHas6WeeksPeriod && iNumWeeks == 5 && (eCycleType == ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FOURFIVEFOUR ||
		        if ( bHas6WeeksPeriod && iNumWeeks >= 5 && (eCycleType == ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FOURFIVEFOUR ||
                       eCycleType ==  ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FIVEFOURFOUR || eCycleType ==  ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_THIRTEENPERIOD) && 
			         ePDCounting == ECALENDARCYCLE_PDCOUNTING.PDCOUNT_BACKWARD )
			        iNumWeeks--;
            }

            iNumPeriods = WeekNumToPeriodNum((short)(iNumWeeks - 1), eCycleType);

            if ( eCycleType == ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_THIRTEENPERIOD && iNumPeriods > 13 )
            {
                return 13;
            }
            else if ( (eCycleType == ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FOURFOURFIVE ||
                       eCycleType == ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FOURFIVEFOUR ||
                       eCycleType == ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FIVEFOURFOUR) &&
                      iNumPeriods > 12)
            {
                return 12;
            }
            else if ( ePDCounting != ECALENDARCYCLE_PDCOUNTING.PDCOUNT_BACKWARD )
            {
                // The WeekNumToPeriodNum function is related to the base date for the calculation of
                // periods, for forward, the base date is at the beginning of a fiscal year, so layout
                // the periods, 445 still 445
                // For backwardoldmonthend, because consider the whole year, can still think of as forward
                // and the 6 week period may not appear depending on the EndDate
                if ( eCycleType == ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FOURFOURFIVE )
                {
                    iNumPeriods = WeekNumToPeriodNum((short)(iNumWeeks - 1), ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FOURFOURFIVE);
        // START KENT
        // this section of code is never passed
        // because DeemedDate(endDate - 7, obj) = denDate - 7
        // if ( 7 < 4 && bShorYear ) always false
        //            if ( endDate - DeemedDate (endDate - 7, obj) < 4 && (bShortYear) )
        //            {
        //                iNumPeriods--;
        //            }
        //            return iNumPeriods;
        // END KENT
                }
                else if ( eCycleType == ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FIVEFOURFOUR )
                {
                    iNumPeriods = WeekNumToPeriodNum((short)(iNumWeeks - 1), ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FIVEFOURFOUR);
        // START KENT
        //            if ( endDate - DeemedDate (endDate - 7, obj) < 4 && (bShortYear) )
        //            {
        //                iNumPeriods--;
        //            }
        //            return iNumPeriods;
        // END KENT
                }
                else
                {
        // START KENT
        //            if ( endDate - DeemedDate (endDate - 7, obj) < 4 && (bShortYear) )
        //            {
        //                iNumPeriods--;
        //            }
        //            return iNumPeriods;
        // END KENT
                }
        // START KENT
                return iNumPeriods;
        // END KENT
            }
            else
            {
                //int iDayDiff = Day(startDate) - 
                // for backward, the base date is at the end of the fiscal year, so to layout the periods
                // for 445 is 544 related to the end
                if ( bHas6WeeksPeriod )
                {
                    iNumWeeks--;
                }
                if ( eCycleType == ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FOURFOURFIVE )
                {
                    iNumPeriods = WeekNumToPeriodNum((short)(iNumWeeks - 1), ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FIVEFOURFOUR);
        /* this section of code is indentical to the else if() and comment out by KENT for NFaus-00123
                    if ( DeemedDate(startDate + 7, obj) - startDate < 4 && (bShortYear) && iNumPeriods > 1 )
                    {
                        iNumPeriods--;
                    }
                    return iNumPeriods;
        */
                }
                else if ( eCycleType == ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FIVEFOURFOUR )
                {
                    iNumPeriods = WeekNumToPeriodNum((short)(iNumWeeks - 1), ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FOURFOURFIVE);
        /* this section of code is indentical to the else if() and comment out by KENT for NFaus-00123
                    if ( DeemedDate(startDate + 7, obj) - startDate < 4 && (bShortYear) && iNumPeriods > 1 )
                    {
                        iNumPeriods--;
                    }
                    return iNumPeriods;
        */
                }
                else
                {
        /* this section of code is indentical to the else if() and comment out by KENT for NFaus-00123
                    if ( DeemedDate(startDate + 7, obj) - startDate < 4 && (bShortYear) && iNumPeriods > 1 )
                    {
                        iNumPeriods--;
                    }
                    return iNumPeriods;
        */
                }
        // START KENT for NFaus-0123
        // for short year, the dtTDate used to calc the num of weeks is possible introduced more than
        // a week(more than 7 days) to the num of weeks 
        //		if start date is some where between CD and ME
        //		LD     ME   CD		6 days between LD and ME and 3 days between CD and ME
        //      +-------+----+		LD -- last week date, ME -- month end,  CD -- close week date.
        // this may add one more period to the iNumPeriods.
        // if this is the case we need to subtract this extra period
                if ( (startDate - dtTDate).TotalDays >= 7 && (bShortYear) && iNumPeriods > 1 )
                {
                    iNumPeriods--;
                }
                return iNumPeriods;
        // END KENT 
            }
        }

         protected bool CalcPeriod(out _PeriodInfo info, DateTime dtStartDate, DateTime dtEndDate, ECALENDARCYCLE_PDCOUNTING ePDCounting,
                                    ECALENDARCYCLE_CYCLETYPE eCycleType, short iPeriodNum, BAFASFiscalYear obj, short periodCount, bool bShortYear)
         {
            DateTime dtDate;
            short iBegNumWeeks;
            short iEndNumWeeks;
            short iTmpPeriodNum = 0;
            bool bHas6WeeksPeriod = false;
	        short periodBias = 0;

             info = new _PeriodInfo();

           // make the start date for the calculation
            if ( ePDCounting == ECALENDARCYCLE_PDCOUNTING.PDCOUNT_FORWARD )
            {
                // When change from monthly to AAP, a user has two days to select, the ME or LD/CD
                // If a user selects LD/CD, the DeemedDate(dtStartDate) will give the same date as the
                // dtStartDate, then use this date to layout all the periods
                // If a user selects ME, the dtDate = DeemedDate(dtStartDate) will give either
                // LD or CD, according to the SAI calc doc, for counting forward always use close day
                // if it is LD then move to CD
                //     |<- 6 days ->|< 3 >|
                //   --+------------+-----+---------------------------------
                //    LD           ME    CD
                //    LD --- the last week date
                //    ME --- the end of the month
                //    CD --- the closest week date

                dtDate = DeemedDate(dtStartDate, obj);
                if ( Math.Abs((dtDate - dtStartDate).TotalDays) > 4 )
                {
                    dtDate = dtDate.AddDays (+ 7);
                }
            }
            else if ( ePDCounting == ECALENDARCYCLE_PDCOUNTING.PDCOUNT_BACKWARD )
            {
                // For this case, the dtEndDate is calculated according to the calendar cycle
                // parameters and could be either LD or CD.  To see if this fiscal year has a 6
                // week period, look backward from the dtEndDate - 366 - 3, one may have the previous
                // end of month
                // The EndOfMonth will give the month end date
                // The DeemedDate will give this fiscal year start date
                // then use this info to see if there is a 6 week period.
                //      |< 6 >|<--------------------------- 366 ------------------------------>| 3|
                //   ---+-----+--+--------------------------------------------------------+----+--+--->
                //     LD    ME  CD                                                      LD   ME  CD
                //     LD --- the last week date
                //     ME --- the end of the month
                //     CD --- the closest week date

                dtDate = DateTimeHelper.GetEndOfMonth (dtEndDate.AddDays (- Constants.MAXDAYCHANGE));
                dtDate = DeemedDate(dtDate, obj);
                if ( (dtEndDate - dtDate).TotalDays >= Constants.FIFTYTHREEWEEKYEAR )
                {
                    bHas6WeeksPeriod = true;
                }
                dtDate = DeemedDate(dtEndDate, obj);
                iTmpPeriodNum = (short)(periodCount - iPeriodNum + 1);
            }
            else
            {
                // For this case, we know it is a short year, the end date is calculated by
                // the calendar cycle parameters.  The start date is entered by the user and
                // can be located in either of the following locations (LD, ME, CD)
                //     |< 6 >|<----------------------- 365 ------------------------->| 3|
                //  ---+-----+--+----------------------------------------------+-----+--+---->
                //    LD    ME  CD                                            LD    ME CD
                //    LD --- the last week date
                //    ME --- the end of the month
                //    CD --- the closest week date
                // From the start date + 365 - 3, one may have the previous end of month
                // the EndOfMonth will give the month end date
                // the DeemedDate will give the date calculated by using the calendar
                // cycle parameters
                // dtDate is used as a base date to layout the periods
                // the dtStartDate - 3 will give a date between LD and CD
                // the DeemedDate(dtStartDate - 3) will give either LD or CD

                dtDate = DateTimeHelper.GetEndOfMonth(dtStartDate.AddDays(+ Constants.MINDAYCHANGE));
                dtDate = DeemedDate(dtDate, obj);

                if ( (dtDate - DeemedDate(dtStartDate.AddDays (- Constants.THREEDAYS), obj)).TotalDays >= Constants.FIFTYTHREEWEEKYEAR )
                {
                    bHas6WeeksPeriod = true;
                }
                if ( eCycleType != ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_THIRTEENPERIOD )
                {
                    iTmpPeriodNum = (short)(12 - iPeriodNum + 1);
                }
                else
                {
                    iTmpPeriodNum = (short)(13 - iPeriodNum + 1);
                }
            }

            if ( ePDCounting == ECALENDARCYCLE_PDCOUNTING.PDCOUNT_FORWARD )
            {
                // Calculate the number of weeks from the base date
                if ( eCycleType == ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FOURFOURFIVE )
                {
                    iBegNumWeeks = PeriodNumToWeekNum ((short)(iPeriodNum - 1), ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FIVEFOURFOUR);
                    iEndNumWeeks = PeriodNumToWeekNum(iPeriodNum, ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FIVEFOURFOUR);
                }
                else if ( eCycleType == ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FIVEFOURFOUR )
                {
                    iBegNumWeeks = PeriodNumToWeekNum ((short)(iPeriodNum - 1), ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FOURFOURFIVE);
                    iEndNumWeeks = PeriodNumToWeekNum(iPeriodNum, ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FOURFOURFIVE);
                }
                else
                {
                    iBegNumWeeks = PeriodNumToWeekNum ((short)(iPeriodNum - 1), eCycleType);
                    iEndNumWeeks = PeriodNumToWeekNum(iPeriodNum, eCycleType);
                }
                // Calculate the period start and end dates
                info.dtStartDate = dtDate.AddDays (+ (iBegNumWeeks * 7 + 1));
                info.dtEndDate = dtDate.AddDays (+ (iEndNumWeeks * 7));
            }
            else if ( ePDCounting == ECALENDARCYCLE_PDCOUNTING.PDCOUNT_BACKWARD )
            {
                // Calculate the number of weeks from the base date
                iBegNumWeeks = PeriodNumToWeekNum(iTmpPeriodNum, eCycleType);
                iEndNumWeeks = PeriodNumToWeekNum((short)(iTmpPeriodNum - 1), eCycleType);

                if ( bHas6WeeksPeriod )
                {
                    // Because the formula is based on the regular 445/454/544 pattern
                    // if it has a 6 week period then
                    iBegNumWeeks++;
                    iEndNumWeeks++;
                }
                // Calculate the period start and end dates
                info.dtStartDate = dtDate.AddDays (- (iBegNumWeeks * 7 - 1));
                info.dtEndDate = dtDate.AddDays (- iEndNumWeeks * 7);
            }
            else
            {
                // Calculate the number of weeks from the base date
                iBegNumWeeks = PeriodNumToWeekNum(iTmpPeriodNum, eCycleType);
                iEndNumWeeks = PeriodNumToWeekNum((short)(iTmpPeriodNum -1), eCycleType);

                if ( bHas6WeeksPeriod )
                {
                    // Because the formula is based on the regular 445/454/544 pattern
                    // it has a 6 week period then
                    iBegNumWeeks++;
                    iEndNumWeeks++;
                }
                // Calculate the period start and end dates
                info.dtStartDate = dtDate.AddDays (- (iBegNumWeeks * 7 - 1));
                info.dtEndDate = dtDate.AddDays (- iEndNumWeeks * 7);
            }
            // Set other period information
            info.iPeriodNumber = iPeriodNum;
            info.IsIdle = false;

            // Some fiscal years may have a few more days at the fiscal year start
            if ( info.dtStartDate < dtStartDate || iPeriodNum == 1 )
            {
		        periodBias = (short)((dtStartDate - info.dtStartDate).TotalDays / 7);
                info.dtStartDate = dtStartDate;
            }

            // Some fiscal years may have a few more days at the fiscal year end
            if ( iPeriodNum == periodCount || info.dtEndDate > dtEndDate )
            {
                info.dtEndDate = dtEndDate;
            }

            info.iWeight = (short)(((info.dtEndDate - info.dtStartDate).TotalDays + 4) / 7);
            if ( !bShortYear )
            {
                if ( info.iWeight < 4 )
                {
                    info.iWeight = (short)(4 - periodBias);
                }
            }
            if ( info.iWeight > 5 )
            {
                info.iWeight = 5;
            }
            else if (info.iWeight == 5 && eCycleType == ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_THIRTEENPERIOD)
            {
                info.iWeight = 4;
            }
            else //if (info.iWeight == 5) // now check for 53 week falling on a 4 period
            {
                switch (ePDCounting)
                {
                case ECALENDARCYCLE_PDCOUNTING.PDCOUNT_FORWARD:
                    switch (eCycleType)
                    {
                    case ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FOURFOURFIVE:    // This is really 544
                        if ( iPeriodNum % 3 != 1  && (info.iWeight > 4 || !bShortYear) )
                            info.iWeight = 4;
                        else if ( iPeriodNum % 3 == 1  && (info.iWeight > 5 || !bShortYear) )
                            info.iWeight = 5;
                        break;
                    case ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FOURFIVEFOUR:
                        if ( iPeriodNum % 3 != 2  && (info.iWeight > 4 || !bShortYear) )
                            info.iWeight = 4;
                        else if ( iPeriodNum % 3 == 2  && (info.iWeight > 5 || !bShortYear) )
                            info.iWeight = 5;
                        break;
                    case ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FIVEFOURFOUR:    // This is really 445
                        if ( iPeriodNum % 3 != 0  && (info.iWeight > 4 || !bShortYear) )
                            info.iWeight = 4;
                        else if ( iPeriodNum % 3 == 0  && (info.iWeight > 5 || !bShortYear) )
                            info.iWeight = 5;
                        break;
                    }
                    break;
                case ECALENDARCYCLE_PDCOUNTING.PDCOUNT_BACKWARD:
                    switch (eCycleType)
                    {
                    case ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FOURFOURFIVE:    // This is really 544
                        if ( (periodCount - iPeriodNum) % 3 != 2  && (info.iWeight > 4 || !bShortYear) )
                            info.iWeight = 4;
                        else if ( (periodCount - iPeriodNum) % 3 == 2  && (info.iWeight > 5 || !bShortYear) )
                            info.iWeight = 5;
                        break;
                    case ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FOURFIVEFOUR:
                        if ( (periodCount - iPeriodNum) % 3 != 1  && (info.iWeight > 4 || !bShortYear) )
                            info.iWeight = 4;
                        else if ( (periodCount - iPeriodNum) % 3 == 1  && (info.iWeight > 5 || !bShortYear) )
                            info.iWeight = 5;
                        break;
                    case ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FIVEFOURFOUR:    // This is really 445
                        if ( (periodCount - iPeriodNum) % 3 != 0  && (info.iWeight > 4 || !bShortYear) )
                            info.iWeight = 4;
                        else if ( (periodCount - iPeriodNum) % 3 == 0  && (info.iWeight > 5 || !bShortYear) )
                            info.iWeight = 5;
                        break;
                    }
                    break;
                case ECALENDARCYCLE_PDCOUNTING.PDCOUNT_BACKWARD_OLDMONTH:
                    switch (eCycleType)
                    {
                    case ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FOURFOURFIVE:    // This is really 544
                        if ( (13 - iTmpPeriodNum) % 3 != 1  && (info.iWeight > 4 || !bShortYear) )
                            info.iWeight = 4;
                        else if ( (13 - iTmpPeriodNum) % 3 == 1  && (info.iWeight > 5 || !bShortYear) )
                            info.iWeight = 5;
                        break;
                    case ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FOURFIVEFOUR:
                        if ( (13 - iTmpPeriodNum) % 3 != 2  && (info.iWeight > 4 || !bShortYear) )
                            info.iWeight = 4;
                        else if ( (13 - iTmpPeriodNum) % 3 == 2  && (info.iWeight > 5 || !bShortYear) )
                            info.iWeight = 5;
                        break;
                    case ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FIVEFOURFOUR:    // This is really 445
                        if ( (13 - iTmpPeriodNum) % 3 != 0  && (info.iWeight > 4 || !bShortYear) )
                            info.iWeight = 4;
                        else if ( (13 - iTmpPeriodNum) % 3 == 0  && (info.iWeight > 5 || !bShortYear) )
                            info.iWeight = 5;
                        break;
                    }
                    break;
                }
            }

            if ( info.dtEndDate > dtEndDate )
            {
                return false;
            }
            return true;
        }

        public override List<_PeriodInfo> PeriodInfo(BAFASFiscalYear obj)
        {
            short i;
            short periodCount = PeriodCount (obj);
            List<_PeriodInfo> info;
            DateTime dtStartDate;
            DateTime dtEndDate;
            ECALENDARCYCLE_PDCOUNTING ePDCounting;
            ECALENDARCYCLE_CYCLETYPE eCycleType;
            bool bShortYear;
            int totalWeights;

             info = new List<_PeriodInfo>(periodCount);

            dtStartDate = obj.YRStartDate;
            dtEndDate   = obj.YREndDate;
            ePDCounting = obj.PDCounting;
            eCycleType  = obj.CycleType;

            bShortYear = ((dtEndDate - dtStartDate).TotalDays < 355/*MINDAYCHANGE*/);

            // I don't know why this is needed (RDBJ)
            if ( eCycleType == ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FOURFOURFIVE )
            {
                eCycleType = ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FIVEFOURFOUR;
            }
            else if ( eCycleType == ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FIVEFOURFOUR )
            {
                eCycleType = ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FOURFOURFIVE;
            }

            for ( i = 0; i < periodCount; i++ )
            {
                _PeriodInfo prdInfo;
                CalcPeriod(out prdInfo, dtStartDate, dtEndDate, ePDCounting, eCycleType, (short)(i + 1), obj, periodCount, bShortYear);
                info.Add(prdInfo);
            }
            totalWeights = 0;

            for ( i = 0; i < periodCount; i++ )
            {
                totalWeights += info[i].iWeight;
            }

            for ( i = 0; i < periodCount; i++ )
            {
                if ( i > 0 )
                {
                    info[i].iFutureWeight = (short)(info[i-1].iFutureWeight + info[i-1].iWeight);
                }
                totalWeights -= info[i].iWeight;
                info[i].iRemainWeight = (short)(totalWeights);
            }

            return info;
        }

        public override DateTime DeemedDate(DateTime dtDate, BAFASFiscalYear obj)
        {
            short iDayOfWeek;
            short iDayDiff;
            DateTime deemed;
            ECALENDARCYCLE_YEARENDELECTION eYearEndElect;
            ECALENDARCYCLE_DATEOFWEEK eDayWeekEnum;

            eYearEndElect = obj.YearEndElect;
            eDayWeekEnum  = obj.DateOfWeek;

            // Make the function default to YEARENDELECTION_LASTDAY
            deemed = dtDate;

            if ( eYearEndElect == ECALENDARCYCLE_YEARENDELECTION.YEARENDELECTION_CLOSESTWEEKDAY )
            {
                deemed = obj.GetNearDayOfWeek(dtDate);
            }
            else if ( eYearEndElect == ECALENDARCYCLE_YEARENDELECTION.YEARENDELECTION_LASTWEEKDAY )
            {
                iDayOfWeek = (short)dtDate.DayOfWeek;
                iDayDiff = (short)(iDayOfWeek - eDayWeekEnum - 1);
                if ( iDayDiff < 0 )
                {
                    // for this case it means mePeriodDayOfWeek is ahead of iDayOfWeek
                    // we want to move back 7 + iDayDiff(iDayDiff is negative) days.
                    iDayDiff = (short)(7 + iDayDiff);
                }
                deemed = dtDate.AddDays (- iDayDiff);
            }
            return deemed;
        }

        short WeekNumToPeriodNum (short iWeekNum, ECALENDARCYCLE_CYCLETYPE aCycleType)
        {
            if ( aCycleType == ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_THIRTEENPERIOD )
            {
                return (short)(iWeekNum / 4 + 1);
            }
            else if ( aCycleType == ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FOURFOURFIVE ||
                        aCycleType == ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FIVEFOURFOUR ||
                        aCycleType == ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FOURFIVEFOUR )
            {
                int adjFactor = 0;
                switch (aCycleType)
                {
                case ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FOURFOURFIVE:
                    adjFactor = 0;
                    break;
                case ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FOURFIVEFOUR:
                    adjFactor = 1;
                    break;
                case ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FIVEFOURFOUR:
                    adjFactor = 2;
                    break;
                }
                return (short)((iWeekNum - (int)((iWeekNum + 1 + 4 * (adjFactor)) / 13)) / 4 + 1);
            }
            return 0;
        }

        short PeriodNumToWeekNum (short iPeriodNum, ECALENDARCYCLE_CYCLETYPE aCycleType)
        {
            if ( aCycleType == ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_THIRTEENPERIOD )
            {
                return (short)(iPeriodNum * 4);
            }
            else if ( aCycleType == ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FOURFOURFIVE ||
                      aCycleType == ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FOURFIVEFOUR ||
                      aCycleType == ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FIVEFOURFOUR )
            {
                int adjFactor = 0;
                switch (aCycleType)
                {
                case ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FOURFOURFIVE:
                    adjFactor = 0;
                    break;
                case ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FOURFIVEFOUR:
                    adjFactor = 1;
                    break;
                case ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FIVEFOURFOUR:
                    adjFactor = 2;
                    break;
                }
                return (short)((iPeriodNum - 1) * 4 + (int)((iPeriodNum + adjFactor) / 3) + 4);
            }
            return 0;
        }

       public override DateTime GetMidPeriodDate(DateTime dtDate, short periodCount, List<_PeriodInfo> info, BAFASFiscalYear obj)
       {
            short i;

            for ( i = 0; i < periodCount; i++ )
            {
                if ( dtDate >= info[i].dtStartDate && dtDate <= info[i].dtEndDate )
                {
                    double days = ((info[i].dtEndDate - info[i].dtStartDate).TotalDays / 2) + 1;
                    DateTime dt = info[i].dtStartDate.AddDays (days);
                    return dt;
                }
            }

            return DateTime.MinValue;
        }

        public override DateTime FiscalYearEndDate(int iYear, int m_iFYEndMonthEnum, BAFASFiscalYear obj)
        {
            return DeemedDate(new DateTime(iYear, m_iFYEndMonthEnum, DateTime.DaysInMonth(iYear, m_iFYEndMonthEnum)), obj);
        }


        public override DateTime GetMidYearDate(short periodCount, List<_PeriodInfo> info, BAFASFiscalYear obj)
        {
            DateTime StartDate;
            DateTime EndDate;

            StartDate = obj.YRStartDate;
            EndDate   = obj.YREndDate;

            double days = ((EndDate - StartDate).TotalDays / 2) + 1;
            DateTime dt = StartDate.AddDays (days);
            return dt;
        }

        public override bool IsShortYear(BAFASFiscalYear obj)
        {
            DateTime StartDate;
            DateTime EndDate;
            short iNumWeeks;
            short iFYEndMonth;
            short iFYNum;
            ECALENDARCYCLE_CYCLETYPE eCycleType;

            StartDate   = obj.YRStartDate;
            EndDate     = obj.YREndDate;
            iFYEndMonth = obj.FYEndMonth;
            iFYNum      = obj.FYNum;
            eCycleType  = obj.CycleType;
            {
                return false;
            }

            if (iFYNum == 1)
            {
                iNumWeeks = (short)(((EndDate - StartDate).TotalDays + 3) / 7);
                switch (eCycleType)
                {
                    case ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_THIRTEENPERIOD:
                        return (iNumWeeks <= 48) ? true : false;
                    case ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FOURFOURFIVE:
                    case ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FOURFIVEFOUR:
                        return (iNumWeeks <= 48) ? true : false;

                    case ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FIVEFOURFOUR:
                        return (iNumWeeks <= 47) ? true : false;
                }
                return false;
            }
            else if ((EndDate - StartDate).TotalDays < 355)
            {
                return true;
            }
            return false;
        }

    };

}
