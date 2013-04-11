using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using SFABusinessTypes;
using SFACalendar;

namespace SFACalcEngine
{
    public class CalcEngine : IBACalcEngine
    {
        bool m_bLimitsApplied;

        string m_sLALForYear;
        string m_sBUPForYear;
        string m_sDBToSL;
        string m_sCalcFlags = string.Empty;
        string m_sAdjFlag;

        double[] m_dLimitArray = new double[4];
        double m_dAddl911Limit;

        DateTime m_deemedStartDate;
        DateTime m_deemedEndDate;

        DateTime m_dtCalcDate;
        double m_stored168KAmt;

        IBACalcLookUp m_pObjICalcLookUp;
        NameValueCollection m_DeprLookup;
        NameValueCollection m_AvgConvLookup;
        IBAFASAdjustmentAllocator m_pObjAdjAlloc;

        public CalcEngine()
        {
            m_DeprLookup = new NameValueCollection();
            m_AvgConvLookup = new NameValueCollection();

            m_AvgConvLookup.Add("NON", "SageFASCalcEngine.NoConvention");
            m_AvgConvLookup.Add("HY", "SageFASCalcEngine.HalfYearConvention");
            m_AvgConvLookup.Add("HYmb", "SageFASCalcEngine.HYMonthbasedConvention");
            m_AvgConvLookup.Add("FY", "SageFASCalcEngine.FYMonthBasedConvention");
            m_AvgConvLookup.Add("MHY", "SageFASCalcEngine.ModHalfYearConvention");
            m_AvgConvLookup.Add("MM", "SageFASCalcEngine.MidPeriodConvention");
            m_AvgConvLookup.Add("MMmb", "SageFASCalcEngine.MidMonthConvention");
            m_AvgConvLookup.Add("MMat", "SageFASCalcEngine.MidMonthATConvention");
            m_AvgConvLookup.Add("FM", "SageFASCalcEngine.FullPeriodConvention");
            m_AvgConvLookup.Add("FMmb", "SageFASCalcEngine.FullMonthConvention");
            m_AvgConvLookup.Add("FMat", "SageFASCalcEngine.FullMonthATConvention");
            m_AvgConvLookup.Add("NM", "SageFASCalcEngine.NextPeriodConvention");
            m_AvgConvLookup.Add("MMM", "SageFASCalcEngine.ModMidPeriodConvention");
            m_AvgConvLookup.Add("MQ", "SageFASCalcEngine.MidQuarterConvention");
            m_AvgConvLookup.Add("MQmb", "SageFASCalcEngine.MidQuarterConvention");

            //
            // Now add the depr methods
            //
            m_DeprLookup.Add("SL", "SageFASCalcEngine.StraightLineMethod");
            m_DeprLookup.Add("DBs", "SageFASCalcEngine.DecliningBalanceMethod");
            m_DeprLookup.Add("DBn", "SageFASCalcEngine.DecliningBalanceMethodNoSwitch");
            // Canadian BEGIN !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            m_DeprLookup.Add("cdnDBn", "SageFASCalcEngine.CdnDecliningBalanceMethod");
            // Canadian END ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            m_DeprLookup.Add("RV", "SageFASCalcEngine.RVMethod");
            m_DeprLookup.Add("SYD", "SageFASCalcEngine.SYDMethod");
            m_DeprLookup.Add("MT", "SageFASCalcEngine.MACRSTable");
            m_DeprLookup.Add("MF", "SageFASCalcEngine.MACRSFormula");
            m_DeprLookup.Add("AT", "SageFASCalcEngine.ACRSTable");
            m_DeprLookup.Add("AST", "SageFASCalcEngine.AltACRSTable");
            m_DeprLookup.Add("ASF", "SageFASCalcEngine.AltACRSFormula");
            m_DeprLookup.Add("MSF", "SageFASCalcEngine.AltMACRSFormula");
            m_DeprLookup.Add("MAF", "SageFASCalcEngine.AltMACRSFormula");
            m_DeprLookup.Add("MST", "SageFASCalcEngine.AltMACRSTable");
            m_DeprLookup.Add("MAT", "SageFASCalcEngine.AltMACRSTable");
            m_DeprLookup.Add("AMZ", "SageFASCalcEngine.AmortizationDeprMethod");
            m_DeprLookup.Add("NON", "SageFASCalcEngine.NONDeprMethod");

        }

        public IBACalcLookUp CalcLookUp
        {
            get
            {
                return m_pObjICalcLookUp;
            }
            set
            {
                m_pObjICalcLookUp = value;
            }
        }

        public bool CalculateProjection(IBADeprScheduleItem pObjSch, out List<IBAPeriodDetailDeprInfo> pOutList)
        {
            IBACalendar pCalendar;
            IBAFiscalYear pRunDateYear;
            IBACalendarManager CalendarManager = null;
            DateTime dtCalcEndDate = DateTime.MinValue;
            IBAPeriodDeprItem pdItem = new PeriodDeprItem();
            List<IBAPeriodDeprItem> objPDItems;
            DateTime dtRunDate = new DateTime(pObjSch.PlacedInServiceDate.Year + 100, 12, 31);

            // Set up the Calendar
            pCalendar = pObjSch.Calendar;
            CalendarManager = (IBACalendarManager)pCalendar;
            CalendarManager.AddCycleEntry(new DateTime(1920, 1, 1), ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_MONTHLY, 12, ECALENDARCYCLE_DATEOFWEEK.DATEOFWEEK_SUNDAY, ECALENDARCYCLE_YEARENDELECTION.YEARENDELECTION_LASTWEEKDAY, ECALENDARCYCLE_PDCOUNTING.PDCOUNT_BACKWARD);
            pCalendar.GetFiscalYear(dtRunDate, out pRunDateYear);
            dtCalcEndDate = pRunDateYear.YREndDate;

            // set up the CalcLookUp
            BADefaultCalcLookup calcLookup = (BADefaultCalcLookup)new BADefaultCalcLookup();
            this.CalcLookUp = calcLookup;

            // calc
            pOutList = null;
            calculateBonus168KAmount(pObjSch);
            if (!CalculatePDItemList(pObjSch, out objPDItems, dtCalcEndDate, ref pdItem, false))
                return false;
            setPeriodDetailDeprInfo(objPDItems, out pOutList);
            //setPeriodDetailDeprInfo(pCalendar, objPDItems, out pOutList);
            return true;
        }

        public IBAPeriodDetailDeprInfo CalculateDepreciation(IBADeprScheduleItem pObjSch, DateTime dtRunDate)
        {
            List<IBAPeriodDetailDeprInfo> pTmpList;
            IBAPeriodDetailDeprInfo periodItem = new PeriodDetailDeprInfo();
            
            CalculateProjection(pObjSch, out pTmpList);

            dtRunDate = dtRunDate.Date;
            foreach (IBAPeriodDetailDeprInfo pdi in pTmpList)
            {
                if (pdi.PeriodStartDate <= dtRunDate && dtRunDate <= pdi.PeriodEndDate || pdi.PeriodEndDate >= new DateTime(3000,1,1))
                {
                    periodItem = pdi;
                    break;
                }
            }
            return periodItem;
        }

        public bool CalculateFASDeprToDate(IBADeprScheduleItem pObjSch, DateTime dtEndDate, out List<IBAPeriodDeprItem> objPDItems)
        {
            IBACalendar pCalendar;
            IBAFiscalYear pEndDateYear;
            IBACalendarManager CalendarManager = null;
            DateTime dtCalcEndDate = DateTime.MinValue;
            IBAPeriodDeprItem pdItem = new PeriodDeprItem();

            // Set up the Calendar
            pCalendar = pObjSch.Calendar;
            CalendarManager = (IBACalendarManager)pCalendar;
            CalendarManager.AddCycleEntry(new DateTime(1920, 1, 1), ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_MONTHLY, 12, ECALENDARCYCLE_DATEOFWEEK.DATEOFWEEK_SUNDAY, ECALENDARCYCLE_YEARENDELECTION.YEARENDELECTION_LASTWEEKDAY, ECALENDARCYCLE_PDCOUNTING.PDCOUNT_BACKWARD);
            pCalendar.GetFiscalYear(dtEndDate, out pEndDateYear);
            dtCalcEndDate = pEndDateYear.YREndDate;

            BADefaultCalcLookup calcLookup = (BADefaultCalcLookup)new BADefaultCalcLookup();
            this.CalcLookUp = calcLookup;

            calculateBonus168KAmount(pObjSch);
            if (!CalculatePDItemList(pObjSch, out objPDItems, dtCalcEndDate, ref pdItem, false))
                return false;
            return true;
        }

        public double CalculateBonus168KAmount(IBADeprScheduleItem pObjSch)
        {
            if (pObjSch.Bonus911Percent <= 0)
                return 0;

            IBACalendar pCalendar;
            IBACalendarManager CalendarManager = null;
            pCalendar = pObjSch.Calendar;
            CalendarManager = (IBACalendarManager)pCalendar;
            CalendarManager.AddCycleEntry(new DateTime(1920, 1, 1), ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_MONTHLY, 12, ECALENDARCYCLE_DATEOFWEEK.DATEOFWEEK_SUNDAY, ECALENDARCYCLE_YEARENDELECTION.YEARENDELECTION_LASTWEEKDAY, ECALENDARCYCLE_PDCOUNTING.PDCOUNT_BACKWARD);

            //double d168KAmt = pObjSch.CalculateBonus911Amount;
            //pObjSch.Bonus911Amount = d168KAmt;
            //pObjSch.Stored168KAmount = d168KAmt;

            //double s179 = pObjSch.Section179;
            //double stored911Amt = pObjSch.Stored168KAmount;
            //double bonus911Amt = pObjSch.Bonus911Amount;
            //pObjSch.Section179 = s179 + (stored911Amt != 0 ? stored911Amt : bonus911Amt); // and add it to section 179          
            return calculateBonus168KAmount(pObjSch); 
        }

        public double CalculateFullCostBasis(IBADeprScheduleItem pObjSch)
        {
            bool InLastQtr = false;
            double CostBase = 0;
            ComputeFullCostBasis(pObjSch, false, out InLastQtr, out CostBase);
            return CostBase;
        }

        public bool CalculatePDItemList(IBADeprScheduleItem pObjSch, out List<IBAPeriodDeprItem> pColPDIList, DateTime dtCalcEndDate, ref IBAPeriodDeprItem pObjPDItem, bool bAdjFlag)
        {
            IBACalendar pObjCalendar = null;
            IBAFiscalYear pObjIFY = null;
            IBADeprMethod pObjDeprMethod = null;
            IBASwitchDepr pObjSwitchDepr;
            IBAAvgConvention pObjAvgConvetion = null;

            double dPriorAccum = 0;
            double dPriorAccumOffSet = 0;
            double dActualPriorAccum = 0;
            double dYearElapsed = 0;
            double dFullBasis = 0;
            double dActualDepr = 0;
            double dFullUseDepr = 0;
            double dLife = 0;
            double dRDeprAmt = 0;
            double dTmpAmt = 0;
            double dTmp = 0;
            double dDeferredAmount;
            double dTotalDeferredAmount = 0;
            double dUsedDeferredAmount = 0;
            double dTotalPersonalAmount = 0;
            double dAnnualPersonalAmount = 0;
            double dStartingPersonalAmount = 0;
            double dBeginYTDPersUse = 0;
            double dBeginPeriodDepr = 0;
            double dBeginPeriodAccum = 0;
            double dTotalDepreciationAllowed;
            double dAdjustmentAmount = 0;
            double dStAdjustmentAmount = 0;
            double dOriginalAdjAmount = 0;
            double dOriginalFullDepr = 0;  // needed for post allocation check to pass to CheckLimits
            double dOriginalAnnualDepr = 0;// needed for post allocation check to pass to CheckLimits

            DateTime dtStAdjCalcStart = DateTime.MinValue;
            DateTime dtStartDate = DateTime.MinValue;
            DateTime dtEndDate = DateTime.MinValue;
            DateTime dtTmpDate = DateTime.MinValue;
            DateTime dtPISDate = DateTime.MinValue;
            DateTime dtCalcStartDate = DateTime.MinValue;
            DateTime dtFYStartDate = DateTime.MinValue;
            DateTime dtAdjCalcStart = DateTime.MinValue;
            DateTime dtAdjPeriodStart = DateTime.MinValue;
            DateTime dtDispDate = DateTime.MinValue;
            DateTime dtPISPeriodEnd = DateTime.MinValue;

            short iFYStartNum = 0;
            short iFYDeemedStartNum = 0;
            short iFYPISStartNum = 0;
            short iFYEndNum = 0;
            short iFYNum = 0;
            short iPISYearOffset = 0;
            short iFYCalcEndNum = 0;
            short iYNum = 0;
            short iTmp = 0;
            short iCurrentYearNumber = 0;
            short PISYearNumber;

            LUXURYLIMIT_TYPE eLFlag;

            bool bPostLife = false;
            bool bDoDispCalcs = false;
            bool bAlreadySwitched = false;
            bool bRet = false;
            bool bDefersDepr = false;
            bool bAdjStillNeeded;
            string sCalcFlags = "";
            string sReduceBaseFlag = "";
            string avgConvention = "";
            string deprMethod;

            decimal curBeginYearAccum = 0.0M;
            decimal curBeginYTDExpense = 0.0M;
            decimal curAdjustAmount = 0.0M;
            decimal curTotalDeferred = 0.0M;
            decimal curDeferredUsed = 0.0M;

            bool hr = false;

            bool atLeastOneItemBuilt = false;
            decimal cyPeriodAmount = 0.0M;

            pColPDIList = null;
            pColPDIList = new List<IBAPeriodDeprItem>();

            FiscalYearCalcer pObjFYCalcer = new FiscalYearCalcer();
            ACEHandler pObjACEHandler = new ACEHandler();

            pObjCalendar = pObjSch.Calendar;
            deprMethod = pObjSch.DeprMethod;
            dtPISDate = pObjSch.PlacedInServiceDate;

            pObjACEHandler.Calendar = (pObjCalendar);
            pObjACEHandler.DeprScheduleItem = (pObjSch);
            pObjACEHandler.PISDate = (dtPISDate);
            pObjCalendar.GetFiscalYearNum(dtPISDate, out PISYearNumber);
            m_stored168KAmt = pObjSch.Stored168KAmount;

            //
            // Determine the end date of the PIS period.
            //
            {
                IBACalcPeriod pObjPeriod;

                pObjCalendar.GetPeriod(dtPISDate, out pObjPeriod);
                dtPISPeriodEnd = pObjPeriod.PeriodEnd;
            }


            if (pObjPDItem != null)
            {
                curAdjustAmount = 0;

                dtTmpDate = pObjPDItem.EndDate;
                cyPeriodAmount = (decimal)(pObjPDItem.EndPeriodAccum);
                curAdjustAmount = pObjPDItem.AdjustAmount;
                try
                {
                    pObjCalendar.GetFiscalYearNum(dtTmpDate.AddDays(1), out iCurrentYearNumber);
                }
                catch
                {
                    iCurrentYearNumber = PISYearNumber;
                }
                if (iCurrentYearNumber > PISYearNumber && dtTmpDate == dtPISDate && CurrencyHelper.CurrencyToDouble(cyPeriodAmount) == 0)
                    iCurrentYearNumber--;

                dOriginalAdjAmount = (double)(curAdjustAmount);
                curAdjustAmount = 0;
            }
            else 
            {
                // We have no prior calc information, therefore current = PIS.
                iCurrentYearNumber = PISYearNumber;
            }

            if (!(hr = pObjACEHandler.FirstACEProcess(PISYearNumber, iCurrentYearNumber, ref deprMethod, ref m_sCalcFlags)))
                return hr;

            // KENT start fix MS MC-00037 and MBalm-00041
            bool b = pObjSch.UseACEHandling;
            // KENT end 

            if (!CreateDeprMethod(deprMethod, false, out pObjDeprMethod) || !pObjDeprMethod.GetAvgConvention(pObjSch, ref avgConvention))
            {
                if (b)
                {
                    string orgACEdeprMethod = pObjSch.DeprMethod;
                    IBADeprMethod orgObjACEDeprMethod;
                    if (CreateDeprMethod(orgACEdeprMethod, false, out orgObjACEDeprMethod))
                    {
                        if (!orgObjACEDeprMethod.GetAvgConvention(pObjSch, ref avgConvention))
                            avgConvention = pObjSch.AvgConvention;
                        orgObjACEDeprMethod = null;
                    }
                    else
                    {
                        avgConvention = pObjSch.AvgConvention;
                    }
                }
                else
                {
                    avgConvention = pObjSch.AvgConvention;
                }
             }

            if (!CreateAvgConvention(avgConvention, out pObjAvgConvetion))
            {
                return false;
            }
            dLife = pObjSch.DeprLife;

            if (b)
            {
                dLife = pObjSch.ADSLife;
            }
            if (!b || dLife == 0)
            {
                dLife = pObjSch.DeprLife;
            }
	        if (dLife <= 0 && string.Compare(deprMethod, "NON") == 0 )
		        dLife = 99;

            if (!(pObjAvgConvetion.Initialize(pObjCalendar, dtPISDate, dLife)))
                return false;

            //calculate the start date and end date
            CalcDeemDates(pObjAvgConvetion, pObjCalendar, dtPISDate, dLife, out dtStartDate, out dtEndDate);
            if (dtStartDate <= DateTime.MinValue || dtEndDate <= DateTime.MinValue)
            {
                return false;
            }

            m_deemedStartDate = dtStartDate;
            m_deemedEndDate = dtEndDate;

	        //
	        // Process special custom depr method rules here
	        //
            string baseName = string.Empty;

		    baseName = pObjDeprMethod.BaseShortName;

		    if ( string.Compare(baseName, "~~custom~~") == 0 )
		    {
			    short deemedStartYearNumber;

                pObjCalendar.GetFiscalYearNum(dtStartDate, out deemedStartYearNumber);
			    //
			    // If FAS Custom method, and deemed start date is in the year after PIS date,
			    // set the deemed start date to the PIS date.
			    //
			    if ( PISYearNumber < deemedStartYearNumber )
				    dtStartDate = dtPISDate;
		    }

            //calc the year elapsed and the prior accum
            if (pObjPDItem != null)
            {
                decimal cyPers;
                decimal cyPersYTD;
                decimal cyTmp;
                dtTmpDate = pObjPDItem.EndDate;

                if (dtTmpDate <= DateTime.MinValue || (dtTmpDate <= dtPISDate && string.Compare(deprMethod, "RV") != 0))
                {
                    dYearElapsed = 0;
                    dtCalcStartDate = dtPISDate;
                }
                else
                {
                    dYearElapsed = CalculateYearElapsed(pObjSch, pObjAvgConvetion, pObjCalendar, dtTmpDate.AddDays(+1));
                    if (dYearElapsed < 0)
                        dYearElapsed = 0;
                    dtCalcStartDate = dtTmpDate.AddDays(+1);
                }

                if (dtTmpDate > DateTime.MinValue)
                {

                    curBeginYearAccum = pObjPDItem.EndDateBeginYearAccum;
                    curBeginYTDExpense = (decimal)pObjPDItem.EndDateYTDExpense;
                    curAdjustAmount = pObjPDItem.AdjustAmount;
                    curTotalDeferred = pObjPDItem.EndDateDeferredAccum;
                    curDeferredUsed = pObjPDItem.EndDateYTDDeferred;
                    cyPers = pObjPDItem.EndDatePersonalUseAccum;
                    cyPersYTD = pObjPDItem.EndDateYTDPersonalUse;
                    cyTmp = (decimal)pObjPDItem.EndPeriodAccum;
                }
                else
                {
                    curBeginYearAccum = 0;
                    curBeginYTDExpense = 0;
                    curAdjustAmount = 0;
                    curTotalDeferred = 0;
                    curDeferredUsed = 0;
                    cyPers = 0;
                    cyPersYTD = 0;
                    cyTmp = 0;
                }

                dBeginPeriodDepr = (double)(cyTmp);
                curAdjustAmount = 0;
                dPriorAccum = (double)(curBeginYearAccum);
                dBeginPeriodAccum = (double)(curBeginYTDExpense);
                dActualPriorAccum = dPriorAccum;
                dTotalDeferredAmount = (double)(curTotalDeferred) + (double)(curDeferredUsed);
                dUsedDeferredAmount = 0;
                dTotalPersonalAmount = ((double)(cyPers));
                dAnnualPersonalAmount = ((double)(cyPersYTD));
                dBeginYTDPersUse = dAnnualPersonalAmount;

                //
                // We need to check if the starting period information is in the prior year so
                // that we can set up the accum variables properly.
                //
                if (dtTmpDate > DateTime.MinValue && (dtTmpDate != dtPISDate || (double)(cyTmp) != 0))
                {
                    short EndDateYear;
                    short StartDateYear;

                    if (!(pObjCalendar.GetFiscalYearNum(dtTmpDate, out EndDateYear)) ||
                        !(pObjCalendar.GetFiscalYearNum(dtTmpDate.AddDays(+1), out StartDateYear)))
                        return false;
                    // We have the numbers, now check them and set the accum variables.
                    if (EndDateYear < StartDateYear)
                    {
                        dPriorAccum += (double)(curBeginYTDExpense);
                        dPriorAccum += (double)(curAdjustAmount);
                        dTotalPersonalAmount += dAnnualPersonalAmount;
                        dAnnualPersonalAmount = 0;
                        dBeginYTDPersUse = 0;
                        dBeginPeriodAccum = 0;
                    }
                    else
                    {
                    }
                    dStartingPersonalAmount = dAnnualPersonalAmount;
                    dPriorAccumOffSet = 0;
                    dActualPriorAccum = dPriorAccum;
                }
                else
                {
                    dPriorAccum += (double)(curBeginYTDExpense);
                    dPriorAccum += (double)(curAdjustAmount);
                    dActualPriorAccum = dPriorAccum;
                    dPriorAccumOffSet = dPriorAccum;
                }
            }
            else
            {
                dtCalcStartDate = m_deemedStartDate;
                bAdjFlag = false;
            }

            if (!(pObjACEHandler.CalculateACEYearInformation(this, pObjAvgConvetion)))
                return false;

            if (bAdjFlag)
            {
                pObjDeprMethod = null;
                if (!(CreateDeprMethod(deprMethod, bAdjFlag, out pObjDeprMethod)))
                    return false;
            }

            if (!InitializeDeprMethod(pObjSch, pObjAvgConvetion, pObjDeprMethod))
                return false;


            IBADeprMethod pObjAutoDeprMethod;
            if (!(CreateDeprMethod(deprMethod, bAdjFlag, out pObjAutoDeprMethod)) ||
                !(InitializeDeprMethod(pObjSch, pObjAvgConvetion, pObjAutoDeprMethod)))
                return false;

	        // fix bug 6595.  may need another flag to indicate a asset has beg info with rv method 
	        // instead changing the interface, use the DBPercent as this flag
            if(string.Compare(deprMethod, "RV") == 0 && dtTmpDate > DateTime.MinValue && dLife < 1)
	        {
		        decimal cyTmp;

		        cyTmp = 0;
		        cyTmp = pObjPDItem.AdjustAmount;
		        if (((double)(cyTmp)) < 0)
			        pObjDeprMethod.DBPercent = 100;
	        }

            //
            // Now we need to adjust the depr values for ACE.
            //
            if (!(hr = pObjACEHandler.InitializeACEDeprMethod(iCurrentYearNumber, m_sCalcFlags, dYearElapsed, dActualPriorAccum, pObjDeprMethod)))
                return hr;

            dTotalDepreciationAllowed = pObjDeprMethod.TotalDepreciationAllowed;

/* KENT
	//
	// At this point we need to see if we are already switched to some other depr method.
	// This code handles that by passing the prior calc flags (in m_sCalcFlags) to the
	// method and asking for the correct switch method.  We then retrieve the correct
	// flags from the switch method and make the switch method the depr method to use.
	//   RDBJ 3/12/2000
	{
		CComQIPtr<IBASwitchDepr, &IID_IBASwitchDepr> swDepr (pObjDeprMethod);
		CComPtr<IBADeprMethod> tmpMeth;

		if ( !!swDepr )
		{
			m_sDBToSL.Empty();
			if ( FAILED(hr = swDepr->GetCurrentMethod(m_sCalcFlags, &tmpMeth)) ||
				 FAILED(hr = tmpMeth->get_ParentFlags(&m_sDBToSL)) )
				return hr;
			pObjDeprMethod.Release();
			pObjDeprMethod = tmpMeth;
		}
	}
KENT */


            eLFlag = pObjSch.GetLuxuryFlag;
            dtPISDate = pObjSch.PlacedInServiceDate;

            m_bLimitsApplied = ((m_sCalcFlags != null && m_sCalcFlags.Contains("l")) ? true : false) || (eLFlag != LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_NOTAPPLY);

            LoadLuxuryTable(eLFlag, dtPISDate);

            //if pisdate is between May 6,2003 and Dec 31 2005,  add 7650.00, otherwise add 4600 to the standard limit.
            short bonus911Percent = pObjSch.Bonus911Percent;
            short zoneCode = -1;
            m_dAddl911Limit = 0.0;

            if (bonus911Percent > 0 && m_dLimitArray[0] > 0)
            {
                if (dtPISDate <= new DateTime(2003, 5, 5))
                {
                    m_dAddl911Limit = 4600.0;
                }
                else if (dtPISDate <= new DateTime(2004, 12, 31))
                {
                    m_dAddl911Limit = 7650.0;
                }
                else if (new DateTime(2008, 1, 1) <= dtPISDate && dtPISDate <= new DateTime(2012, 12, 31))
                {
                    zoneCode = pObjSch.ZoneCode;

                    if (zoneCode == 0 || zoneCode == 3)
                        m_dAddl911Limit = 8000.0;
                }
            }

            short d168Pct = bonus911Percent;
            double dAutoPostUsageDeduction = 0;
            double dPostUsageDeduction = 0;
            dAutoPostUsageDeduction = pObjDeprMethod.PostUsageDeduction;

            pObjDeprMethod.DeemedStartDate = (dtStartDate);
            pObjDeprMethod.DeemedEndDate = (dtEndDate);
            pObjDeprMethod.PriorAccum = (dPriorAccum + dTotalPersonalAmount);
            pObjDeprMethod.YearElapsed = (dYearElapsed);

	        //HARD FIX FOR RV METHOD HANDLING
	        if ((dtCalcStartDate > dtPISDate) && (dtCalcStartDate > m_deemedStartDate) && ( pObjPDItem != null ) 
		        && (string.Compare(deprMethod, "RV") == 0))
	        {
		        DateTime dtFYStartDate1;
                DateTime dtFYEndDate1;

		        pObjIFY= null;

		        if ( !(hr = pObjCalendar.GetFiscalYear(dtCalcStartDate, out pObjIFY)) )
                    return hr;
		        dtFYStartDate1 = pObjIFY.YRStartDate;
                dtFYEndDate1   = pObjIFY.YREndDate;

		        // if calc start is at the beginning of fiscal year, don't do anything
		        if (dtTmpDate > DateTime.MinValue && dtCalcStartDate > dtFYStartDate1)
		        {
			        short ytdWeight;
			        short annualWeight;


			        pObjIFY.GetTotalAnnualPeriodWeights(out annualWeight);

			        if (m_deemedStartDate > dtFYStartDate1 && m_deemedStartDate < dtFYEndDate1)
			        {
				        pObjIFY.GetPeriodWeights(m_deemedStartDate, dtCalcStartDate.AddDays (-1), out ytdWeight);
			        }
			        else
			        {
				        pObjIFY.GetPreviousPeriodWeights(dtCalcStartDate,out ytdWeight);
			        }

			        if(dLife - dYearElapsed - (double)ytdWeight/(double)annualWeight < 0.05)
				        pObjFYCalcer.m_bFixRVdata = false;
			        else
			        {
				        pObjFYCalcer.m_bFixRVdata = true;
				        pObjFYCalcer.m_dAddToPriorAccum = dBeginPeriodAccum;
				        pObjFYCalcer.m_dAddToYearElapsed = (double)ytdWeight/(double)annualWeight;
			        }
			
		        }
	        }

	        //END OF RV HARD FIX

            //init calcer
            pObjFYCalcer.DeprMethod = (pObjDeprMethod);
            pObjFYCalcer.DeprScheduleItem = (pObjSch);
            pObjFYCalcer.AvgConvertion = (pObjAvgConvetion);
            pObjFYCalcer.DeemedStartDate = (dtStartDate);
            pObjFYCalcer.DeemedEndDate = (dtEndDate);
            pObjFYCalcer.LUXURYLIMITTYPEflag = (eLFlag);

            pObjFYCalcer.PISDate = (dtPISDate);

            pObjFYCalcer.CalcDate = (m_dtCalcDate);
            pObjFYCalcer.Convention = (avgConvention);

            dtDispDate = pObjSch.DispDate;
            pObjFYCalcer.DispDate = (dtDispDate);

            //init local vars
            bAlreadySwitched = false;
            sReduceBaseFlag = (pObjDeprMethod.AdjustedCost > pObjDeprMethod.Basis) ? "r" : "";

            if (dtCalcEndDate > DateTime.MinValue)
            {
                if (dtEndDate > dtCalcEndDate)
                {
                    dtEndDate = dtCalcEndDate;
                }
                if (!(pObjCalendar.GetFiscalYearNum(dtCalcEndDate, out iFYCalcEndNum)))
                    return hr;
            }

            if (dtDispDate > DateTime.MinValue)
            {
                if (dtEndDate > dtDispDate)
                {
                    dtEndDate = dtDispDate;
                }
            }

            if (!(hr = pObjCalendar.GetFiscalYearNum(dtEndDate, out iFYEndNum)) ||
                !(hr = pObjCalendar.GetFiscalYearNum(dtPISDate, out iFYPISStartNum)))
                return hr;

            if (!(hr = pObjCalendar.GetFiscalYearNum(dtStartDate, out iFYStartNum)))
            {
                if (dtStartDate < dtPISDate)
                    iFYStartNum = iFYPISStartNum;
                else
                    return hr;
            }

            iFYDeemedStartNum = iFYStartNum;

            iPISYearOffset = (short)(PISYearNumber - iFYDeemedStartNum);

            if (pObjPDItem != null)
            {
                dtTmpDate = pObjPDItem.EndDate;
                cyPeriodAmount = (decimal)pObjPDItem.EndPeriodAccum;

                if (dtTmpDate <= DateTime.MinValue)
                    dtTmpDate = dtStartDate;
                else
                    dtTmpDate = dtTmpDate.AddDays(1);

                if (dtTmpDate == dtPISDate.AddDays(1) && (double)(cyPeriodAmount) == 0)
                {
                    dtTmpDate = dtTmpDate.AddDays(-1);
                }
                if (!(hr = pObjCalendar.GetFiscalYearNum(dtTmpDate, out iFYStartNum)))
                {
                    if (dtTmpDate < dtPISDate)
                        iFYStartNum = iFYPISStartNum;
                    else
                        return hr;
                }
                else if (dtDispDate != DateTime.MinValue && dtTmpDate > dtDispDate)		//	KENT RM Gr-00189
                {
                    iFYStartNum++;
                    pObjFYCalcer.DeemedStartDate = (dtTmpDate);		//SAI JSchon-00174
                }
            }


            dtTmpDate = pObjSch.PlacedInServiceDate;
            pObjCalendar.GetFiscalYearNum(dtTmpDate, out iFYNum);
            pObjDeprMethod.YearNum = (short)(iFYStartNum - iFYNum + 1);

            IBADeprTableSupport tableSupp = pObjDeprMethod as IBADeprTableSupport;

            if (iFYStartNum < iFYPISStartNum)
                iFYNum = iFYPISStartNum;
            else
                iFYNum = iFYStartNum;

	        if ( tableSupp != null )
	        {
                bDefersDepr = false;
 		        bPostLife = false;
                bDefersDepr = tableSupp.DeferShortYearAmount;
                try
                {
                    bPostLife = tableSupp.InPostRecovery;
                }
                catch
                {
			        bPostLife = (iFYNum >= iFYEndNum + 1) ? true : false;
		        }
	        }
	        else
                bPostLife = (iFYNum >= iFYEndNum + 1) ? true : false;

	        if ( bPostLife )
	        {
		        sCalcFlags = "";
		        if ( string.Compare (m_sCalcFlags, "s") != 0 )
			        sCalcFlags += "s";
		        if ( string.Compare (m_sCalcFlags, "v") != 0 )
			        sCalcFlags += "v";
	        }

            if ( m_pObjAdjAlloc != null)
            {
                //
                // If we have an adjustment allocator, then initialize it here
                //
                m_pObjAdjAlloc.schedule = pObjSch;
                m_pObjAdjAlloc.Calendar = pObjCalendar;
                m_pObjAdjAlloc.DeemedEndDate = m_deemedEndDate;
            }

            dtAdjCalcStart = dtCalcStartDate;

            if (m_pObjAdjAlloc != null)
            {
                pObjIFY= null;
                if (!(hr = pObjCalendar.GetFiscalYearByNum(iFYNum, out pObjIFY)))
                    return hr;

                m_pObjAdjAlloc.FiscalYear = pObjIFY;
                dTmp = pObjDeprMethod.YearElapsed;
                dTmpAmt = pObjDeprMethod.Life;
                m_pObjAdjAlloc.AdjRemainingLife = (dTmpAmt - dTmp);
                if (!(hr = m_pObjAdjAlloc.AdjustmentStillNeeded(dtAdjCalcStart, out bAdjStillNeeded)))
                    return hr;
            }
            else 
                bAdjStillNeeded = false;

            dRDeprAmt = pObjDeprMethod.RemainingDeprAmt;


            while (bDoDispCalcs == false &&
                   ((((dActualPriorAccum + dTotalPersonalAmount) < dTotalDepreciationAllowed || dTotalDeferredAmount > 0.00001) &&
                     ((iFYNum < iFYEndNum + 1) ||
                      (bPostLife && m_bLimitsApplied && dRDeprAmt != 0 && (iFYCalcEndNum == 0 || iFYNum <= iFYCalcEndNum)) ||
                      (bPostLife && tableSupp != null && Math.Abs(dTotalDeferredAmount) > 0.0001 && (iFYCalcEndNum == 0 || iFYNum <= iFYCalcEndNum))))
                    || bAdjStillNeeded))
            {
                // Fix auto calculation for 168K method
                if ((eLFlag == LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_LUXURYCAR || eLFlag == LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_LTTRUCKSANDVANS) &&
                    d168Pct > 0 && !bPostLife)
                {
                    IBAFiscalYear pObjAutoIFY;
                    DateTime tmpDate = dtStartDate;
                    DateTime tmpEndDate;
                    double adjCost = 0;
                    double ProUseAmt = 0;
                    double AnnualDeprAmt = 0;
                    double AutoPriAccum = 0;
                    double dEscLife = 0;
                    //double dTmp;
                    double factor = 0;
                    double dBUPct = 0;
                    double dIUPct = 0;
                    double dtmp168Pct = bonus911Percent;

                    // recalculate the 168K amt withou auto limit and reset the post usage deduction
                    adjCost = pObjSch.AdjustedCost;

                    if (d168Pct == 100)
                        d168Pct = 50;

                    ProUseAmt = (adjCost * d168Pct / 100);
                    dPostUsageDeduction = ProUseAmt;

                    // get fiscal year for calcer
                    pObjAutoIFY = null;
                    if (!(hr = pObjCalendar.GetFiscalYearByNum(iFYNum, out pObjAutoIFY)))
                        return hr;
                    tmpEndDate = pObjAutoIFY.YREndDate;
                    pObjFYCalcer.FiscalYearInfo = (pObjAutoIFY);

                    // apply bus use
                    dTmp = pObjAutoDeprMethod.Basis;
                    if (!(hr = pObjSch.GetBusinessUse(tmpEndDate, ref dBUPct, ref dIUPct)))
                        return hr;

                    dBUPct += dIUPct;
                    if (bPostLife)
                        factor = dBUPct;
                    else
                        factor = CalcUsageFraction(pObjSch, dTmp, tmpDate);
                    dPostUsageDeduction = dPostUsageDeduction * factor;

                    // if it is not the first year then 
                    if (!pObjFYCalcer.IsFirstYear())
                    {
                        // get calc start date
                        pObjAutoIFY = null;
                        if (!(hr = pObjCalendar.GetFiscalYear(dtStartDate, out pObjAutoIFY)))
                            return hr;
                        tmpDate = pObjAutoIFY.YREndDate;
                        // init depr method before calc
                        pObjAutoDeprMethod.YearElapsed = (dEscLife);
                        pObjAutoDeprMethod.PostUsageDeduction = (dPostUsageDeduction);
                        pObjAutoDeprMethod.PriorAccum = (AutoPriAccum);

                        while (tmpDate < tmpEndDate) //dtCalcEndDate)
                        {
                            // check for switch
                            pObjFYCalcer.Fix168KAuto = true;
                            bool didSwitch;
                            do
                            {
                                didSwitch = false;
                                pObjSwitchDepr = null;
                                pObjSwitchDepr = pObjAutoDeprMethod as IBASwitchDepr;
                                if (pObjSwitchDepr != null)
                                {
                                    bRet = pObjSwitchDepr.SwitchRequired;

                                    if (bRet)
                                    {
                                        bRet = pObjSwitchDepr.CheckForSwitch;

                                        if (bRet)
                                        {
                                            pObjAutoDeprMethod = null;
                                            pObjAutoDeprMethod = pObjSwitchDepr.SwitchMethod;

                                            pObjFYCalcer.DeprMethod = (pObjAutoDeprMethod);
                                            m_sDBToSL = string.Empty;
                                            m_sDBToSL = pObjAutoDeprMethod.ParentFlags;
                                            didSwitch = true;
                                        }
                                    }
                                }
                            }
                            while (didSwitch);

                            // calc annual amt
                            AnnualDeprAmt = pObjAutoDeprMethod.CalculateAnnualDepr();

                            // get year fraction
                            pObjAutoIFY = null;
                            if (!(hr = pObjCalendar.GetFiscalYear(tmpDate, out pObjAutoIFY)))
                                return hr;
                            pObjFYCalcer.FiscalYearInfo = (pObjAutoIFY);

                            // apply year fraction
                            AnnualDeprAmt = AnnualDeprAmt * pObjFYCalcer.GetFraction();

                            if (dtmp168Pct == 100 && pObjFYCalcer.IsFirstYear() && AnnualDeprAmt + dPostUsageDeduction < dAutoPostUsageDeduction)
                                AnnualDeprAmt = dAutoPostUsageDeduction - dPostUsageDeduction;

                            // apply bus use
                            dTmp = pObjAutoDeprMethod.Basis;
                            pObjSch.GetBusinessUse(tmpDate, ref dBUPct, ref dIUPct);

                            dBUPct += dIUPct;
                            if (bPostLife)
                                factor = dBUPct;
                            else
                                factor = CalcUsageFraction(pObjSch, dTmp, tmpDate);
                            AnnualDeprAmt = AnnualDeprAmt * factor;

                            // pri accum
                            AutoPriAccum += AnnualDeprAmt;
                            pObjAutoDeprMethod.PriorAccum = (AutoPriAccum);

                            // calc year elapsed
                            dEscLife = pObjAutoDeprMethod.YearElapsed;

                            {
                                double dLife1;
                                double dFraction;
                                dLife1 = pObjAutoDeprMethod.Life;
                                dFraction = pObjFYCalcer.GetFraction(true);
                                if (dEscLife + dFraction > dLife1)
                                    dEscLife = dLife1;
                                else
                                    dEscLife += dFraction;
                                pObjAutoDeprMethod.YearElapsed = (dEscLife);
                            }

                            // move to next year
                            tmpDate = tmpDate.AddDays(1);
                            pObjAutoIFY = null;
                            if (!(hr = pObjCalendar.GetFiscalYear(tmpDate, out pObjAutoIFY)))
                                return hr;
                            tmpDate = pObjAutoIFY.YREndDate;
                        }
                    }

                    // use the no limit to calc 
                    pObjDeprMethod.PriorAccum = (AutoPriAccum);
                    pObjDeprMethod.PostUsageDeduction = (dPostUsageDeduction);

                    // reset calcer
                    pObjFYCalcer.DeprMethod = (pObjDeprMethod);
                    pObjFYCalcer.AutoPostUsageDeduction = dAutoPostUsageDeduction;
                    pObjFYCalcer.PostUsageDeduction = dPostUsageDeduction;

                }

                dDeferredAmount = 0;

                pObjIFY = null;
                if (!(hr = pObjCalendar.GetFiscalYearByNum(iFYNum, out pObjIFY)))
                    return hr;

                //
                //Process and handle ACE here.
                //
                {
                    IBADeprMethod outMethod;

                    if (!(hr = pObjACEHandler.ProcessACEForYear(iFYNum, pObjDeprMethod, dActualPriorAccum, this, pObjAvgConvetion, dYearElapsed, out outMethod)))
                        return hr;

                    //
                    // In case the depr method was changed, update the variables here.
                    //
                    pObjDeprMethod = null;
                    pObjDeprMethod = outMethod;
                    pObjFYCalcer.DeprMethod = (pObjDeprMethod);
                    dTotalDepreciationAllowed = pObjDeprMethod.TotalDepreciationAllowed;

                    if (pObjFYCalcer.Fix168KAuto)
                        dTotalDepreciationAllowed = dTotalDepreciationAllowed + dPostUsageDeduction - dAutoPostUsageDeduction;
                }


                {
                    pObjFYCalcer.FiscalYearInfo = (pObjIFY);
                    if (tableSupp != null)
                    {
                        if (!bPostLife)
                        {
                            bPostLife = false;
                            try
                            {
                                bPostLife = tableSupp.InPostRecovery;
                            }
                            catch
                            {
                                bPostLife = (iFYNum >= iFYEndNum + 1) ? true : false;
                            }
                        }
                    }
                    //check if switch needed
                    //calc depr amount
                    if (bPostLife == false)
                    {

                        if (m_pObjAdjAlloc != null)	// KENT START fix MS MC-00039
                        {
                            if (dtAdjCalcStart == dtCalcStartDate)
                            {

                                DateTime dtStAdjPeriodStart = DateTime.MinValue;
                                string sStAdjFlag = "";
                                double dLife1;
                                double dLifeElapsed;

                                m_pObjAdjAlloc.FiscalYear = (pObjIFY);
                                dLifeElapsed = pObjDeprMethod.YearElapsed;
                                dLife1 = pObjDeprMethod.Life;
                                m_pObjAdjAlloc.AdjRemainingLife = (dLife1 - dLifeElapsed);
                                dtStAdjCalcStart = pObjIFY.YRStartDate;
                                if (!(hr = m_pObjAdjAlloc.CalculateAdjustment(dtStAdjCalcStart, ref dStAdjustmentAmount, ref sStAdjFlag, ref dtStAdjPeriodStart)))
                                    return hr;
                                if (sStAdjFlag != null && sStAdjFlag[0] != 0)
                                {
                                    double dTemp;
                                    dTemp = pObjDeprMethod.PriorAccum;
                                    pObjDeprMethod.PriorAccum = (dTemp + dStAdjustmentAmount);
                                }
                            }

                        }
                        else
                        {
                            dAdjustmentAmount = 0;
                            dtAdjPeriodStart = DateTime.MinValue;
                            m_sAdjFlag = "";
                        }		// KENT END

                        bool didSwitch;
                        do
                        {
                            didSwitch = false;
                            pObjSwitchDepr = null;
                            pObjSwitchDepr = pObjDeprMethod as IBASwitchDepr;
                            if (pObjSwitchDepr != null)
                            {
                                bRet = pObjSwitchDepr.SwitchRequired;

                                if (bRet)
                                {
                                    bRet = pObjSwitchDepr.CheckForSwitch;
                                    if (bRet)
                                    {
                                        pObjDeprMethod = null;
                                        pObjDeprMethod = pObjSwitchDepr.SwitchMethod;

                                        pObjFYCalcer.DeprMethod = (pObjDeprMethod);
                                        m_sDBToSL = string.Empty;
                                        m_sDBToSL = pObjDeprMethod.ParentFlags;

                                        didSwitch = true;
                                    }
                                }
                            }
                        }
                        while (didSwitch);
                    }
                    //
                    // Handle the computation of FAS style adjustments here
                    //

                    if (m_pObjAdjAlloc != null)
                    {
                        m_pObjAdjAlloc.FiscalYear = (pObjIFY);
                        dTmp = pObjDeprMethod.YearElapsed;
                        dTmpAmt = pObjDeprMethod.Life;
                        m_pObjAdjAlloc.AdjRemainingLife = (dTmpAmt - dTmp);
                        m_pObjAdjAlloc.CalculateAdjustment(dtAdjCalcStart, ref dAdjustmentAmount, ref m_sAdjFlag, ref dtAdjPeriodStart);

                        if (m_sAdjFlag != null && m_sAdjFlag[0] != 0)
                            bAdjStillNeeded = false;
                    }
                    else
                    {
                        dAdjustmentAmount = 0;
                        dtAdjPeriodStart = DateTime.MinValue;
                        m_sAdjFlag = "";
                    }

                    if (bPostLife == false)
                    {
                        dFullUseDepr = pObjFYCalcer.Calculate(out dDeferredAmount, bPostLife, dTotalDeferredAmount);
                    }
                    else
                    {
                        if (tableSupp != null)
                        {
                            if (bDefersDepr)
                            {
                                if (eLFlag != LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_NOTAPPLY)
                                {
                                    dTotalDeferredAmount = dTotalDepreciationAllowed - dActualPriorAccum;
                                }
                                dFullUseDepr = pObjFYCalcer.Calculate(out dDeferredAmount, bPostLife, dTotalDeferredAmount);
                                if (eLFlag != LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_NOTAPPLY)
                                    dTotalDeferredAmount = 0;
                                if (dTotalDeferredAmount > 0.00004)
                                {
                                    if (dFullUseDepr > dTotalDeferredAmount)
                                    {
                                        dFullUseDepr = dTotalDeferredAmount;
                                        dDeferredAmount = -dTotalDeferredAmount;
                                    }
                                    else
                                    {
                                        //dTotalDeferredAmount -= dFullUseDepr;
                                        dDeferredAmount = -dFullUseDepr;
                                    }
                                }
                            }
                            else
                            {
                                dFullUseDepr = pObjDeprMethod.RemainingDeprAmt;
                                if (pObjFYCalcer.Fix168KAuto)
                                    dFullUseDepr = dFullUseDepr + dPostUsageDeduction - dAutoPostUsageDeduction;
                            }
                        }
                        else
                        {
                            if (eLFlag == LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_NOTAPPLY)
                                dFullUseDepr = 0;
                            else
                            {
                                dFullUseDepr = pObjDeprMethod.RemainingDeprAmt;
                                if (pObjFYCalcer.Fix168KAuto)
                                    dFullUseDepr = dFullUseDepr + dPostUsageDeduction - dAutoPostUsageDeduction;
                            }
                        }

                        //
                        // At this point we need to adjust the depr amount by the adjustment amount.  
                        // Otherwise we get a doubling of depreciation, or a proration of depreciation.
                        //
                        if (dAdjustmentAmount != 0.0 && (m_sAdjFlag) != null && (m_sAdjFlag)[0] == 'a' && dtAdjPeriodStart == dtAdjCalcStart)
                        {
                            dFullUseDepr -= dAdjustmentAmount;
                            if (dFullUseDepr < 0)
                                dFullUseDepr = 0;
                        }
                    }

                    //
                    // If the Placed In Service date is in the year before the deemed start date,
                    // we need to zero out the depreciation amount.  Otherwise we will have 
                    // doubled up depreciation.  (e.g. MMM in second half of last month).
                    // 
                    if (iFYNum == iFYPISStartNum && iFYPISStartNum < iFYDeemedStartNum)
                        dFullUseDepr = 0;

                    iYNum = pObjDeprMethod.YearNum;

                    // fix Ticket 8001689576 
                    // the code was introduced in version 38 
                    // now we want to set something to zero
                    if (Math.Abs(dFullUseDepr) < 0.008)
                        dFullUseDepr = 0;

                    dActualDepr = dFullUseDepr;

                    dtTmpDate = pObjIFY.YREndDate;
                    dTmp = pObjDeprMethod.Basis;


                    {

                        double factor = 0;
                        double dBUPct = 0;
                        double dIUPct = 0;

                        if (!(hr = pObjSch.GetBusinessUse(dtTmpDate, ref dBUPct, ref dIUPct)))
                            return hr;

                        dBUPct += dIUPct;
                        if (bPostLife)
                        {
                            factor = dBUPct;
                            if (factor + 0.000001 < 1.0)
                            {
                                m_sBUPForYear = "b";
                            }
                            else
                                m_sBUPForYear = "";
                        }
                        else
                            factor = CalcUsageFraction(pObjSch, dTmp, dtTmpDate);
                        dActualDepr = dFullUseDepr * factor;
                        dActualDepr = Currency.FormatCurrency(dActualDepr);

                        dOriginalAnnualDepr = dActualDepr;
                        //set auto limit for the year to restrict total depr for year to this limit
                        pObjFYCalcer.m_dAutoLimit = GetFYLimit(pObjIFY, iYNum + iPISYearOffset, pObjSch);

                        dActualDepr = CheckLimits(pObjIFY, iYNum + iPISYearOffset, dFullUseDepr, dActualDepr, dBUPct, pObjSch, ref dAnnualPersonalAmount, ref dAdjustmentAmount);
                        dFullUseDepr = dActualDepr + dAnnualPersonalAmount;
                        dOriginalFullDepr = dFullUseDepr;
                    }


                    sCalcFlags = m_sLALForYear;
                    sCalcFlags += sReduceBaseFlag;
                    sCalcFlags += m_sBUPForYear;
                    sCalcFlags += m_sDBToSL;
                    sCalcFlags += m_sAdjFlag;

                    {
                        string tmp;

                        if (!(hr = pObjACEHandler.get_ACEFlag(iFYNum, out tmp)))
                            return hr;
                        sCalcFlags += tmp;
                    }

                    //build pd list
                    pObjFYCalcer.GeneratePeriodDeprItem(
                        pObjIFY, dActualPriorAccum - dPriorAccumOffSet, dActualDepr,
                        pColPDIList, sCalcFlags, dTotalDeferredAmount, dDeferredAmount,
                        dTotalPersonalAmount, dAnnualPersonalAmount, m_bLimitsApplied,
                        dAdjustmentAmount, dtAdjPeriodStart, dStartingPersonalAmount,
                        pObjPDItem);


                    //
                    // Now make sure that the annual personal use amount is correct.
                    //
                    {
                        decimal cyAnnualPers;
                        int count;
                        IBAPeriodDeprItem pObjPD;

                        count = pColPDIList.Count;
                        pObjPD = pColPDIList[count - 1];

                        cyAnnualPers = pObjPD.EndDateYTDPersonalUse;

                        if (CurrencyHelper.CurrencyToDouble(cyAnnualPers) < dStartingPersonalAmount + dAnnualPersonalAmount)
                            dAnnualPersonalAmount = CurrencyHelper.CurrencyToDouble(cyAnnualPers) - dStartingPersonalAmount;
                    }

                    atLeastOneItemBuilt = true;

                    if (pObjFYCalcer.IsDispYear() && /*dtEndDate*/(dtCalcEndDate >= pObjFYCalcer.DispDate || dtCalcEndDate <= DateTime.MinValue))
                    {
                        bDoDispCalcs = true;
                    }
                    else
                    {
                        dTotalPersonalAmount += dAnnualPersonalAmount + dStartingPersonalAmount;
                        dStartingPersonalAmount = 0;
                        dTotalDeferredAmount += dDeferredAmount;
                        //update local vars
                        dtAdjCalcStart = pObjIFY.YREndDate;
                        dTmpAmt = pObjDeprMethod.PriorAccum;

                        dtAdjCalcStart = dtAdjCalcStart.AddDays(+1);

                        {
                            int count;
                            IBAPeriodDeprItem pObjPD;

                            //
                            // First, to fix problems with "Begin" information, re-get the 
                            // values that GeneratePeriodDeprItem determined.  Otherwise
                            // we would not have the proper Year To Date, Accum and Adjustment
                            // numbers.
                            //
                            count = pColPDIList.Count;

                            if (count > 0)
                            {
                                decimal cyAdjAmount;
                                decimal cyAccum;
                                decimal cyPersUse;
                                decimal cyPersUseYTD;

                                pObjPD = pColPDIList[count - 1];

                                cyAccum = (decimal)pObjPD.EndPeriodAccum;
                                cyAdjAmount = pObjPD.AdjustAmount;
                                cyPersUse = pObjPD.EndDatePersonalUseAccum;
                                cyPersUseYTD = pObjPD.EndDateYTDPersonalUse;

                                dActualPriorAccum = (double)(cyAccum); // KENT +	CurrencyToDouble(cyAdjAmount);

                                {
                                    pObjDeprMethod.PriorAccum = (dActualPriorAccum +
                                        (double)(cyPersUse) +
                                        (double)(cyPersUseYTD));
                                }
                            }
                            else
                            {
                                pObjDeprMethod.PriorAccum = (dTmpAmt + dFullUseDepr + dAdjustmentAmount /*+ dOriginalAdjAmount*/);
                                dActualPriorAccum = dActualPriorAccum + dActualDepr + dAdjustmentAmount /*+ dOriginalAdjAmount*/;
                            }
                        }

                        dTmpAmt = pObjDeprMethod.YearElapsed;

                        //
                        // If the Placed In Service date is in the year before the deemed start date,
                        // we need to zero out the Year Elapsed number, because no depreciable years
                        // have elapsed. (e.g. MMM in second half of last month).
                        // 
                        if (iFYNum == iFYPISStartNum && iFYPISStartNum < iFYDeemedStartNum)
                            pObjDeprMethod.YearElapsed = (0);
                        else
                        {
                            double dLife1;
                            double dFraction;
                            dLife1 = pObjDeprMethod.Life;
                            dFraction = pObjFYCalcer.GetFraction(true);
                            if (dTmpAmt + dFraction > dLife1)
                                dTmpAmt = dLife1;
                            else
                                dTmpAmt += dFraction;
                            pObjDeprMethod.YearElapsed = (dTmpAmt);
                        }

                        iTmp = pObjDeprMethod.YearNum;
                        pObjDeprMethod.YearNum = (short)(iTmp + 1);


                        dOriginalAdjAmount = 0; // Make sure this is cleared after the first year.
                        dTmpAmt = pObjDeprMethod.Life;
                        dTmp = pObjDeprMethod.YearElapsed;

                        if (tableSupp != null)
                        {
                            if (!bPostLife)
                            {
                                bPostLife = false;
                                try
                                {
                                    bPostLife = tableSupp.InPostRecovery;
                                }
                                catch
                                {
                                    bPostLife = (iFYNum >= iFYEndNum) ? true : false;
                                }
                            }
                        }
                        else if (dTmp >= dTmpAmt)
                        {
                            bPostLife = true;
                        }
                        iFYNum = (short)(iFYNum + 1);
                        dFullBasis = 0;
                        if (m_pObjAdjAlloc != null)
                        {
                            IBAFiscalYear FY;
                            if (!(hr = pObjCalendar.GetFiscalYearByNum(iFYNum, out FY)))
                                return hr;

                            m_pObjAdjAlloc.FiscalYear = (FY);
                            dTmp = pObjDeprMethod.YearElapsed;
                            dTmpAmt = pObjDeprMethod.Life;
                            m_pObjAdjAlloc.AdjRemainingLife = (dTmpAmt - dTmp);

                            if (!(hr = m_pObjAdjAlloc.AdjustmentStillNeeded(dtAdjCalcStart, out bAdjStillNeeded)))
                                return hr;
                        }
                    }

                }
                dRDeprAmt = pObjDeprMethod.RemainingDeprAmt;

                if (pObjFYCalcer.Fix168KAuto)
                    dRDeprAmt = dRDeprAmt + dPostUsageDeduction - dAutoPostUsageDeduction;
            }

            if ((dtCalcEndDate >= dtDispDate && dtCalcStartDate <= dtDispDate) && dtDispDate > DateTime.MinValue && (!atLeastOneItemBuilt || dtDispDate > m_deemedEndDate))
                bDoDispCalcs = true;
            m_sCalcFlags = sCalcFlags;

            if (m_sCalcFlags != null && m_sCalcFlags.Contains("v"))
            {
                string tmp;

                if (!(hr = pObjACEHandler.get_ACEFlag(iFYNum, out tmp)))
                    return hr;
                m_sCalcFlags += tmp;
            }


            //
            // In this case we did not have any items built.  Therefore we may need to build at least one
            // from the input information.
            //
            if (!atLeastOneItemBuilt)
            {
                DateTime YearStart;
                DateTime YearEnd;
                IBAPeriodDeprItem pPDItem;
                IBACalcPeriod pObjIPd;
                PeriodDeprItem obj;
                short iPdWeights;
                decimal curTmp;

                if (dtCalcStartDate > DateTime.MinValue)
                {
                    if (pObjFYCalcer.DispDate > DateTime.MinValue && pObjFYCalcer.DispDate < dtCalcStartDate)
                        dtCalcStartDate = pObjFYCalcer.DispDate;

                    pObjIFY = null;
                    if (!(hr = pObjCalendar.GetFiscalYear(dtCalcStartDate, out pObjIFY)))
                    {
                        return hr;
                    }
                    YearStart = pObjIFY.YRStartDate;
                    YearEnd = pObjIFY.YREndDate;
                    //
                    // If the calc start date is less than the Year End, create the PD Items.
                    //
                    if (dtCalcStartDate <= YearEnd && dtCalcStartDate >= YearStart)		//KENT
                    {
                        obj = new PeriodDeprItem();
                        pPDItem = obj;

                        pPDItem.StartDate = (dtCalcStartDate);
                        pPDItem.EndDate = (YearEnd);
                        iFYNum = pObjIFY.FYNum;
                        pPDItem.FYNum = (iFYNum);
                        pObjIFY.GetPeriodWeights(dtCalcStartDate, YearEnd, out iPdWeights);
                        pPDItem.TotalPeriodWeights = (iPdWeights);
                        pPDItem.EntryType = PERIODDEPRITEM_ENTRYTYPE.PERIODDEPRITEM_NORMAL;

                        CurrencyHelper.DoubleToCurrency(dPriorAccum, out curTmp);
                        pPDItem.BeginYearAccum = (curTmp);
                        pPDItem.EndDateBeginYearAccum = (curTmp);

                        CurrencyHelper.DoubleToCurrency(0, out curTmp);
                        pPDItem.EndDateDeferredAccum = (curTmp);

                        CurrencyHelper.DoubleToCurrency(0, out curTmp);
                        pPDItem.EndDateDeferredAccum = (curTmp);

                        CurrencyHelper.DoubleToCurrency(dBeginPeriodAccum, out curTmp);
                        pPDItem.BeginYTDExpense = (curTmp);

                        CurrencyHelper.DoubleToCurrency(0, out curTmp);
                        pPDItem.DeprAmount = (curTmp);
                        pPDItem.EndDateYTDDeferred = (curTmp);
                        pPDItem.CalcFlags = (m_sCalcFlags);
                        pColPDIList.Add(pPDItem);

                        // start KENT fix KHous-00296
                        curTmp = (decimal)pPDItem.EndPeriodAccum;

                        dActualPriorAccum = CurrencyHelper.CurrencyToDouble(curTmp);
                        // end KENT

                    }
                    else
                    {
                        // 
                        // We did not have a PD item and we are at the start of a year.  Therefore
                        // we need to adjust the FY pointer to be 1 year earlier so that the handling
                        // for the GeneratePDItemTrailer function to generate a PD item for this year.
                        //
                        pObjIFY = null;
                        if (!(hr = pObjCalendar.GetFiscalYear(YearStart.AddDays(-1), out pObjIFY)))
                        {
                            return hr;
                        }

                    }
                }
            }

            //
            // At this point we need to handle disposals.
            //
            if (bDoDispCalcs)
            {
                //
                // The first step to handle disposals is to make sure that the PD Item List goes
                // out to infinity.  This is needed for the case where the disposal occurs after 
                // the final year of depreciation.  Otherwise the update logic does not have 
                // a valid starting point for its work.  The problem that occurs is that the last
                // year of depreciation is lost.
                //
                {
                    if (pObjIFY == null)
                    {
                        //
                        // If we got here, it means that we are calculating after the end of the life
                        // and we did not calculate any PD items above.  Therefore make the end record
                        // use the calc start date.
                        //
                        if (dtCalcStartDate > DateTime.MinValue)
                        {
                            if (!(hr = pObjCalendar.GetFiscalYear(dtCalcStartDate, out pObjIFY)))
                            {
                                return hr;
                            }
                        }
                        else
                        {
                            if (!(hr = pObjCalendar.GetFiscalYearByNum(iFYEndNum, out pObjIFY)))
                            {
                                return hr;
                            }
                        }
                    }

                    //
                    // We only want to remember the s and v flags from prior years.
                    //
                    {
                        DateTime yearEnd;

                        yearEnd = pObjIFY.YREndDate;

                        if (pObjFYCalcer.DispDate > DateTime.MinValue && pObjFYCalcer.DispDate <= yearEnd)
                            if (m_sCalcFlags != null && m_sCalcFlags.Contains("d"))
                                m_sCalcFlags += "d";

                        //
                        // Handle the computation of FAS style adjustments here
                        //
                        if (m_pObjAdjAlloc != null)
                        {
                            m_pObjAdjAlloc.FiscalYear = (pObjIFY);
                            dTmp = pObjDeprMethod.YearElapsed;
                            dTmpAmt = pObjDeprMethod.Life;
                            m_pObjAdjAlloc.AdjRemainingLife = (dTmpAmt - dTmp);
                            if (!(hr = m_pObjAdjAlloc.CalculateAdjustment(dtAdjCalcStart, ref dAdjustmentAmount, ref m_sAdjFlag, ref dtAdjPeriodStart)))
                                return hr;

                            if (dActualPriorAccum + dAdjustmentAmount > dTotalDepreciationAllowed - dTotalPersonalAmount)
                            {
                                double adj = (dTotalDepreciationAllowed - dTotalPersonalAmount) -
                                             (dActualPriorAccum + dAdjustmentAmount);
                                dAdjustmentAmount -= adj;
                                if (dAdjustmentAmount < 0)
                                {
                                    dAdjustmentAmount = 0;
                                }
                            }
                            if ((m_sAdjFlag) != null && m_sAdjFlag[0] == 'a' && dAdjustmentAmount > 0)
                                if (m_sAdjFlag != null && m_sAdjFlag.Contains("a"))
                                    m_sCalcFlags += "a";
                        }
                        else
                        {
                            dAdjustmentAmount = 0;
                            dtAdjPeriodStart = DateTime.MinValue;
                            m_sAdjFlag = "";
                        }

                    }
                    pObjFYCalcer.GeneratePDItemTrailer(pObjIFY, dActualPriorAccum - dPriorAccumOffSet, pColPDIList, m_sCalcFlags, dTotalDeferredAmount, dtAdjPeriodStart, dAdjustmentAmount);
                }
                //
                // Now we need to handle the disposal itself.
                //
                double dFullDispDepr = 0.0;
                double dActualDispDepr = 0.0;
                double dMaxDeprAllowed = 0.0;

                dMaxDeprAllowed = dTotalDepreciationAllowed - dActualPriorAccum - dTotalPersonalAmount;
                if (dMaxDeprAllowed < -0.008)
                    dMaxDeprAllowed = 0;


                if (eLFlag != LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_NOTAPPLY && tableSupp != null && bDefersDepr && bPostLife)
                {
                    dTotalDeferredAmount = dTotalDepreciationAllowed - dActualPriorAccum;
                }
                pObjFYCalcer.FiscalYearInfo = (pObjIFY);

                if (dtAdjCalcStart == dtCalcStartDate)
                {
                    pObjFYCalcer.m_StAdjustAmount = dStAdjustmentAmount;
                    pObjFYCalcer.m_StAdjCalcStart = dtStAdjCalcStart;
                }

                // KENT both CalculateDispDepr() and UpdatePDItemlistForDisp() do not consider the adjustment calculation
                // when disposal in the adjustment year then need to coniser the adjustment calculation
                if (pObjFYCalcer.IsInBegInfoYear(dtCalcEndDate, pObjPDItem) && avgConvention == "HYmb" && bonus911Percent == 0)
                {
                    dFullDispDepr = pObjFYCalcer.CalculateDispDeprForBeginInfoYear(pColPDIList, pObjPDItem);
                }
                else
                {
                    dFullDispDepr = pObjFYCalcer.CalculateDispDepr(tableSupp, dTotalDeferredAmount, bDefersDepr, bPostLife, dMaxDeprAllowed, eLFlag,
                                                                   dAdjustmentAmount, dtAdjPeriodStart, dStartingPersonalAmount, pObjPDItem);
                }
                if (eLFlag != LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_NOTAPPLY && tableSupp != null && bDefersDepr && bPostLife)
                    dTotalDeferredAmount = 0;


                if (dFullDispDepr >= 0.0)
                {
                    double factor = 0;
                    double dBUPct = 0;
                    double dIUPct = 0;
                    double dPersUse = 0;

                    iYNum = pObjDeprMethod.YearNum;

                    dtTmpDate = pObjIFY.YREndDate;
                    dTmp = pObjDeprMethod.Basis;

                    if (!(hr = pObjSch.GetBusinessUse(dtTmpDate, ref dBUPct, ref dIUPct)))
                        return hr;

                    dBUPct += dIUPct;
                    if (bPostLife)
                    {
                        factor = dBUPct;
                        if (factor + 0.000001 < 1.0)
                        {
                            m_sBUPForYear = "b";
                        }
                        else
                            m_sBUPForYear = "";
                    }
                    else
                        factor = CalcUsageFraction(pObjSch, dTmp, dtTmpDate);
                    dActualDispDepr = dFullDispDepr * factor;
                    dActualDispDepr = Currency.FormatCurrency(dActualDispDepr);

                    dFullUseDepr = CheckLimits(pObjIFY, iYNum + iPISYearOffset, dFullDispDepr, dActualDispDepr, dBUPct, pObjSch, ref dPersUse, ref dAdjustmentAmount);
                    dActualDispDepr = dFullUseDepr;
                }
                else
                {
                    dActualDispDepr = dFullDispDepr;
                }


                sCalcFlags = m_sLALForYear;
                sCalcFlags += m_sBUPForYear;
                sCalcFlags += m_sDBToSL;
                //
                // Add the "d" flag to the calc flags.
                //
                if (m_sCalcFlags != null && m_sCalcFlags.Contains("d"))
                    m_sCalcFlags += "d";

                if (!(hr = pObjFYCalcer.UpdatePDItemlistForDisp(pObjSch,
                        pObjFYCalcer.DispDate, ref dActualDispDepr, m_deemedEndDate,
                        m_sCalcFlags, pColPDIList, dAdjustmentAmount, dtAdjPeriodStart,
                        dStartingPersonalAmount, pObjPDItem)))
                    return hr;
                dActualPriorAccum = dActualPriorAccum + dActualDispDepr;
            }

            //
            // At this point we need to make the final PD item entry that will go to infinity.  This entry helps us
            // to handle calc requests after the end of depreciation.  Make sure that we do not generate sub items.
            //
            //	pObjFYCalcer.put_BuildAllPDItems(false);
            if (pObjIFY == null)
            {
                //
                // If we got here, it means that we are calculating after the end of the life
                // and we did not calculate any PD items above.  Therefore make the end record
                // use the calc start date.
                //
                if (dtCalcStartDate > DateTime.MinValue)
                {
                    if (!(hr = pObjCalendar.GetFiscalYear(dtCalcStartDate, out pObjIFY)))
                    {
                        return hr;
                    }
                }
                else
                {
                    if (!(hr = pObjCalendar.GetFiscalYearByNum(iFYEndNum, out pObjIFY)))
                    {
                        return hr;
                    }
                }
            }

	        sCalcFlags = "";
            if (m_sCalcFlags != null && m_sCalcFlags.Contains("s"))
                sCalcFlags += "s";
            if (m_sCalcFlags != null && m_sCalcFlags.Contains("v"))
                sCalcFlags += "v";
	        m_sCalcFlags = sCalcFlags;

            {
		        DateTime yearEnd;

		        yearEnd = pObjIFY.YREndDate;
	
		        if ( pObjFYCalcer.DispDate > DateTime.MinValue && pObjFYCalcer.DispDate <= yearEnd )
                    if (m_sCalcFlags != null && m_sCalcFlags.Contains("d")) 
                        m_sCalcFlags += "d";
	        }

		    if ( m_pObjAdjAlloc != null )
		    {
			    m_pObjAdjAlloc.FiscalYear = pObjIFY;
			    dTmp = pObjDeprMethod.YearElapsed;
			    dTmpAmt = pObjDeprMethod.Life;
			    m_pObjAdjAlloc.AdjRemainingLife = dTmpAmt - dTmp;
			    m_pObjAdjAlloc.CalculateAdjustment(dtAdjCalcStart, ref dAdjustmentAmount, ref m_sAdjFlag, ref dtAdjPeriodStart);

			    if ( dActualPriorAccum + dAdjustmentAmount > dTotalDepreciationAllowed - dTotalPersonalAmount )
			    {
				    double adj = (dTotalDepreciationAllowed - dTotalPersonalAmount) - 
							     (dActualPriorAccum + dAdjustmentAmount);
				    dAdjustmentAmount -= adj;
				    if ( dAdjustmentAmount < 0  )
				    {
					    dAdjustmentAmount = 0;
				    }
			    }
			    if ( m_sAdjFlag != null && m_sAdjFlag[0] == 'a' && dAdjustmentAmount > 0 )
				    if ( m_sAdjFlag.Contains("a") )
					    m_sCalcFlags += "a";
		    }
		    else
		    {
                dAdjustmentAmount = 0;
                dtAdjPeriodStart = DateTime.MinValue;
                m_sAdjFlag = "";
		    }
	
            pObjFYCalcer.GeneratePDItemTrailer(pObjIFY, dActualPriorAccum - dPriorAccumOffSet, pColPDIList, m_sCalcFlags, dTotalDeferredAmount, dtAdjPeriodStart, dAdjustmentAmount);
            pObjDeprMethod = null;
            pObjAutoDeprMethod = null;
            return true;
        }

        public bool CreateDeprMethod(string DeprMethod, bool bAdjFlag, out IBADeprMethod pVal)
        {
            IBADeprMethod basemeth = null;
            IBADeprMethod meth = null;
            IBADeprMethod swMeth = null;
            IBASwitchDepr swDepr = null;
            bool hr;
            string progId;
            string switchName;
            string method;

            pVal = null;

            method = DeprMethod;
            progId = m_DeprLookup.Get(DeprMethod);

            if (progId == null)
                progId = "SageFASCalcEngine.CustomDeprMethod";


            if (!SmartDeprMethodCreator(progId, out meth))
                return false;

            basemeth = meth;
            swDepr = meth as IBASwitchDepr;
            while (swDepr != null)
            {
                switchName = string.Empty;
                progId = string.Empty;
                swMeth = null;
                switchName = swDepr.SwitchMethodName;
                progId = m_DeprLookup.Get(switchName);
                if (progId == null)
                    progId = "SageFASCalcEngine.CustomDeprMethod";

                if (!(hr = SmartDeprMethodCreator(progId, out swMeth)))
                    return hr;

                swDepr.SwitchMethod = (swMeth);

                meth = null;
                meth = swMeth;
                swDepr = null;
                swDepr = meth as IBASwitchDepr;
            }

            pVal = basemeth;

            return true;
        }

        public bool InitializeDeprMethod(IBADeprScheduleItem schedule, IBAAvgConvention convention, IBADeprMethod method)
        {
            if (schedule == null || method == null || convention == null)
                return false;

            return method.Initialize(schedule, convention);
        }

        public bool SmartDeprMethodCreator(string progId, out IBADeprMethod pVal)
        {
            if (progId.CompareTo("SageFASCalcEngine.StraightLineMethod") == 0)
            {
                pVal = new StraightLineMethod();
                return true;
            }
            else if (progId.CompareTo("SageFASCalcEngine.DecliningBalanceMethod") == 0)
            {
                pVal = new DecliningBalanceMethod();
                return true;
            }
            else if (progId.CompareTo("SageFASCalcEngine.DecliningBalanceMethodNoSwitch") == 0)
            {
                pVal = new DecliningBalanceMethodNoSwitch();
                return true;
            }
            else if (progId.CompareTo("SageFASCalcEngine.RVMethod") == 0)
            {
                pVal = new RVMethod();
                return true;
            }
            else if (progId.CompareTo("SageFASCalcEngine.SYDMethod") == 0)
            {
                pVal = new SYDMethod();
                return true;
            }
            else if (progId.CompareTo("SageFASCalcEngine.MACRSFormula") == 0)
            {
                pVal = new MACRSFormula();
                return true;
            }
            else if (progId.CompareTo("SageFASCalcEngine.AltACRSFormula") == 0)     // SA
            {
                pVal = new AltACRSFormula();
                return true;
            }
            else if (progId.CompareTo("SageFASCalcEngine.AltMACRSFormula") == 0)    // AD
            {
                pVal = new AltMACRSFormula();
                return true;
            }
            else if (progId.CompareTo("SageFASCalcEngine.AmortizationDeprMethod") == 0)    // AD
            {
                pVal = new AmortizationDeprMethod();
                return true;
            }
            else if (progId.CompareTo("SageFASCalcEngine.NONDeprMethod") == 0)    // AD
            {
                pVal = new NONDeprMethod();
                return true;
            } 
            pVal = null;
            return false;
        }

        private bool CreateAvgConvention(string AvgConvention, out IBAAvgConvention pVal)
        {
            pVal = null;

            string progId;
            bool hr;

            if (m_AvgConvLookup == null)
                throw new Exception("Lookup list not initialized.");

            progId = m_AvgConvLookup.Get(AvgConvention);

            if (!SmartAvgConventionCreator(progId, out pVal))
                return false;

            return true;
        }

        bool SmartAvgConventionCreator(string progId, out IBAAvgConvention pVal)
        {
            pVal = null;

		    if ( string.Compare(progId, "SageFASCalcEngine.NoConvention") == 0 )
	        {
                pVal = new NoConvention();
                return true;
	        }
            else if ( string.Compare(progId, "SageFASCalcEngine.HalfYearConvention") == 0 )
	        {
                pVal = new HalfYearConvention();
                return true;
            }
            else if (string.Compare(progId, "SageFASCalcEngine.HYMonthbasedConvention") == 0)
            {
                pVal = new HYMonthbasedConvention();
                return true;
            }
            else if (string.Compare(progId, "SageFASCalcEngine.FYMonthBasedConvention") == 0)
            {
                pVal = new FYMonthBasedConvention();
                return true;
            }
            else if (string.Compare(progId, "SageFASCalcEngine.ModHalfYearConvention") == 0)
            {
                pVal = new ModHalfYearConvention();
                return true;
            }
            else if (string.Compare(progId, "SageFASCalcEngine.MidPeriodConvention") == 0)
            {
                pVal = new MidPeriodConvention();
                return true;
            }
            else if (string.Compare(progId, "SageFASCalcEngine.MidMonthConvention") == 0)
            {
                pVal = new MidMonthConvention();
                return true;
            }
            else if (string.Compare(progId, "SageFASCalcEngine.FullPeriodConvention") == 0)
            {
                pVal = new FullPeriodConvention();
                return true;
            }
            else if (string.Compare(progId, "SageFASCalcEngine.FullMonthConvention") == 0)
            {
                pVal = new FullMonthConvention();
                return true;
            }
            else if (string.Compare(progId, "SageFASCalcEngine.NextPeriodConvention") == 0)
            {
                pVal = new NextPeriodConvention();
                return true;
            }
            else if (string.Compare(progId, "SageFASCalcEngine.ModMidPeriodConvention") == 0)
            {
                pVal = new ModMidPeriodConvention();

                return true;
            }
            else if (string.Compare(progId, "SageFASCalcEngine.MidQuarterConvention") == 0)
            {
                pVal = new MidQuarterConvention();

                return true;
            }
            return false;
        }

        private bool CalcDeemDates(IBAAvgConvention pObjAvgCon, IBACalendar pObjCalendar,
                            DateTime dtDate, double dblDeprLife, out DateTime dtDeemStartDate, out DateTime dtDeemEndDate)
        {
            IBAFiscalYear pObjIFY;
            dtDeemStartDate = DateTime.MinValue;
            dtDeemEndDate = DateTime.MinValue;

            if (!(pObjAvgCon.Initialize(pObjCalendar, dtDate, dblDeprLife)))
                return false;

            dtDeemStartDate = pObjAvgCon.DeemedStartDate;
            dtDeemEndDate = pObjAvgCon.DeemedEndDate;

            return true;
        }

        public double CalculateYearElapsed(IBADeprScheduleItem pObjSch,
                                                    IBAAvgConvention pObjAvgConvetion,
                                                    IBACalendar pObjCalendar,
                                                    DateTime dtDate)
        {
            IBAFiscalYear pObjIFY;
            DateTime dtDSDate;
            DateTime dtDEDate;
            DateTime dtTmp;
            double dFYFactor;
            double dYFactor;
            short iRWght;
            short iTWght;
            double dLife = 0;
            short iFYStartNum;
            short iFYEndNum;
            int i;
            double dYearElapsed = 0;
            bool hr;

            //
            // First adjust the date to the beginning of a fiscal year.
            //
            if (!(hr = pObjCalendar.GetFiscalYear(dtDate, out pObjIFY)))
                return 0;

            dtDate = pObjIFY.YRStartDate;
            pObjIFY = null;

            //
            // Now get the data we need to compute the years elapsed.
            //

            // KENT start fix MS MC-00037
            bool b = pObjSch.UseACEHandling;
            if (b)
            {
                dLife = pObjSch.ADSLife;
            }
            if (!b || dLife == 0)
            {
                dLife = pObjSch.DeprLife;
            }
            // KENT end

            dtTmp = pObjSch.PlacedInServiceDate;
            if (!(hr = pObjAvgConvetion.Initialize(pObjCalendar, dtTmp, dLife)))
                return 0;

            dtDSDate = pObjAvgConvetion.DeemedStartDate;
            dtDEDate = pObjAvgConvetion.DeemedEndDate;
            pObjCalendar.GetFiscalYearNum(dtDSDate, out iFYStartNum);
            pObjCalendar.GetFiscalYearNum(dtDate, out iFYEndNum);

            for (i = iFYStartNum; i < iFYEndNum; i++)
            {
                pObjIFY = null;
                if (!(hr = pObjCalendar.GetFiscalYearByNum((short)i, out pObjIFY)))
                {
                    return 0;
                }
                dtTmp = pObjIFY.YREndDate;
                if (dtTmp <= dtDate)
                {
                    dtTmp = pObjIFY.YRStartDate;

                    if (dtDSDate >= dtTmp)
                    {
                        if (!(hr = pObjAvgConvetion.GetFirstYearFactor(dtDSDate, out dFYFactor)))
                        {
                            return 0;
                        }

                        dYearElapsed = dFYFactor /* * dYFactor*/;
                    }
                    else
                    {
                        if (!(hr = pObjIFY.GetFiscalYearFraction(out dYFactor)))
                        {
                            return 0;
                        }

                        dYearElapsed = dYearElapsed + dYFactor;
                    }
                }
                else
                {
                    dtTmp = pObjIFY.YRStartDate;
                    if (dtDSDate >= dtTmp)
                    {
                        if (!(hr = pObjAvgConvetion.GetFirstYearFactor(dtDSDate, out dFYFactor)) ||
                            !(hr = pObjIFY.GetRemainingPeriodWeights(dtDate, out iRWght)) ||
                            !(hr = pObjIFY.GetTotalFiscalYearPeriodWeights(out iTWght)))
                        {
                            return 0;
                        }

                        dYearElapsed = dFYFactor - (double)(iRWght) / iTWght;
                    }
                    else if (dtDate >= dtDEDate)
                    {
                        if (!(hr = pObjAvgConvetion.GetLastYearFactor(dLife - dYearElapsed, dtDEDate, out dFYFactor)) ||
                            !(hr = pObjIFY.GetFiscalYearFraction(out dYFactor)))
                        {
                            return 0;
                        }

                        dYearElapsed = dYearElapsed + dFYFactor / dYFactor;

                    }
                    else
                    {
                        dtTmp = pObjIFY.YRStartDate;
                        if (!(hr = pObjIFY.GetPeriodWeights(dtTmp, dtDate, out iRWght)) ||
                            !(hr = pObjIFY.GetTotalFiscalYearPeriodWeights(out iTWght)))
                        {
                            return 0;
                        }

                        dYearElapsed = dYearElapsed + (double)(iRWght) / iTWght;
                    }
                    return dYearElapsed;
                }
            }
            return dYearElapsed;
        }

        bool LoadLuxuryTable(LUXURYLIMIT_TYPE eLuxuryFlag, DateTime dtDate)
        {
            bool bRet;

            if (m_pObjICalcLookUp == null)
                return false;

            m_pObjICalcLookUp.GetLuxuryLimits(dtDate, eLuxuryFlag,
                  ref m_dLimitArray[0], ref m_dLimitArray[1], ref m_dLimitArray[2], ref m_dLimitArray[3], out bRet);

            if (bRet)
                return true;
            else
                return false;

        }

        double GetFYLimit(IBAFiscalYear pObjIFY, int iYearNum, IBADeprScheduleItem pObjSch)
        {
            double dAutoLimit = 0.0;
            double dFraction = 0.0;

            if (iYearNum <= 4)
            {
                dAutoLimit = m_dLimitArray[iYearNum - 1];
            }
            else
            {
                dAutoLimit = m_dLimitArray[3];
            }

            if (dAutoLimit > 0.0)
            {
                pObjIFY.GetFiscalYearFraction(out dFraction);
                dAutoLimit = Currency.FormatCurrency(dAutoLimit * dFraction);


                if (iYearNum == 1)
                {
                    double s179;

                    dAutoLimit += m_dAddl911Limit;    // add the additional amount for 168(k) usage

                    s179 = pObjSch.Section179;
                    dAutoLimit -= s179;
                    if (dAutoLimit < 0)
                        dAutoLimit = 0;
                }
            }

            return dAutoLimit;
        }

        double CheckLimits(IBAFiscalYear pObjIFY, int iYearNum, double dDeprAmt, double dBusUseDeprAmt, double busUsePct, IBADeprScheduleItem pObjSch, ref double dPersUseAmt, ref double dAdjAmount)
        {
            double dAutoLimit;
            double dBUAutoLimit;
            double dFraction;
            double dRet;
            bool hasLimit;

            dRet = dBusUseDeprAmt;
            dPersUseAmt = 0;
            if (iYearNum <= 4)
            {
                dAutoLimit = m_dLimitArray[iYearNum - 1];
            }
            else
            {
                dAutoLimit = m_dLimitArray[3];
            }
            pObjIFY.GetFiscalYearFraction(out dFraction);
            dAutoLimit = dAutoLimit * dFraction;
            hasLimit = (dAutoLimit > 0);
            if (hasLimit && iYearNum == 1)
            {
                dAutoLimit += m_dAddl911Limit;    // add the additional amount for 168(k) usage
                short bonus911Percent = pObjSch.Bonus911Percent;
                if (bonus911Percent > 0)
                {
                    double bonus911Amt = pObjSch.Bonus911Amount;
                    if (m_stored168KAmt != 0 && m_stored168KAmt < bonus911Amt)
                        dAutoLimit = m_stored168KAmt;
                }
            }

            dBUAutoLimit = Currency.FormatCurrency(dAutoLimit * busUsePct); //apply business use on the entire limit

            if (hasLimit && iYearNum == 1)
            {
                double s179;

                s179 = pObjSch.Section179;
                dAutoLimit -= s179;
                if (dAutoLimit < 0)
                    dAutoLimit = 0;
                dBUAutoLimit -= s179;
                if (dBUAutoLimit < 0)
                    dBUAutoLimit = 0;
            }

            if (hasLimit)
            {
                if ((dBusUseDeprAmt /*+ *dAdjAmount*/) > dBUAutoLimit)
                {
                    m_bLimitsApplied = true;
                    m_sLALForYear = "l";
                    dRet = dBUAutoLimit;
                    /*			*dAdjAmount = dBUAutoLimit - dBusUseDeprAmt;
                                if (*dAdjAmount < 0)
                                    *dAdjAmount = 0;*/
                    dPersUseAmt = Currency.FormatCurrency(dAutoLimit - dBUAutoLimit);
                }
                else
                {
                    dPersUseAmt = Currency.FormatCurrency(dDeprAmt - dBusUseDeprAmt);
                    m_sLALForYear = "";
                }
            }
            else
            {
                dPersUseAmt = CurrencyHelper.FormatCurrency(dDeprAmt - dBusUseDeprAmt);
                m_sLALForYear = "";
            }

            return dRet;
        }

        double CalcUsageFraction(IBADeprScheduleItem pObjSch, double dBasis, DateTime dtDate)
        {
            double dBUPct = 0;
            double dIUPct = 0;
            double dBasisDiff = 0;
            double dAdjCost = 0;

            if (Math.Abs(dBasis) < 0.0001)
                return 0;

            pObjSch.GetBusinessUse(dtDate, ref dBUPct, ref dIUPct);
            dBUPct += dIUPct;
            dAdjCost = pObjSch.AdjustedCost;
            dBasisDiff = dAdjCost * (1 - dBUPct);

            if (dBUPct < 1.0)
                m_sBUPForYear = "b";
            else
                m_sBUPForYear = "";

            return (dBasis - dBasisDiff) / dBasis;
        }

        double calculateBonus168KAmount(IBADeprScheduleItem pObjSch)
        {
            if (pObjSch.Bonus911Percent <= 0)
                return 0;

            double d168KAmt = pObjSch.CalculateBonus911Amount;
            pObjSch.Bonus911Amount = d168KAmt;
            pObjSch.Stored168KAmount = d168KAmt;

            double s179 = pObjSch.Section179;
            double stored911Amt = pObjSch.Stored168KAmount;
            double bonus911Amt = pObjSch.Bonus911Amount;
            pObjSch.Section179 = s179 + (stored911Amt != 0 ? stored911Amt : bonus911Amt); // and add it to section 179          
            return d168KAmt;
        }

        public bool ComputeITCRecap(IBADeprScheduleItem schedule, DateTime RunDate, out double TablePct, out double Recap, out double AddBack)
        {
            bpblBookTypeEnum type = schedule.BookType;
            double ITCAmount = schedule.ITCAmount;
            IBACalendar calendar = schedule.Calendar;
            double BaseITCFactor = 0;
            double ITCFactor = 0; ;
            CalcHelper helper = new CalcHelper();
            IBACalendarManager CalendarManager = (IBACalendarManager)calendar;
            CalendarManager.AddCycleEntry(new DateTime(1920, 1, 1), ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_MONTHLY, 12, ECALENDARCYCLE_DATEOFWEEK.DATEOFWEEK_SUNDAY, ECALENDARCYCLE_YEARENDELECTION.YEARENDELECTION_LASTWEEKDAY, ECALENDARCYCLE_PDCOUNTING.PDCOUNT_BACKWARD);

            TablePct = 0;
            Recap = 0;
            AddBack = 0;

            return helper.ComputeITCRecap(schedule, calendar, RunDate, ITCAmount, out BaseITCFactor, out ITCFactor, out TablePct, out Recap, out AddBack);
        }

        public bool ComputeFullCostBasis(IBADeprScheduleItem schedule, bool ForMidQtr, out bool InLastQtr, out double Basis)
        {
            IBACalendar calendar = schedule.Calendar;
            CalcHelper helper = new CalcHelper();
            IBACalendarManager CalendarManager = (IBACalendarManager)calendar;
            CalendarManager.AddCycleEntry(new DateTime(1920, 1, 1), ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_MONTHLY, 12, ECALENDARCYCLE_DATEOFWEEK.DATEOFWEEK_SUNDAY, ECALENDARCYCLE_YEARENDELECTION.YEARENDELECTION_LASTWEEKDAY, ECALENDARCYCLE_PDCOUNTING.PDCOUNT_BACKWARD);

            calculateBonus168KAmount(schedule);
            return helper.ComputeFullCostBasis(schedule, calendar, false, ForMidQtr, out InLastQtr, out Basis);
        }

        private void setPeriodDetailDeprInfo(List<IBAPeriodDeprItem> pInList, out List<IBAPeriodDetailDeprInfo> pOutList)
        {
            IBAPeriodDetailDeprInfo obj;
            PeriodDetailDeprInfo objInfo;

            pOutList = new List<IBAPeriodDetailDeprInfo>();
            foreach (IBAPeriodDeprItem pdi in pInList)
            {

                    objInfo = new PeriodDetailDeprInfo();
                    obj = (IBAPeriodDetailDeprInfo)objInfo;

                    obj.FiscalYearStartDate = pdi.StartDate;
                    obj.FiscalYearEndDate = pdi.EndDate;
                    obj.FiscalYearBeginAccum = pdi.BeginYearAccum;
                    obj.FiscalYearDeprAmount = pdi.DeprAmount;
                    obj.FiscalYearEndAccum = (decimal)pdi.EndPeriodAccum;
                    obj.CalcFlags = pdi.CalcFlags;

                    obj.PeriodStartDate = pdi.StartDate;
                    obj.PeriodEndDate = pdi.EndDate;
                    obj.PeriodBeginAccum = 0;
                    obj.PeriodDeprAmount = pdi.DeprAmount;
                    obj.PeriodEndAccum = (decimal)pdi.EndPeriodAccum;
                    obj.CalcFlags = pdi.CalcFlags;

                    pOutList.Add(objInfo);

            }
        }

        private void setPeriodDetailDeprInfo(IBACalendar pCalendar, List<IBAPeriodDeprItem > pInList, out List<IBAPeriodDetailDeprInfo> pOutList)
        {
            IBAPeriodDetailDeprInfo obj;
            PeriodDetailDeprInfo objInfo;
            IBAFiscalYear objFY;
            IBACalcPeriod objPeriod;
            DateTime dtStartDate = DateTime.MinValue;
            DateTime dtEndDate = DateTime.MinValue;
            short iWeight = 12;
            short iPeriodNum = 0;
            decimal monthlyAmount = 0;

            pOutList = new List<IBAPeriodDetailDeprInfo>();
            foreach (IBAPeriodDeprItem pdi in pInList)
            {
                pCalendar.GetFiscalYear(pdi.StartDate, out objFY);
                if (objFY.YRStartDate <= m_deemedStartDate && m_deemedStartDate <= objFY.YREndDate)
                {
                    dtStartDate = m_deemedStartDate;
                }
                else
                {
                    dtStartDate = pdi.StartDate;
                }
                if (objFY.YRStartDate < m_deemedEndDate && m_deemedEndDate <= objFY.YREndDate)
                {
                    dtEndDate = m_deemedEndDate;
                }
                else
                {
                    dtEndDate = objFY.YREndDate;
                }

                objFY.GetPeriodWeights(dtStartDate, dtEndDate, out iWeight);
                iPeriodNum = 0;
                do
                {
                    objInfo = new PeriodDetailDeprInfo();
                    obj = (IBAPeriodDetailDeprInfo)objInfo;

                    obj.FiscalYearStartDate = pdi.StartDate;
                    obj.FiscalYearEndDate = pdi.EndDate;
                    obj.FiscalYearBeginAccum = pdi.BeginYearAccum;
                    obj.FiscalYearDeprAmount = pdi.DeprAmount;
                    obj.FiscalYearEndAccum = (decimal)pdi.EndPeriodAccum;
                    obj.CalcFlags = pdi.CalcFlags;

                    objFY.GetPeriod(dtStartDate, out objPeriod);

                    if (iWeight > 0)
                        monthlyAmount = pdi.DeprAmount / iWeight;
                    else
                        monthlyAmount = 0;

                    obj.PeriodStartDate = objPeriod.PeriodStart;
                    obj.PeriodEndDate = objPeriod.PeriodEnd;


                    obj.PeriodBeginAccum = iPeriodNum * monthlyAmount;
                    obj.PeriodDeprAmount = monthlyAmount;
                    obj.PeriodEndAccum = obj.PeriodBeginAccum + obj.PeriodDeprAmount;
                    obj.CalcFlags = pdi.CalcFlags;

                    pOutList.Add(objInfo);

                    iPeriodNum++;
                    dtStartDate = objPeriod.PeriodEnd.AddDays(1);
                }
                while (dtStartDate < pdi.EndDate && pdi.EndDate < new DateTime(3000,1,1));
            }
        }
    }

    public static class CurrencyHelper
    {
        public static void DoubleToCurrency(double inValue, out decimal outValue)
        {
            outValue = (decimal)inValue;
        }

        public static double FormatCurrency(double value)
        {
            return value;
        }
        public static double CurrencyToDouble(decimal currency)
        {
            return (double)currency;
        }
    }
}