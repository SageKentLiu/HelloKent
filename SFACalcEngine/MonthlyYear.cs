using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFACalendar;

namespace SFACalcEngine
{
    class MonthlyYear
    {
        private IBAFiscalYear m_pObjIFY;
        private DateTime m_dtDeemedFYStart;
        private DateTime m_dtDeemedFYEnd;

        const int MidMonthDay = 15;

        public MonthlyYear()
        {
            m_dtDeemedFYStart = DateTime.MinValue;
            m_dtDeemedFYEnd = DateTime.MinValue;
        }

        public IBAFiscalYear FiscalYearInfo
        {
            get
            {
                return m_pObjIFY;
            }

            set
            {
                m_pObjIFY = value;
            }
        }

        public void DeemedFYDates()
        {
            int iYear;
            int iMonth;
            int iSEMonth;
            DateTime dtDate;

            dtDate = m_pObjIFY.YRStartDate;

            iMonth = (dtDate.Month);
            iYear = (dtDate.Year);

            iSEMonth = StartMonth();
            if (iMonth != iSEMonth)
            {
                // 1st day of next month
                m_dtDeemedFYStart = new DateTime(iYear, iMonth, 1).AddMonths(1);
            }
            else
            {
                // 1st day of the month
                m_dtDeemedFYStart = new DateTime(iYear, iMonth, 1);
            }

            dtDate = m_pObjIFY.YREndDate;

            iMonth = (dtDate).Month;
            iYear = (dtDate.Year);

            iSEMonth = EndMonth();
            if (iMonth != iSEMonth)
            {
                //last day of prev month
                m_dtDeemedFYEnd = (new DateTime(iYear, iMonth, 1)).AddDays(-1);
            }
            else
            {
                //last day of the month
                m_dtDeemedFYEnd = (new DateTime(iYear, iMonth, 1).AddMonths(1)).AddDays(-1);
            }
        }

        //  +------------------------------+------+--+---------------.
        // 1st                             A     1st B
        // for monthly a fiscal year always start at first date of the
        // month.
        // for AAP a fiscal year may start at some where between A and
        // B (the 9 days rule)
        int StartMonth()
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
            dtDate = m_pObjIFY.YRStartDate;

            iYear = (dtDate.Year);
            iMonth = (dtDate).Month;
            dtFirstDate = new DateTime(iYear, iMonth, 1);

            // if first year is a short year (due to business start date)
            iFYNum = m_pObjIFY.FYNum;
            bRet = m_pObjIFY.IsShortYear;
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
                    iEMonth = m_pObjIFY.FYEndMonth;
                    iStartMonth = (int)iEMonth + 1;
                }
                else
                {
                    // if any short year other than first year (due to fiscal end month change)
                    if ((dtDate - dtFirstDate).TotalDays > MidMonthDay)
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

        int EndMonth()
        {
            short iEndMonth;

            iEndMonth = m_pObjIFY.FYEndMonth;
            return (int)iEndMonth;
        }

        int YearNumber()
        {
            int iYear;
            int iMonth;
            DateTime dtDate;

            dtDate = m_pObjIFY.YREndDate;

            iYear = (dtDate.Year);
            iMonth = (dtDate.Month);

            //if FY end date month not equal to FY end month
            if (iMonth != EndMonth())
            {
                iMonth = iMonth - 1;
                if (iMonth < 1)
                {
                    iYear = iYear - 1;
                }
            }
            return iYear;
        }


        public double GetFirstYearFactor(DateTime dtDate)
        {
            int iMonths;

            if (dtDate <= DateTime.MinValue)
            {
                // for Half Year convention
                return 0.5;
            }

            iMonths = (m_dtDeemedFYEnd).Month - (dtDate).Month + 1;
            if (iMonths < 1)
            {
                iMonths = iMonths + 12;
            }

            if ((dtDate.Day) == MidMonthDay)
            {
                return ((double)(iMonths) - 0.5) / 12 /*GetFYNumMonths()*/;
            }
            else
            {
                return (double)(iMonths) / 12 /*GetFYNumMonths()*/;
            }
        }

        public double GetLastYearFactor(DateTime dtDate)
        {
            int iMonths;

            if (dtDate <= DateTime.MinValue)
            {
                // for Half Year convention
                return 0.5;
            }

            iMonths = (dtDate).Month - (m_dtDeemedFYStart).Month;
            if (iMonths < 1 && (dtDate.Year) > (m_dtDeemedFYStart.Year))
            {
                iMonths = iMonths + 12;
            }

            if ((dtDate.Day) == MidMonthDay)
            {
                return ((double)(iMonths) + 0.5) / 12 /*GetFYNumMonths()*/;
            }
            else
            {
                return (double)(iMonths) / 12 /*GetFYNumMonths()*/;
            }
        }
        public DateTime GetMidQuarterDate(DateTime dtDate)
        {
            int iFYNumDays;
            int iQtrNumDays;
            int iNumDays;
            int iQtrMonth;
            int iQtrDay;
            DateTime deemedDate;
            DateTime dtMidQtr;
            DateTime dtSDate;
            DateTime dtEDate;
            DateTime dtMidQuarterDate;
            bool bRet;

            deemedDate = dtDate;
            if (deemedDate < m_dtDeemedFYStart)
            {
                deemedDate = m_dtDeemedFYStart;
            }
            else if (deemedDate > m_dtDeemedFYEnd)
            {
                deemedDate = m_dtDeemedFYEnd;
            }

            if (IsMonthlyQuarter())
            {
                //get the month number base 1
                iQtrDay = MidMonthDay;
                iQtrMonth = (deemedDate).Month - (m_dtDeemedFYStart).Month + 1;
                if (iQtrMonth < 1)
                {
                    iQtrMonth += 12;
                }

                if (GetFYNumMonths() == 8)
                {
                    iQtrMonth = (int)((iQtrMonth - 1) / 2) * 2 + 2;
                    iQtrDay = 1;
                }
                //if regular year find the right mid quarter month
                bRet = m_pObjIFY.IsShortYear;
                if (!bRet)
                {
                    iQtrMonth = (int)((iQtrMonth - 1) / 3) * 3 + 2;
                }
                int month = (m_dtDeemedFYStart.Month) + iQtrMonth - 1;
                int year = (m_dtDeemedFYStart.Year);
                while (month > 12)
                {
                    month -= 12;
                    year++;
                }
                while (month < 1)
                {
                    month += 12;
                    year--;
                }
                dtMidQuarterDate = new DateTime(year, month, iQtrDay);
                return dtMidQuarterDate;
            }

            //continue here :SSS
            dtEDate = m_pObjIFY.YREndDate;
            dtSDate = m_pObjIFY.YRStartDate;
            iFYNumDays = (int)(dtEDate - dtSDate).TotalDays + 1;

            int iQtrNumDays1 = 0;
            int iQtrNumDays2 = 0;
            int iQtrNumDays3 = 0;
            int iRDays = 0;
            int iDayOffSet = 0;

            iQtrNumDays = iFYNumDays / 4;
            iRDays = iFYNumDays % 4;

            if (iRDays == 0)
            {
                iQtrNumDays1 = iQtrNumDays;
                iQtrNumDays2 = iQtrNumDays1 + iQtrNumDays;
                iQtrNumDays3 = iQtrNumDays2 + iQtrNumDays;
            }
            else if (iRDays == 1)
            {
                // if one day more, give to the 3rd quater
                iQtrNumDays1 = iQtrNumDays;
                iQtrNumDays2 = iQtrNumDays1 + iQtrNumDays;
                iQtrNumDays3 = iQtrNumDays2 + iQtrNumDays + 1;
            }
            else if (iRDays == 2)
            {
                // if two days more, give to the 2nd and 4th quaters
                iQtrNumDays1 = iQtrNumDays;
                iQtrNumDays2 = iQtrNumDays1 + iQtrNumDays + 1;
                iQtrNumDays3 = iQtrNumDays2 + iQtrNumDays;
            }
            else if (iRDays == 3)
            {
                // if three days more, give to the 1st, 3rd and 4th quaters
                iQtrNumDays1 = iQtrNumDays + 1;
                iQtrNumDays2 = iQtrNumDays1 + iQtrNumDays;
                iQtrNumDays3 = iQtrNumDays2 + iQtrNumDays + 1;
            }

            iNumDays = (int)(dtDate - dtSDate).TotalDays + 1;
            if (0 < iNumDays && iNumDays <= iQtrNumDays1)
            {
                iDayOffSet = 0;
                iQtrNumDays = iQtrNumDays1;
            }
            else if (iQtrNumDays1 < iNumDays && iNumDays <= iQtrNumDays2)
            {
                iDayOffSet = iQtrNumDays1;
                iQtrNumDays = iQtrNumDays2 - iQtrNumDays1;
            }
            else if (iQtrNumDays2 < iNumDays && iNumDays <= iQtrNumDays3)
            {
                iDayOffSet = iQtrNumDays2;
                iQtrNumDays = iQtrNumDays3 - iQtrNumDays2;
            }
            else
            {
                iDayOffSet = iQtrNumDays3;
            }
            // truncation for iQtrNumDays / 2 in the following c++ statement
            // take first quarter as an example and randomly pickup dtSDate = 11/5/yy
            // if iQtrNumDays is odd number, for example, iQtrNumDays = 5 which means 5 days in each 
            // quarter, then the dates in the first quarter are 11/5/yy, 11/6/yy, 11/7/yy, 11/8/yy, 
            // 11/9/yy. the mid-quarter date is 11/7/yy. in the following statement 
            // iQtrNumDays / 2 = 5 / 2 = 2 and Day(dtSDate) = Day(11/5/yy) = 5
            // the Day(dtSDate) + iQtrNumDays / 2 = 5 + 2 = 7 
            // will give 11/7/yy as the mid-quarter date
            // if iQtrNumDays is even number, for example, iQtrNumDays = 6 which means 6 days in each 
            // quarter, then the dates in the first quarter are 11/5/yy, 11/6/yy, 11/7/yy, 11/8/yy, 
            // 11/9/yy. 11/10/yy. what is the mid-quarter date? 11/7/yy or 11/8/yy? 
            // in this case 
            // iQtrNumDays / 2 = 6 / 2 = 3 and Day(dtSDate) = Day(11/5/yy) = 5
            // the Day(dtSDate) + iQtrNumDays / 2 = 5 + 3 = 8 
            // will pickup 11/8/yy as the mid-quarter date
            // ---
            // if want to use 11/7/yy as the mid-quarter date then 
            // Day(dtSDate) + iQtrNumDays / 2 + iQtrNumDays % 2 - 1, 
            // may work for both iQtrNumDays is odd or even numbers
            dtMidQtr = new DateTime((dtSDate.Year), (dtSDate).Month, (dtSDate.Day) + iDayOffSet + iQtrNumDays / 2);
            if ((dtMidQtr.Day) < 15)
            {
                dtMidQuarterDate = new DateTime(dtMidQtr.Year, dtMidQtr.Month, 1);
            }
            else
            {
                dtMidQuarterDate = new DateTime((dtMidQtr.Year), (dtMidQtr.Month), 15);
            }
            return dtMidQuarterDate;
        }

        public short GetMidQuarterNumber(DateTime dtDate)
        {
            int iFYNumDays;
            int iQtrNumDays;
            int iQtrNo;
            int iNumDays;
            int iQtrMonth;
            int iQtrDay;
            DateTime deemedDate;
            DateTime dtSDate;
            DateTime dtEDate;
            DateTime dtMidQuarterDate;
            bool bRet;

            deemedDate = dtDate;
            if (deemedDate < m_dtDeemedFYStart)
            {
                deemedDate = m_dtDeemedFYStart;
            }
            else if (deemedDate > m_dtDeemedFYEnd)
            {
                deemedDate = m_dtDeemedFYEnd;
            }

            if (IsMonthlyQuarter())
            {
                short yeardays = (short)(m_dtDeemedFYEnd - m_dtDeemedFYStart).TotalDays;
                short middays;

                //get the month number base 1
                iQtrDay = MidMonthDay;
                iQtrMonth = (deemedDate.Month) - (m_dtDeemedFYStart.Month) + 1;
                if (iQtrMonth < 1)
                {
                    iQtrMonth += 12;
                }

                if (GetFYNumMonths() == 8)
                {
                    iQtrMonth = (int)((iQtrMonth - 1) / 2) * 2 + 2;
                    iQtrDay = 1;
                }
                //if regular year find the right mid quarter month
                bRet = m_pObjIFY.IsShortYear;
                if (!bRet)
                {
                    iQtrMonth = (int)((iQtrMonth - 1) / 3) * 3 + 2;
                }
                dtMidQuarterDate = new DateTime((m_dtDeemedFYStart.Year), (m_dtDeemedFYStart.Month) + iQtrMonth - 1,
                                    iQtrDay);
                middays = (short)(dtMidQuarterDate - m_dtDeemedFYStart).TotalDays;
                if (middays < yeardays / 4)
                    return 1;
                else if (middays < yeardays / 2)
                    return 2;
                else if (middays < yeardays * 3 / 4)
                    return 3;
                return 4;
            }

            //continue here :SSS
            dtEDate = m_pObjIFY.YREndDate;
            dtSDate = m_pObjIFY.YRStartDate;
            iFYNumDays = (int)(dtEDate - dtSDate).TotalDays + 1;

            int iQtrNumDays1 = 0;
            int iQtrNumDays2 = 0;
            int iQtrNumDays3 = 0;
            int iRDays = 0;

            iQtrNumDays = iFYNumDays / 4;
            iRDays = iFYNumDays % 4;

            if (iRDays == 0)
            {
                iQtrNumDays1 = iQtrNumDays;
                iQtrNumDays2 = iQtrNumDays1 + iQtrNumDays;
                iQtrNumDays3 = iQtrNumDays2 + iQtrNumDays;
            }
            else if (iRDays == 1)
            {
                // if one day more, give to the 3rd quater
                iQtrNumDays1 = iQtrNumDays;
                iQtrNumDays2 = iQtrNumDays1 + iQtrNumDays;
                iQtrNumDays3 = iQtrNumDays2 + iQtrNumDays + 1;
            }
            else if (iRDays == 2)
            {
                // if two days more, give to the 2nd and 4th quaters
                iQtrNumDays1 = iQtrNumDays;
                iQtrNumDays2 = iQtrNumDays1 + iQtrNumDays + 1;
                iQtrNumDays3 = iQtrNumDays2 + iQtrNumDays;
            }
            else if (iRDays == 3)
            {
                // if three days more, give to the 1st, 3rd and 4th quaters
                iQtrNumDays1 = iQtrNumDays + 1;
                iQtrNumDays2 = iQtrNumDays1 + iQtrNumDays;
                iQtrNumDays3 = iQtrNumDays2 + iQtrNumDays + 1;
            }

            iNumDays = (int)(dtDate - dtSDate).TotalDays + 1;
            if (0 < iNumDays && iNumDays <= iQtrNumDays1)
            {
                iQtrNo = 1;
            }
            else if (iQtrNumDays1 < iNumDays && iNumDays <= iQtrNumDays2)
            {
                iQtrNo = 2;
            }
            else if (iQtrNumDays2 < iNumDays && iNumDays <= iQtrNumDays3)
            {
                iQtrNo = 3;
            }
            else
            {
                iQtrNo = 4;
            }
            return (short)iQtrNo;
        }

        bool IsMonthlyQuarter()
        {
            //if regular year or
            // short year with 4 months starting and ending on month boundaries
            int iNumMonths;
            DateTime dtSDate;
            DateTime dtEDate;
            bool bRet;

            iNumMonths = GetFYNumMonths();

            bRet = m_pObjIFY.IsShortYear;
            dtSDate = m_pObjIFY.YRStartDate;
            dtEDate = m_pObjIFY.YREndDate;
            if (!bRet || ((iNumMonths == 4 || iNumMonths == 8) &&
                m_dtDeemedFYStart == dtSDate && m_dtDeemedFYEnd == dtEDate))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        int GetFYNumMonths()
        {
            int iFYNumMonths;
            bool bRet;

            bRet = m_pObjIFY.IsShortYear;
            if (!bRet)
            {
                iFYNumMonths = 12;
            }
            else
            {
                // if short year find number of months in FY
                iFYNumMonths = (m_dtDeemedFYEnd.Month) - (m_dtDeemedFYStart.Month) + 1;
                if (iFYNumMonths < 1)
                {
                    iFYNumMonths = iFYNumMonths + 12;
                }
            }
            return iFYNumMonths;
        }

        public DateTime GetMidMonthDate(DateTime dtDate)
        {
            DateTime dtMidMonthDate;

            if (dtDate < m_dtDeemedFYStart)
            {
                dtMidMonthDate = new DateTime((m_dtDeemedFYStart.Year), (m_dtDeemedFYStart.Month),
                                    MidMonthDay);
                return dtMidMonthDate;
            }

            if (dtDate > m_dtDeemedFYEnd)
            {
                dtMidMonthDate = new DateTime((m_dtDeemedFYEnd.Year), (m_dtDeemedFYEnd.Month),
                                    MidMonthDay);
                return dtMidMonthDate;
            }

            dtMidMonthDate = new DateTime((dtDate.Year), (dtDate.Month), MidMonthDay);
            return dtMidMonthDate;
        }

        public DateTime GetFullMonthDate(DateTime dtDate)
        {
            DateTime dtFullMonthDate;

            if (dtDate < m_dtDeemedFYStart)
            {
                dtFullMonthDate = m_dtDeemedFYStart;
                return dtFullMonthDate;
            }

            if (dtDate > m_dtDeemedFYEnd)
            {
                dtFullMonthDate = new DateTime((m_dtDeemedFYEnd.Year), (m_dtDeemedFYEnd.Month),
                                1);
                return dtFullMonthDate;
            }

            dtFullMonthDate = new DateTime((dtDate.Year), (dtDate.Month), 1);
            return dtFullMonthDate;
        }

        DateTime GetTruedEndDate(DateTime dt)
        {
            int month, day, year;

            month = dt.Month;
            day = dt.Day;
            year = dt.Year;

            if (day <= 15)
            {
                return new DateTime(year, month, 1).AddDays(-1);
            }
            else
            {
                month++;
                if (month > 12)
                {
                    month = (short)(month - 12);
                    year++;
                }
                return new DateTime(year, month, 1).AddDays(-1);
            }
        }

        DateTime GetTruedStartDate(DateTime dt)
        {
            short iFYNum;

            int month, day, year;

            month = dt.Month;
            day = dt.Day;
            year = dt.Year;

            if (m_pObjIFY == null)
                iFYNum = 2; // make it other than 1
            else
                iFYNum = m_pObjIFY.FYNum;
            if (iFYNum == 1 && (m_dtDeemedFYStart.Month) == month)
            {
                // The month of the start of business is handled differently.
                return new DateTime(year, month, 1);
            }
            else
            {
                if (day <= 15)
                    return new DateTime(year, month, 1);
                else
                {
                    month++;
                    if (month > 12)
                    {
                        month = (short)(month - 12);
                        year--;
                    }
                    return new DateTime(year, month, 1);
                }
            }
        }

        public DateTime GetMidYearDate()
        {
            int iMonths;
            int startMonth, startDay, startYear;

            DateTime dt = GetTruedStartDate(m_dtDeemedFYStart);
            startMonth = dt.Month;
            startDay = dt.Day;
            startYear = dt.Year;


            iMonths = (GetTruedEndDate(m_dtDeemedFYEnd)).Month - startMonth + 1;
            if (iMonths < 1)
            {
                iMonths = iMonths + 12;
            }
            if (iMonths % 2 == 1)
            {
                // we need a mid month date
                startDay = 15;
            }
            else
            {
                // we need a first day date.
                startDay = 1;
            }
            iMonths = (short)((short)(iMonths >> 1) + startMonth);
            if (iMonths > 12)
            {
                startYear++;
                iMonths = (short)(iMonths - 12);
            }
            return new DateTime(startYear, iMonths, startDay);
        }


    }

}
