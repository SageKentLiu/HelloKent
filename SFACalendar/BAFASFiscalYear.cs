using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFACalendar
{

    public class BAFASFiscalYear : IBAFiscalYear
    {

        DateTime m_dtStartDate;
        DateTime m_dtEndDate;
        short m_iFYNum;
        bool m_bShortYear;
        ECALENDARCYCLE_CYCLETYPE m_eCycleType;
        ECALENDARCYCLE_DATEOFWEEK m_eDayWeekEnum;
        ECALENDARCYCLE_PDCOUNTING m_ePDCounting;
        ECALENDARCYCLE_YEARENDELECTION m_eYearEndElect;
        short m_iFYEndMonthEnum;

        short m_maxWeights;
        _IFYHelper m_helper;
        List<_PeriodInfo> m_PeriodInfo;
        short m_PeriodCount;

        public BAFASFiscalYear()
        {

        }

        public DateTime YRStartDate
        {
            get
            {
                return m_dtStartDate;
            }
            set
            {
                m_dtStartDate = value;
            }
        }

        public DateTime YREndDate
        {
            get
            {
                return m_dtEndDate;
            }
            set
            {
                m_dtEndDate = value;
            }
        }

        public short FYNum
        {
            get
            {
                return m_iFYNum;
            }
            set
            {
                m_iFYNum = value;
            }
        }

        public bool IsShortYear
        {
            get
            {
                return m_bShortYear;
            }
            set
            {
                m_bShortYear = value;
            }
        }

        public short FYEndMonth
        {
            get
            {
                return m_iFYEndMonthEnum;
            }
            set
            {
                m_iFYEndMonthEnum = value;
            }
        }

        public ECALENDARCYCLE_CYCLETYPE CycleType
        {
            get { return m_eCycleType; }
            set
            {
                if (m_helper != null)
                {
                    m_helper = null;
                }
                if (m_PeriodInfo != null)
                {
                    m_PeriodInfo = null;
                }
                m_PeriodCount = 0;
                switch (value)
                {
                    case ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_MONTHLY:
                        m_helper = new CMonthlyHelper();
                        m_maxWeights = m_helper.MaxWeights();
                        break;
                    case ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FOURFOURFIVE:
                    case ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FOURFIVEFOUR:
                    case ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FIVEFOURFOUR:
                    case ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_THIRTEENPERIOD:
                        m_helper = new CAAPHelper();
                        m_maxWeights = m_helper.MaxWeights ();
                        break;
                    case ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_CUSTOM:
                    default:
                        m_helper = new CMonthlyHelper();
                        m_maxWeights = m_helper.MaxWeights();
                        return;
                }
                m_eCycleType = value;
            }
        }

        public ECALENDARCYCLE_DATEOFWEEK DateOfWeek
        {
            get { return m_eDayWeekEnum; }
            set
            {
                m_eDayWeekEnum = value;
            }
        }

        public ECALENDARCYCLE_PDCOUNTING PDCounting
        {
            get { return m_ePDCounting; }
            set
            {
                m_ePDCounting = value;
            }
        }

        public ECALENDARCYCLE_YEARENDELECTION YearEndElect
        {
            get { return m_eYearEndElect; }
            set
            {
                m_eYearEndElect = value;
            }
        }

        public bool GetRemainingPeriodWeights(DateTime dtDate, out short pVal)
        {
            short i;

            if (m_PeriodCount == 0)
            {
                m_PeriodCount = m_helper.PeriodCount(this);
                m_PeriodInfo = m_helper.PeriodInfo(this);
            }
            for (i = 0; i < m_PeriodCount; i++)
            {
                if (dtDate >= m_PeriodInfo[i].dtStartDate && dtDate <= m_PeriodInfo[i].dtEndDate)
                {
                    pVal = m_PeriodInfo[i].iRemainWeight;
                    return true;
                }
            }
            throw new Exception("Specified date is outside of the current fiscal year.");
        }

        public bool GetPreviousPeriodWeights(DateTime dtDate, out short pVal)
        {
            short i;

            if (m_PeriodCount == 0)
            {
                m_PeriodCount = m_helper.PeriodCount(this);
                m_PeriodInfo = m_helper.PeriodInfo(this);
            }
            for (i = 0; i < m_PeriodCount; i++)
            {
                if (dtDate <= m_PeriodInfo[i].dtEndDate && dtDate >= m_PeriodInfo[i].dtStartDate)
                {
                    pVal = m_PeriodInfo[i].iFutureWeight;
                    return true;
                }
            }
            throw new Exception("Specified date is outside of the current fiscal year.");
        }

        public bool GetCurrentPeriodWeight(DateTime dtDate, out short pVal)
        {
            short i;

            if (m_PeriodCount == 0)
            {
                m_PeriodCount = m_helper.PeriodCount(this);
                m_PeriodInfo = m_helper.PeriodInfo(this);
            }

            for (i = 0; i < m_PeriodCount; i++)
            {
                if (dtDate >= m_PeriodInfo[i].dtStartDate && dtDate <= m_PeriodInfo[i].dtEndDate)
                {
                    pVal = m_PeriodInfo[i].iWeight;
                    return true;
                }
            }

            throw new Exception("Specified date is outside of the current fiscal year.");
        }

        public bool GetPeriodWeights(DateTime dtStartDate, DateTime dtEndDate, out short pVal)
        {
            short iWeight;
            short iRemain;
            short iCurrent;
            short iEndRemain;
            bool hr;
            pVal = -1;
            if (!(hr = GetRemainingPeriodWeights(dtStartDate, out iRemain)) ||
                 !(hr = GetCurrentPeriodWeight(dtStartDate, out iCurrent)) ||
                 !(hr = GetRemainingPeriodWeights(dtEndDate, out iEndRemain)))
            {
                return hr;
            }
            iWeight = (short)(iRemain + iCurrent - iEndRemain);
            if (iWeight > m_maxWeights)
            {
                iWeight = m_maxWeights;
            }
            pVal = iWeight;
            return true;
        }

        public bool GetTotalAnnualPeriodWeights(out short pVal)
        {
            pVal = m_maxWeights;
            return true;
        }

        public bool GetTotalFiscalYearPeriodWeights(out short pVal)
        {
            if (m_PeriodCount == 0)
            {
                m_PeriodCount = m_helper.PeriodCount(this);
                m_PeriodInfo = m_helper.PeriodInfo(this);
            }
            pVal = m_helper.TotalFYWeights(m_PeriodCount, m_PeriodInfo, m_dtStartDate, m_dtEndDate, this);
            return true;
        }

        public bool GetFiscalYearFraction(out double pVal)
        {
            short iWeights;
            bool hr;
            pVal = 0;

            if (!(hr = GetTotalFiscalYearPeriodWeights(out iWeights)))
                return hr;
            if (iWeights >= m_maxWeights)
            {
                pVal = 1;
            }
            else
                pVal = (double)(iWeights) / m_maxWeights;
            return true;
        }

        public bool GetNumPeriods(out short pVal)
        {
            if (m_PeriodCount == 0)
            {
                m_PeriodCount = m_helper.PeriodCount(this);
                m_PeriodInfo = m_helper.PeriodInfo(this);
            }
            pVal = m_PeriodCount;
            return true;
        }

        public bool GetPeriod(DateTime dtDate, out IBACalcPeriod objPeriod)
        {
            short i;

            (objPeriod) = null;

            if (m_PeriodCount == 0)
            {
                m_PeriodCount = m_helper.PeriodCount(this);
                m_PeriodInfo = m_helper.PeriodInfo(this);
            }

            for (i = 0; i < m_PeriodCount; i++)
            {
                if (dtDate >= m_PeriodInfo[i].dtStartDate && dtDate <= m_PeriodInfo[i].dtEndDate)
                {
                    BAFASCalcPeriod obj = new BAFASCalcPeriod();

                    obj.PeriodStart = (m_PeriodInfo[i].dtStartDate);
                    obj.PeriodEnd = (m_PeriodInfo[i].dtEndDate);
                    obj.PeriodNum = (m_PeriodInfo[i].iPeriodNumber);
                    obj.Weight = (m_PeriodInfo[i].iWeight);
                    obj.IsIdle = m_PeriodInfo[i].IsIdle;
                    objPeriod = obj;
                    return true;
                }
            }

            return false;  // changed for performance  RDBJ 5/17/00
        }

        public bool GetPeriodByNum(short iPeriodNum, out IBACalcPeriod objPeriod)
        {
            bool hr;

            objPeriod = null;
            if (m_PeriodCount == 0)
            {
                m_PeriodCount = m_helper.PeriodCount(this);
                m_PeriodInfo = m_helper.PeriodInfo(this);
            }
            if (iPeriodNum > m_PeriodCount || iPeriodNum < 1)
            {
                throw new Exception("The specified period number is invalid.");
            }

            BAFASCalcPeriod obj = new BAFASCalcPeriod();

            obj.PeriodStart = (m_PeriodInfo[iPeriodNum - 1].dtStartDate);
            obj.PeriodEnd = (m_PeriodInfo[iPeriodNum - 1].dtEndDate);
            obj.PeriodNum = (m_PeriodInfo[iPeriodNum - 1].iPeriodNumber);
            obj.Weight = (m_PeriodInfo[iPeriodNum - 1].iWeight);
            obj.IsIdle = m_PeriodInfo[iPeriodNum - 1].IsIdle;
            objPeriod = obj;


            return true;
        }

        public bool GetMidPeriodDate(DateTime dtDate, out DateTime pVal)
        {
            if (m_PeriodCount == 0)
            {
                m_PeriodCount = m_helper.PeriodCount(this);
                m_PeriodInfo = m_helper.PeriodInfo(this);
            }

            pVal = m_helper.GetMidPeriodDate(dtDate, m_PeriodCount, m_PeriodInfo, this);
            if (pVal == DateTime.MinValue)
            {
                throw new Exception("Specified date is outside of the current fiscal year.");
            }
            return true;
        }

        public bool FiscalYearEndDate(long iYear, out DateTime pVal)
        {
            pVal = m_helper.FiscalYearEndDate((int)iYear, m_iFYEndMonthEnum, this);
            return true;
        }

        public bool GetMidYearDate(out DateTime pVal)
        {
            if (m_PeriodCount == 0)
            {
                m_PeriodCount = m_helper.PeriodCount(this);
                m_PeriodInfo = m_helper.PeriodInfo(this);
            }
            pVal = m_helper.GetMidYearDate(m_PeriodCount, m_PeriodInfo, this);
            return true;
        }

        public bool InitializeWithCycleObject(IBACycleObject newVal, short iYearNumber)
        {
            bool hr;
            DateTime _dtStartDate;
            DateTime _dtEndDate;
            DateTime tmpDate;
            ECALENDARCYCLE_CYCLETYPE _eCycleType;
            ECALENDARCYCLE_DATEOFWEEK _eDayWeekEnum;
            ECALENDARCYCLE_PDCOUNTING _ePDCounting;
            ECALENDARCYCLE_YEARENDELECTION _eYearEndElect;
            short _iFYEndMonthEnum;
            long lYearEnd;
            short iYearNumberOffset;


            if (newVal == null)
            {
                throw new Exception("E_POINTER");
            }

            _dtStartDate = newVal.EffectiveDate;
            _dtEndDate = newVal.EndDate;
            _eCycleType = newVal.CycleType;
            _eDayWeekEnum = newVal.DateOfWeek;
            _ePDCounting = newVal.PDCounting;
            _eYearEndElect = newVal.YearEndElect;
            _iFYEndMonthEnum = newVal.FYEndMonth;
            iYearNumberOffset = newVal.YearNumberOffset;

            FYNum = (short)(iYearNumber + iYearNumberOffset);
            CycleType = (_eCycleType);
            DateOfWeek = (_eDayWeekEnum);
            PDCounting = (_ePDCounting);
            YearEndElect = (_eYearEndElect);
            FYEndMonth = (_iFYEndMonthEnum);

            if (_iFYEndMonthEnum > (_dtStartDate.Month))
            {
                lYearEnd = (_dtStartDate.Year) + iYearNumber - 1;
            }
            else if (_iFYEndMonthEnum == (_dtStartDate.Month))
            {
                lYearEnd = (_dtStartDate.Year) + iYearNumber;

                FiscalYearEndDate((_dtStartDate.Year), out tmpDate);

                /*  _dtStartDate <= tmpDate   = sign added by KENT fix JLRU-00036 */
                if (_dtStartDate <= tmpDate && (((tmpDate - _dtStartDate).TotalDays > 6) || (iYearNumberOffset == 0 /*&& iYearNumber == 1*/)))
                    lYearEnd--;
            }
            else
            {
                lYearEnd = (_dtStartDate.Year) + iYearNumber;
            }
            if (iYearNumber == 1)
            {
                YRStartDate = (_dtStartDate);
            }
            else
            {
                FiscalYearEndDate(lYearEnd - 1, out tmpDate);
                YRStartDate = tmpDate.AddDays(1);
            }

            FiscalYearEndDate(lYearEnd, out tmpDate);
            YREndDate = (tmpDate);

            if ((_dtEndDate > DateTime.MinValue && _dtEndDate.AddDays(-7) < m_dtEndDate) || (m_dtStartDate < _dtEndDate && _dtEndDate < m_dtEndDate))
            {
                YREndDate = (_dtEndDate);
            }

            return true;
        }

        public DateTime GetNearDayOfWeek (DateTime dtDate)
        {
            short iDayDiff;

            iDayDiff = (short)((short)(m_eDayWeekEnum) - 1 - (short)(dtDate.DayOfWeek)); 
            if ( Math.Abs(iDayDiff) < 4 )
            {
                // look at the time line built from the day of week
                // 1 2 3 4 5 6 7
                // if ePeriodDayOfWeek - Weekday(dtDate) = iDayDiff is < 4 and
                // iDayDiff is > 0 then we want to move forward iDayDiff days from the
                // dtDate
                // if abs(ePeriodDayOfWeek - Weekday(dtDate)= iDayDiff) is < 4 and
                // iDayDiff is < 0 then we want to move backward iDayDiff days from the
                // dtDate.  Because iDayDiff now is negative then we have the same if
                // condition.
                return dtDate.AddDays(iDayDiff);
            }
            else if ( iDayDiff > 0 )
            {
                // if iDayDiff >= 4 (refer to 0 < iDayDiff < 4  move forward)
                // we want to move backward (7 - iDayDiff) days.
                return dtDate.AddDays ( - (7 - iDayDiff));
            }
            else
            {
                // if iDayDiff <= -4 (refer to -4 < iDayDiff < 0  move backward)
                // we want to move backward (7 - iDayDiff) days.
                return dtDate.AddDays (7 + iDayDiff);
            }
        }
    }
}
