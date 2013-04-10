using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFACalendar
{
    public class BAFASCalendar : IBACalendar, IBACalendarManager
    {
        const int BUCKETCOUNT = 30;


        List<IBACycleObject> m_FYList;
        List<BAFiscalYearHolder> m_yearBuckets;
        XMLPersistance m_persist;
        bool m_IsDirty;
        bool m_translate;
        short m_nextBucket;

        public BAFASCalendar()
        {
            m_FYList = new List<IBACycleObject>();
            m_yearBuckets = new List<BAFiscalYearHolder>(BUCKETCOUNT);
            for (int i = 0; i < BUCKETCOUNT; i++)
                m_yearBuckets.Add(new BAFiscalYearHolder());

        }

        public bool BuildFiscalYear(IBACycleObject objCalCycle, short YearNumber, out IBAFiscalYear objIFY)
        {
            IBAFiscalYear obj = new BAFASFiscalYear();

            obj.InitializeWithCycleObject(objCalCycle, YearNumber);
            objIFY = obj;

            return true;
        }

        public bool GetFiscalYear(DateTime dtDate, out IBAFiscalYear objIFY)
        {
            int iYear;
            IBACycleObject objCalCycle;
            IBAFiscalYear objFY;
            int count;
            int posi;
            DateTime dtEffectiveDate;
            DateTime dtEndDate;
            DateTime dtYREndDate;
            DateTime dtYRStartDate;

            if (m_FYList == null)
            {
                throw new Exception("Cycle list is uninitialized.");
            }
            count = m_FYList.Count;

            if (dtDate < new DateTime(1920, 1, 1))
                throw new Exception("Invalid date specified.");
            if (dtDate > new DateTime(3000, 1, 1)) //change to allow to date till 2999
                throw new Exception("Invalid date specified.");

            if (FindBucket(dtDate, out objIFY))
                return true;

            for (posi = 0; posi < count; posi++)
            {
                bool ThisOne;

                objCalCycle = m_FYList[posi];

                dtEffectiveDate = objCalCycle.EffectiveDate;
                dtEndDate = objCalCycle.EndDate;

                if (dtDate >= dtEffectiveDate &&
                     (dtEndDate <= DateTime.MinValue || dtEndDate >= dtDate))
                    ThisOne = true;
                else if (posi == 0 && dtDate.Month == dtEffectiveDate.Month &&
                          dtDate.Year == dtEffectiveDate.Year)
                {
                    dtDate = dtEffectiveDate;
                    ThisOne = true;
                }
                else
                    ThisOne = false;

                if (ThisOne)
                {
                    // Find the nearest iYearNum
                    iYear = ((dtDate.Year) - (dtEffectiveDate.Year) + 1);

                    // dtDate == dtEndDate means dtDate is a year end date
                    // if Month(dtDate) == 1 and Day(dtDate) <= 4
                    // it means this year end date is move to the next year for some reason
                    if (dtDate == dtEndDate && (dtDate.Month) == 1 && (dtDate.Day) <= 4)
                        iYear -= 1;

                    BuildFiscalYear(objCalCycle, (short)iYear, out objFY);
                    dtYRStartDate = objFY.YRStartDate;
                    dtYREndDate = objFY.YREndDate;

                    if (dtYREndDate < dtDate)
                    {
                        objFY = null;
                        BuildFiscalYear(objCalCycle, (short)(iYear + 1), out objFY);
                    }
                    else if (dtYRStartDate > dtDate)
                    {
                        while (dtYRStartDate > dtDate)
                        {
                            objFY = null;
                            iYear = iYear - 1;
                            BuildFiscalYear(objCalCycle, (short)iYear, out objFY);
                            dtYRStartDate = objFY.YRStartDate;
                            dtYREndDate = objFY.YREndDate;
                        }
                    }
                    //
                    // Save this year object in the cache.
                    //
                    m_yearBuckets[m_nextBucket].HoldMe(objFY);
                    objIFY = objFY;

                    return true;
                }


                objCalCycle = null;
            }
            throw new Exception("Invalid date specified.");
        }

        public bool GetFiscalYearByNum(short iYearNum, out IBAFiscalYear objIFY)
        {
            short iNumYears;
            short iYearNumOffset;
            IBACycleObject objCalCycle;
            int posi;
            bool hr = false;
            DateTime dtEndDate;
            IBAFiscalYear objFY;

            objIFY = null;
            if (m_FYList == null)
            {
                new Exception("Cycle list is uninitialized.");
            }

            for (posi = 0; posi < m_FYList.Count; posi++)
            {
                objCalCycle = m_FYList[posi];

                if (objCalCycle != null)
                {


                    iNumYears = objCalCycle.NumberOfYears;
                    iYearNumOffset = objCalCycle.YearNumberOffset;
                    dtEndDate = objCalCycle.EndDate;

                    if (iYearNumOffset < iYearNum && (dtEndDate <= DateTime.MinValue || iYearNum <= iYearNumOffset + iNumYears))
                    {
                        if ( (FindBucket(DetermineDateForFY(objCalCycle, (short)(iYearNum - iYearNumOffset)), out objIFY)) )
                        	return true;

                        if (!(hr = BuildFiscalYear(objCalCycle, (short)(iYearNum - iYearNumOffset), out objFY)))
                        {
                            return hr;
                        }
                        //
                        // Save this year object in the cache.
                        //
                        m_yearBuckets[m_nextBucket].HoldMe(objFY);
                        objIFY = objFY;
                        return true;
                    }


                    objCalCycle = null; ;
                }
                objIFY = null;
                throw new Exception("Invalid year number specified.");
            }
            return false;
        }

        public bool GetFiscalYearNum(DateTime dtDate, out short pVal)
        {
            IBAFiscalYear objIFY = null;

            pVal = 0;
            if (! GetFiscalYear(dtDate, out objIFY))
            {
                return false;
            }
            pVal = objIFY.FYNum;
            return true;
        }

        public bool GetPeriod(DateTime dtDate, out IBACalcPeriod pVal)
        {
            IBAFiscalYear objIFY;
            IBACalcPeriod objIPeriod;
            bool hr;

            pVal = null;
            if (!(hr = GetFiscalYear(dtDate, out objIFY)))
            {
                return hr;
            }
            if (!(hr = objIFY.GetPeriod(dtDate, out objIPeriod)))
            {
                return hr;
            }
            pVal = objIPeriod;
            return true;
        }

        public bool FirstShortYear(out DateTime pVal)
        {
            int posi, count;
            IBACycleObject objCalCycle;
            IBAFiscalYear objIFY = null;
            bool b;
            DateTime dtDate;
            bool hr;

            pVal = DateTime.MinValue;

            if (m_FYList == null)
            {
                throw new Exception("Cycle list is uninitialized.");
            }

            count = m_FYList.Count;
 
            for (posi = 0; posi < count; posi++)
            {
                objCalCycle = m_FYList[posi];
                dtDate = objCalCycle.EffectiveDate;

                objCalCycle = null;
                objIFY = null;

                if (!(hr = GetFiscalYear(dtDate, out objIFY)))
                {
                    return hr;
                }

                b = objIFY.IsShortYear;
 
                if (b)
                {
                    pVal = dtDate;
                    break;
                }
            }

            return true;
        }

        public bool LastShortYear(out DateTime pVal)
        {
            int posi, count;
            IBACycleObject objCalCycle;
            IBAFiscalYear objIFY = null;
            bool b;
            DateTime dtDate;
            bool hr;

            pVal = DateTime.MinValue;

            if (m_FYList == null)
            {
                throw new Exception ("Cycle list is uninitialized.");
            }

            count = m_FYList.Count;

            for (posi = count - 1; posi >= 0; posi--)
            {
                objCalCycle = m_FYList[posi];
                dtDate = objCalCycle.EffectiveDate;

                objCalCycle = null;
                objIFY = null;

                if (!(hr = GetFiscalYear(dtDate, out objIFY)))
                {
                    return hr;
                }

                b = objIFY.IsShortYear;

                if (b)
                {
                    pVal = dtDate;
                    break;
                }
            }

            return true;
        }

        public bool ShortYearList(out List<IBAFiscalYear> pVal)
        {
            int posi, count;
            IBACycleObject objCalCycle;
            IBAFiscalYear objIFY = null;
            bool b;
            DateTime dtDate;
            bool hr;

            pVal = null;

            if (m_FYList == null)
            {
                throw new Exception("Cycle list is uninitialized.");
            }

            count = m_FYList.Count;

            for (posi = 0; posi < count; posi++)
            {
                objCalCycle = m_FYList[posi];
                dtDate = objCalCycle.EffectiveDate;

                objCalCycle = null;
                objIFY = null;

                if (!(hr = GetFiscalYear(dtDate, out objIFY)))
                {
                    return hr;
                }

                b = objIFY.IsShortYear;

                if (b)   // it's a short year
                {
                    if (pVal == null)
                    {
                        // create the list if this is the first short year
                        pVal = new List<IBAFiscalYear>();
                    }
                    // add the fy to the list
                    pVal.Add(objIFY);
                }
            }

            return true;
        }


        public bool GetPeriodByNum(short iYearNum, short iPeriodNum, out IBACalcPeriod objPeriod)
        {
            IBAFiscalYear objIFY;
            bool hr;

            if (m_FYList == null)
                new Exception("Cycle list is uninitialized.");

            if ((hr = GetFiscalYearByNum(iYearNum, out objIFY)))
            {
                if (!(hr = objIFY.GetPeriodByNum(iPeriodNum, out objPeriod)))
                    return hr;
                else
                {
                    return true;
                }

            }
            objPeriod = null;
            return false;
        }

        public bool FiscalYearEndDate(int lYear, ECALENDARCYCLE_CYCLETYPE eCycleTypeEnum, short eFYEndMonthEnum, ECALENDARCYCLE_YEARENDELECTION eYREndElectionEnum, ECALENDARCYCLE_DATEOFWEEK ePdDayWeekEnum, out DateTime pVal)
        {
	        DateTime dtTmpDate;
	        int iDayOfWeek, iDayDiff;

	        // make end of month
	        dtTmpDate = DateTimeHelper.GetEndOfMonth(new DateTime(lYear, eFYEndMonthEnum, Constants.EndOfMonthSeed));

	        // make the function default to YEARENDELECTION_LASTDAY
	        pVal = dtTmpDate;

	        if (eCycleTypeEnum != ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_MONTHLY)
	        {
		        if (eYREndElectionEnum == ECALENDARCYCLE_YEARENDELECTION.YEARENDELECTION_CLOSESTWEEKDAY)
			        pVal = DateTimeHelper.GetNearDayOfWeek(dtTmpDate, ePdDayWeekEnum);
		        else if (eYREndElectionEnum == ECALENDARCYCLE_YEARENDELECTION.YEARENDELECTION_LASTWEEKDAY)
		        {
			        iDayOfWeek = (short)dtTmpDate.DayOfWeek;
			        iDayDiff = (short)(iDayOfWeek - ePdDayWeekEnum - 1);
			        if ( iDayDiff < 0 )
			        {
				        iDayDiff = (short)(7 + iDayDiff);
			        }
                    pVal = new DateTime(dtTmpDate.Year, dtTmpDate.Month, dtTmpDate.Day).AddDays(- iDayDiff);
		        }
	        }

	        return true;
        }

        public bool FiscalYearStartDate(int lYear, ECALENDARCYCLE_CYCLETYPE eCycleTypeEnum, short eFYEndMonthEnum, ECALENDARCYCLE_YEARENDELECTION eYREndElectionEnum, ECALENDARCYCLE_DATEOFWEEK ePdDayWeekEnum, out DateTime pVal)
        {
            bool hr;

            hr = FiscalYearEndDate(lYear - 1,
                                      eCycleTypeEnum,
                                      eFYEndMonthEnum,
                                      eYREndElectionEnum,
                                      ePdDayWeekEnum,
                                      out pVal);
            pVal = pVal.AddDays (1);
            return true;
        }

        public bool AddCycleEntry(DateTime dtEffectiveDate, ECALENDARCYCLE_CYCLETYPE eCycleType, short eFYEndMonth, ECALENDARCYCLE_DATEOFWEEK ePeriodDayOfWeek, ECALENDARCYCLE_YEARENDELECTION eYearEndElection, ECALENDARCYCLE_PDCOUNTING ePDCounting)
        {
            IBACycleObject objCalCycle;
            BAFASCycleObject objNewCalCycle;
            int posi, count, origCount;
            bool hr;
            DateTime dtFYStart, dtFYEnd, dtDate;
            ECALENDARCYCLE_DATEOFWEEK eTmpDateOfWeek;
            ECALENDARCYCLE_CYCLETYPE eTmpCycleType;
            ECALENDARCYCLE_YEARENDELECTION eTmpYearEndElection;
            IBACycleObject objPrevCalCycle;
            short eNumberOfYears, eYearNumberOffset, eTmpFYEndMonth;
            ECALENDARCYCLE_PDCOUNTING eTmpPDCounting;

            if (eCycleType == ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_MONTHLY)
            {
                eYearEndElection = ECALENDARCYCLE_YEARENDELECTION.YEARENDELECTION_LASTWEEKDAY;
                ePDCounting = ECALENDARCYCLE_PDCOUNTING.PDCOUNT_BACKWARD;
            }

            ClearBuckets();

            if (m_FYList == null)
            {
                throw new Exception("Cycle list is uninitialized.");
            }

            count = m_FYList.Count;

            origCount = count;

            if (count > 0)
            {
                for (posi = count - 1; posi >= 0; --posi)
                {
                    objCalCycle = m_FYList[posi];
                    dtDate = objCalCycle.EffectiveDate;


                    objCalCycle = null;

                    if (dtDate >= dtEffectiveDate.AddDays(-7))
                        --count;
                }

                if (count > 0)
                {
                    objCalCycle = m_FYList[count - 1];

                    if (objCalCycle == null)
                    {
                        throw new Exception("Invalid effective date.");
                    }

                    eTmpCycleType = objCalCycle.CycleType;
                    eTmpFYEndMonth = objCalCycle.FYEndMonth;
                    eTmpYearEndElection = objCalCycle.YearEndElect;
                    eTmpDateOfWeek = objCalCycle.DateOfWeek;
                    eTmpPDCounting = objCalCycle.PDCounting;

                    if (eTmpCycleType == eCycleType &&
                        eTmpDateOfWeek == ePeriodDayOfWeek &&
                        eTmpFYEndMonth == eFYEndMonth &&
                        eTmpYearEndElection == eYearEndElection &&
                        eTmpPDCounting == ePDCounting)
                    {
                        for (posi = origCount - 1; posi >= count; posi--)
                            m_FYList.RemoveAt(posi);
                        objCalCycle.EndDate = DateTime.MinValue;
                        return true;
                    }

                    if (!(FiscalYearEndDate((dtEffectiveDate.AddDays(-1)).Year,
                                            eTmpCycleType,
                                            eTmpFYEndMonth,
                                            eTmpYearEndElection,
                                            eTmpDateOfWeek,
                                            out dtFYEnd)) ||
                        !(FiscalYearStartDate((dtEffectiveDate.AddDays(-1)).Year + 1,
                                            eCycleType,
                                            eTmpFYEndMonth,
                                            eYearEndElection,
                                            ePeriodDayOfWeek,
                                            out dtFYStart)))
                    {
                        return false;
                    }

                    if (dtEffectiveDate != dtFYEnd && dtEffectiveDate != dtFYStart && dtEffectiveDate != dtFYEnd.AddDays(+1))
                    {
                        if ((dtEffectiveDate.Month) == 1 && (dtEffectiveDate.Day) < 7)
                        {
                            if (!(FiscalYearEndDate((dtEffectiveDate.AddDays(-7)).Year,
                                                    eTmpCycleType,
                                                    eTmpFYEndMonth,
                                                    eTmpYearEndElection,
                                                    eTmpDateOfWeek,
                                                    out dtFYEnd)) ||
                                !(FiscalYearStartDate((dtEffectiveDate.Year),
                                                    eCycleType,
                                                    eTmpFYEndMonth,
                                                    eYearEndElection,
                                                    ePeriodDayOfWeek,
                                                    out dtFYStart)))
                            {
                                return false;
                            }
                        }
                        if (dtEffectiveDate != dtFYEnd && dtEffectiveDate != dtFYStart && dtEffectiveDate != dtFYEnd.AddDays(+1))
                        {
                            throw new Exception("Not a valid fiscal year end");
                        }
                    }
                }
                count = origCount;

                // remove use case 8.1b 4.2.a.1 calendar cycles
                for (posi = count - 1; posi >= 0; posi--)
                {
                    objCalCycle = null;
                    objCalCycle = m_FYList[posi];
                    dtDate = objCalCycle.EffectiveDate;

                    objCalCycle = null;

                    if (dtDate >= dtEffectiveDate.AddDays(-7))
                    {
                        m_FYList.RemoveAt(posi);
                        --count;
                    }
                }
            }



            if (count <= 0)
            {

                if (dtEffectiveDate == DateTime.MinValue ||
                    eCycleType < ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_MONTHLY ||
                    eCycleType > ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_CUSTOM ||
                    eFYEndMonth < 0 ||
                    eFYEndMonth > 12 ||
                    ePeriodDayOfWeek < ECALENDARCYCLE_DATEOFWEEK.DATEOFWEEK_SUNDAY ||
                    ePeriodDayOfWeek > ECALENDARCYCLE_DATEOFWEEK.DATEOFWEEK_SATURDAY ||
                    eYearEndElection < ECALENDARCYCLE_YEARENDELECTION.YEARENDELECTION_LASTWEEKDAY ||
                    eYearEndElection > ECALENDARCYCLE_YEARENDELECTION.YEARENDELECTION_CLOSESTWEEKDAY)
                {
                    throw new Exception("Parameter out of range.");
                }

                objNewCalCycle = new BAFASCycleObject();
                objCalCycle = objNewCalCycle;

                objCalCycle.EffectiveDate = (dtEffectiveDate);
                objCalCycle.CycleType = (eCycleType);
                objCalCycle.DateOfWeek = (ePeriodDayOfWeek);
                objCalCycle.FYEndMonth = (eFYEndMonth);
                objCalCycle.YearEndElect = (eYearEndElection);
                objCalCycle.PDCounting = (ePDCounting);
            }
            else               // count > 0 (modify cycle)
            {
                // assign the CycleEndDate for the previous CBAFASCycleObject
                objCalCycle = m_FYList[count - 1];
                objCalCycle.EndDate = dtEffectiveDate.AddDays(-1);

                objCalCycle = null;

                objNewCalCycle = new BAFASCycleObject();
                objCalCycle = objNewCalCycle;

                objCalCycle.EffectiveDate = (dtEffectiveDate);
                objCalCycle.CycleType = (eCycleType);
                objCalCycle.DateOfWeek = (ePeriodDayOfWeek);
                objCalCycle.FYEndMonth = (eFYEndMonth);
                objCalCycle.YearEndElect = (eYearEndElection);
                objCalCycle.PDCounting = (ePDCounting);


                objPrevCalCycle = m_FYList[count - 1];
                dtDate = objPrevCalCycle.EffectiveDate;


                if (dtDate >= dtEffectiveDate)
                {
                    --count;
                    m_FYList.RemoveAt(count);
                }

                objPrevCalCycle = null;

                objPrevCalCycle = m_FYList[count - 1];

                eYearNumberOffset = objPrevCalCycle.YearNumberOffset;
                eNumberOfYears = objPrevCalCycle.NumberOfYears;

                objPrevCalCycle = null;

                if (count >= 2)
                {
                    objCalCycle.YearNumberOffset = (short)(eYearNumberOffset + eNumberOfYears);
                }
                else if (count == 1)
                {
                    objCalCycle.YearNumberOffset = (eNumberOfYears);
                }
            }

            m_FYList.Add(objCalCycle);
            m_IsDirty = true;
            return true;

        }

        public bool ClearCycleEntries()
        {
            if (m_FYList == null)
            {
                return true;
            }
            ClearBuckets();
            return true;
        }

        public bool Clone(out IBACalendar pVal)
        {
            throw new Exception("not implement");
        }

        public List<IBACycleObject> CycleList
        {
            get
            {
                return m_FYList;
            }
            set
            {
                m_IsDirty = true;
                m_FYList = null;
                m_FYList = value;
                ClearBuckets();
            }
        }

        public bool IsDirty
        {
            get
            {
                return m_IsDirty;
            }
            set
            {
                m_IsDirty = value;
            }
        }

        public DateTime StartOfBusiness
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        bool FindBucket(DateTime dt, out IBAFiscalYear pVal)
        {
            int i;
            long value;
            long min;

            for (i = 0; i < BUCKETCOUNT; i++)
            {
                if (m_yearBuckets[i].GetObject(dt, out pVal))
                    return true;
            }

            min = 99999999L;
            for (i = 0; i < BUCKETCOUNT; i++)
            {
                value = m_yearBuckets[i].get_Count();
                if (value < min)
                {
                    m_nextBucket = (short)i;
                    min = value;
                }
            }
            pVal = null;
            return false;
        }

        void ClearBuckets()
        {
            int i;

            for (i = 0; i < BUCKETCOUNT; i++)
                m_yearBuckets[i].HoldMe(null);
        }

        static DateTime DetermineDateForFY(IBACycleObject newVal, short iYearNumber)
        {
            DateTime _dtStartDate;
            short _iFYEndMonthEnum;
            int month;
            short iOffset;
            DateTime dtTarget = DateTime.MinValue;
            ECALENDARCYCLE_CYCLETYPE _eCycleType;

            if (newVal == null)
            {
                return DateTime.MinValue;
            }

            _dtStartDate = newVal.EffectiveDate;
            _eCycleType = newVal.CycleType;
            _iFYEndMonthEnum = newVal.FYEndMonth;
            iOffset = newVal.YearNumberOffset;

            month = (_dtStartDate.Month);
            if (iOffset == 0 && month == _iFYEndMonthEnum && _eCycleType == ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_MONTHLY)
            {
                dtTarget = new DateTime((_dtStartDate.Year) + iYearNumber - 1, _iFYEndMonthEnum, 10);
                if (dtTarget < _dtStartDate)
                    dtTarget = _dtStartDate;
            }
            else
            {
                if ((_dtStartDate.Day) > 18)
                    month++;
                if (_iFYEndMonthEnum < month)
                    dtTarget = new DateTime((_dtStartDate.Year) + iYearNumber, _iFYEndMonthEnum, 10);
                else
                    dtTarget = new DateTime((_dtStartDate.Year) + iYearNumber - 1, _iFYEndMonthEnum, 10);
            }
            return dtTarget;
        }


    }
}
