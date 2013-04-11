using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SFACalendar;

namespace SFACalcEngine
{
    public class FiscalYearCalcer
    {
        private DateTime m_dtPISDate;
        private DateTime m_dtDispDate;
        private DateTime m_dtCalcDate;
        private DateTime m_dtDeemedStartDate;
        private DateTime m_dtDeemedEndDate;
        private IBAFiscalYear m_pObjIFY;
        private IBAAvgConvention m_pObjConvention;
        private IBADeprMethod m_pObjDeprMethod;
        private IBADeprScheduleItem m_pObjSchedule;
        private LUXURYLIMIT_TYPE m_eLFlag;
        private string m_sConvention;

        public double m_StAdjustAmount;
        public DateTime m_StAdjCalcStart;

        //FOR RV HARD FIX
        public bool m_bFixRVdata;
        public double m_dAddToPriorAccum;
        public double m_dAddToYearElapsed;

        //luxury auto limit changes
        public double m_dAutoLimit;
        public double AutoPostUsageDeduction;
        public double PostUsageDeduction;
        public bool Fix168KAuto;

        public FiscalYearCalcer()
        {
            m_dtPISDate = DateTime.MinValue;
            m_dtDispDate = DateTime.MinValue;
            m_dtDeemedStartDate = DateTime.MinValue;
            m_dtDeemedEndDate = DateTime.MinValue;
            m_eLFlag = LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_NOTAPPLY;
            m_StAdjustAmount = 0.0;

            //for RV hard fix
            m_bFixRVdata = false;
            m_dAddToPriorAccum = 0.0;
            m_dAddToYearElapsed = 0.0;

            m_dAutoLimit = 0.0;
            //	m_bBuildAllPDItems = false;
            AutoPostUsageDeduction = 0.0;
            PostUsageDeduction = 0.0;
            Fix168KAuto = false;
        }

        public IBAFiscalYear FiscalYearInfo
        {
            get { return m_pObjIFY; }
            set { m_pObjIFY = value; }
        }

        public IBAAvgConvention AvgConvertion
        {
            get { return m_pObjConvention; }
            set { m_pObjConvention = value; }
        }

        public IBADeprMethod DeprMethod
        {
            get { return m_pObjDeprMethod; }
            set { m_pObjDeprMethod = value; }
        }

        public IBADeprScheduleItem DeprScheduleItem
        {
            get { return m_pObjSchedule; }
            set { m_pObjSchedule = value; }
        }

        public DateTime PISDate
        {
            get { return m_dtPISDate; }
            set { m_dtPISDate = value; }
        }

        public DateTime DispDate
        {
            get { return m_dtDispDate; }
            set { m_dtDispDate = value; }
        }

        public DateTime DeemedStartDate
        {
            get { return m_dtDeemedStartDate; }
            set { m_dtDeemedStartDate = value; }
        }

        public DateTime DeemedEndDate
        {
            get { return m_dtDeemedEndDate; }
            set { m_dtDeemedEndDate = value; }
        }

        public LUXURYLIMIT_TYPE LUXURYLIMITTYPEflag
        {
            set { m_eLFlag = value; }
        }

        public DateTime CalcDate
        {
            set { m_dtCalcDate = value; }
        }

        public string Convention
        {
            set { m_sConvention = value; }
        }

        public double Calculate(out double DeferredAmount, bool bPostLife, double dTotalDeferredAmount)
        {
            double  dCalculate;
            double  dAnnualDepr;
            double  dRemainingDeprAmt;
            bool    bRet;
            bool    bDefersDepr;
            bool    hr;

            DeferredAmount = 0;

            bRet = m_pObjDeprMethod.IsFiscalYearBased;
            dRemainingDeprAmt = m_pObjDeprMethod.RemainingDeprAmt;

            IBADeprTableSupport tableSupp = (m_pObjDeprMethod) as IBADeprTableSupport;
            if (tableSupp != null)
            {
                bDefersDepr = tableSupp.DeferShortYearAmount;
            }
            else
                bDefersDepr = false;

            if (bRet)
            {
                //if fiscal year based, then set the fraction to the depr method
                //the CalculateAnnualDepr will use this fraction to calc the year
                //depr amt
                m_pObjDeprMethod.FiscalYearFraction = GetFraction();
                dCalculate = m_pObjDeprMethod.CalculateAnnualDepr();
            }
            else
            {
                //Handle RV hard fix
                if (m_bFixRVdata == true)
                {
                    double dAccum;
                    double dElapsed;

                    dAccum = m_pObjDeprMethod.PriorAccum;
                    dElapsed = m_pObjDeprMethod.YearElapsed;
                    m_pObjDeprMethod.PriorAccum = dAccum + m_dAddToPriorAccum;
                    m_pObjDeprMethod.YearElapsed = dElapsed + m_dAddToYearElapsed;
                    dAnnualDepr = m_pObjDeprMethod.CalculateAnnualDepr();
                    m_pObjDeprMethod.PriorAccum = dAccum;
                    m_pObjDeprMethod.YearElapsed = dElapsed;

                    m_bFixRVdata = false;
                }
                else
                {
                    //if not then, modify the annual depr amt by the fraction
                    dAnnualDepr = m_pObjDeprMethod.CalculateAnnualDepr();
                }

                // START KENT fix MS MC -00047
                // after life and luxury auto, because the auto limit apply the method may calc dAnnualDepr = 0
                // but the depr calc need to continue
                if ((m_eLFlag == LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_LUXURYCAR || m_eLFlag == LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_ELECTRICCAR) &&
                     (int)dAnnualDepr == 0 && bPostLife)
                    dAnnualDepr = dRemainingDeprAmt;
                // END KENT 

                dCalculate = dAnnualDepr;
                if (bDefersDepr)
                {
                    if ( bPostLife && dRemainingDeprAmt > dTotalDeferredAmount )
                        dRemainingDeprAmt = dTotalDeferredAmount;

                    if ( dCalculate > dRemainingDeprAmt || (IsLastYear() && Math.Abs(dCalculate - dRemainingDeprAmt) < 1.0) )
                        dCalculate = dRemainingDeprAmt;

                    if (dAnnualDepr > dRemainingDeprAmt || (IsLastYear() && Math.Abs(dAnnualDepr - dRemainingDeprAmt) < 1.0))
                        dAnnualDepr = dRemainingDeprAmt;

                    dCalculate = dCalculate * GetFraction();
                    DeferredAmount = CurrencyHelper.FormatCurrency(CurrencyHelper.FormatCurrency(dAnnualDepr) - CurrencyHelper.FormatCurrency(dCalculate));
                }
                else
                {
                    //Handle RV fix
                    DateTime dtPIS = PISDate;
                    DateTime dtYEnd = m_pObjIFY.YREndDate;
                    string deprMethod = m_pObjDeprMethod.BaseShortName;
                    ECALENDARCYCLE_CYCLETYPE eCType = m_pObjIFY.CycleType;

                    if (IsFirstYear() && IsLastYear() &&
                       (deprMethod == "RV") &&
                       (eCType != ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_MONTHLY && eCType != ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_CUSTOM) &&
                       (dtYEnd == m_dtDeemedEndDate))
                    {
                        dCalculate = m_pObjDeprMethod.Basis;
                    }
                    else
                    {
                        dCalculate = dAnnualDepr * GetFraction();
                    }
                }
            }
            //take care the rounding
            dCalculate = Currency.FormatCurrency(dCalculate); // was 2 places
            return dCalculate;
        }

        public double CalculateDispDepr(IBADeprTableSupport tableSupp, double DeferredAmount, bool DefersDepr, bool InPostLife, double dMaxDeprAllowed, LUXURYLIMIT_TYPE eLFlag,
                                            double dAdjustAmount,
                                            DateTime dtAdjPeriodStart,
                                            double dStartingPersonalAmount,
                                            IBAPeriodDeprItem pObjStartPoint)
        {
            double dCalculateDispDepr = 0.0;
            double dLife;
            double dYearElapsed;
            double dFactor;
            bool   bFYBased;
            DISPOSALOVERRIDETYPE eDot;
            //ATLASSERT(m_pObjDeprMethod != null);
            //ATLASSERT(m_pObjConvention != null);
            //ATLASSERT(m_pObjSchedule != null);

            if ( m_pObjDeprMethod == null || m_pObjConvention == null || m_pObjSchedule == null )
                return 0;

            dLife = m_pObjDeprMethod.Life;
            dYearElapsed = m_pObjDeprMethod.YearElapsed;
            //    m_pObjConvention.put_RemainingLife (dLife - dYearElapsed);
            bFYBased = m_pObjDeprMethod.IsFiscalYearBased;
            //
            // First check the schedule item to see if it needs to override the standard handling
            // for disposals.  This is needed in the AMT and ACE schedules. 
            //
            eDot = DISPOSALOVERRIDETYPE.disposaloverride_Normal;

            ////
            //// Then if the schedule did not need special handling, check the depr method.
            ////
            if ( eDot == DISPOSALOVERRIDETYPE.disposaloverride_Normal )
            {
                eDot = m_pObjDeprMethod.DisposalOverride;
            }
            //
            // If both say use normal, then get the information from the averaging convention and
            // use it.
            //
            if ( eDot == DISPOSALOVERRIDETYPE.disposaloverride_Normal )
            {
                if( bFYBased )
                {
                    //if fiscal year based, then set the fraction to the depr method
                    //the CalculateAnnualDepr will use this fraction to calc the year
                    //depr amt
                    m_pObjConvention.GetDisposalYearFactor(dLife - dYearElapsed, m_dtDispDate, out dFactor);
                    m_pObjDeprMethod.FiscalYearFraction = dFactor;

                    //
                    // We need to handle the post life calcs here.  Otherwise we will get zero 
                    // depreciation for most depr methods.
                    //
                    if ( InPostLife )
                    {
                        if ( tableSupp != null )
                        {
                            if ( DefersDepr )
                            {
                                dCalculateDispDepr = m_pObjDeprMethod.CalculateAnnualDepr();
                                if ( DeferredAmount > 0.00004 || eLFlag == LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_NOTAPPLY )
                                {
                                    if ( dCalculateDispDepr > DeferredAmount )
                                    {
                                        dCalculateDispDepr = DeferredAmount;
                                    }
                                }
                            }
                            else
                            {
                                dCalculateDispDepr = -1.0;
                            }
                        }
                        else
                        {
                            if ( eLFlag != LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_NOTAPPLY )
                                //dCalculateDispDepr = -1.0;
                                dCalculateDispDepr = m_pObjDeprMethod.RemainingDeprAmt;
                            else
                                dCalculateDispDepr = -1.0;
                        }

                    }
                    else
                    {
                        if ( m_dtDispDate > m_dtDeemedEndDate ) 
                        {
                            dCalculateDispDepr = -1.0;
                        }
                        else
                        {
                            dCalculateDispDepr = m_pObjDeprMethod.CalculateAnnualDepr();
                        }
                    }
                }
                else
                {
                    //
                    // We need to handle the post life calcs here.  Otherwise we will get zero 
                    // depreciation for most depr methods.
                    //
                    if ( InPostLife )
                    {
                        if ( tableSupp != null )
                        {
                            if ( DefersDepr )
                            {
                                if ( DeferredAmount > 0.00004 )   //SAI:;;;|| eLFlag == LUXURYLIMITTYPE_NOTAPPLY )
                                {
                                    dCalculateDispDepr = m_pObjDeprMethod.CalculateAnnualDepr();
                                    if ( dCalculateDispDepr > DeferredAmount )
                                    {
                                        dCalculateDispDepr = DeferredAmount;
                                    }
                                }
                                else
                                {
                                    dCalculateDispDepr = -1;
                                }
                                //
                                // Make the disposal portion of the factor not be used in post
                                // life computations.
                                //
                                // IF NANCY OR MARY DECIDE THAT ACRS SHOULD FOLLOW THE SAME
                                // HANDLING AS MACRS FOR POST LIFE AUTO DISPOSAL, UNCOMMENT
                                // THE NEXT TWO LINES.
                                //
        //						if ( !(m_pObjIFY.GetFiscalYearFraction(&dFactor)) )
        //							dFactor = 1.0;
                            }
                            else
                            {
                                //
                                //m_pObjDeprMethod.RemainingDeprAmt(&dCalculateDispDepr);
                                dCalculateDispDepr = -1;

                            }
                        }
                        else
                        {
                            if ( eLFlag != LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_NOTAPPLY )
                                dCalculateDispDepr = m_pObjDeprMethod.RemainingDeprAmt;
                                //dCalculateDispDepr = -1;
                            else
                                dCalculateDispDepr = -1;
                        }

                    }
                    else
                    {
                        if ( m_dtDispDate > m_dtDeemedEndDate ) 
                        {
                            dCalculateDispDepr = -1.0;
                        }
                        else 
                        {

                            bool bUseFirstYearFactor;
                            bool hr;
                            double dFraction;
                            double dAnnualDepr;
                            string deprMethod;

                            deprMethod = m_pObjDeprMethod.BaseShortName;

                            //if not FY based and during the life then, modify the annual depr amt by the fraction
                            m_pObjConvention.GetDisposalYearFactor(dLife - dYearElapsed, m_dtDispDate, out dFactor);

                            if( deprMethod.Contains("~~custom~~") ) //&& Year(m_dtDeemedStartDate) == Year(m_dtDispDate))
                            {
                                IBACalendar pObjCalendar;
                                IBAFiscalYear pObjFY;
                                DateTime dtEnd;
                                pObjCalendar = m_pObjSchedule.Calendar;
                                pObjCalendar.GetFiscalYear(m_dtDeemedStartDate, out pObjFY);
                                dtEnd = pObjFY.YREndDate;

                                if (m_dtDeemedStartDate <= m_dtDispDate && m_dtDispDate <= dtEnd)
                                {
                                    double dFactor1;
                                    m_pObjConvention.GetFirstYearFactor(m_dtDeemedStartDate, out dFactor1);
                                    dFactor = dFactor / dFactor1;
                                }
                            }

                            dAnnualDepr = m_pObjDeprMethod.CalculateAnnualDepr();

                            bUseFirstYearFactor = m_pObjDeprMethod.UseFirstYearFactor;

                            if ( !bUseFirstYearFactor && m_dtDeemedEndDate >= m_dtDispDate )
                            {
                                if( IsLastYear() )
                                {
                                    //the last year
                                    dLife = m_pObjDeprMethod.Life;
                                    dYearElapsed = m_pObjDeprMethod.YearElapsed;
                                    dFraction = dLife - dYearElapsed;
                                    if ( dFraction != 0 )
                                        dFactor = dFactor / dFraction;
                                }
                            }
                            else if ( !bUseFirstYearFactor )
                                dFactor = 1;

                            dCalculateDispDepr = dAnnualDepr * dFactor;

                            if ( dCalculateDispDepr > dMaxDeprAllowed )
                                dCalculateDispDepr = dMaxDeprAllowed;
                        }

                    }
                }
            }
            // If either one called for full year of depr in disposal year, then do it.
            else if ( eDot == DISPOSALOVERRIDETYPE.disposaloverride_FullYear )
            {
                dCalculateDispDepr = -1.0;
            }
            // Otherwise we get no depr in disposal year.
            else
            {
                dCalculateDispDepr = 0;
            }

            if(m_dtDispDate < m_dtDeemedStartDate )
            {
                dCalculateDispDepr = 0;
            }

            if (dCalculateDispDepr >= 0.0)
            {
                DateTime dtFYStartDate;
                //
                // Now take care of the year end immediate adjustments
                //
                    dtFYStartDate = m_pObjIFY.YRStartDate;

                if (dAdjustAmount > 0.0)
                {
                    if (dtAdjPeriodStart == dtFYStartDate)
                        m_StAdjustAmount = dAdjustAmount;
                }
                else if (m_StAdjCalcStart != dtFYStartDate)
                {
                    m_StAdjustAmount = 0.0;
                }

                dCalculateDispDepr += m_StAdjustAmount;

                //
                // Now make sure that the depreciation does not exceed the remaining depr.  If it 
                // does, limit it.
                //

                //take care of the rounding
                dCalculateDispDepr = CurrencyHelper.FormatCurrency(dCalculateDispDepr);
            }
          
            return dCalculateDispDepr;
        }

        //public double				CalculateDispDeprForBeginInfoYear(List<IBAPeriodDeprItem> pColPItemList, IBAPeriodDeprItem *pObjStartPoint);
        public double GetFraction()
        {
            return GetFraction(false);
        }

        public double GetFraction(bool forceFirstYearFactor)
        {
            double dFraction = 0.0;
            double dLife;
            double dYearElapsed;
            bool   bUseFirstYearFactor;

            bUseFirstYearFactor = m_pObjDeprMethod.UseFirstYearFactor;

            if (bUseFirstYearFactor || forceFirstYearFactor)
            {
                if (IsFirstYear())
                {
                    //the first year
                    m_pObjConvention.GetFirstYearFactor(m_dtDeemedStartDate, out dFraction);
                    return dFraction;
                }

                // KENT the && m_eLFlag == LUXURYLIMITTYPE_NOTAPPLY is added to fix NFaus-00132
                // even it is a last year but for lux auto, should use a normal year factor
                if ((IsLastYear() && m_eLFlag == LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_NOTAPPLY) ||
                    (IsLastYear() && Fix168KAuto))
                {
                    //the last year
                    dLife = m_pObjDeprMethod.Life;
                    dYearElapsed = m_pObjDeprMethod.YearElapsed;
                    dFraction = dLife - dYearElapsed;
                    return dFraction;
                }
            }
            //normal year and short year
            m_pObjIFY.GetFiscalYearFraction(out dFraction);
            return dFraction;
        }

        public bool IsDispYear()
        {
            DateTime dtDate;

            dtDate = m_pObjIFY.YREndDate;
            if (m_dtDispDate != DateTime.MinValue && m_dtDispDate <= dtDate)
                return true;
            else
                return false;
        }

        public bool IsFirstYear()
        {
            DateTime dtDate;

            dtDate = m_pObjIFY.YRStartDate;
            if (PISDate >= dtDate)
                return true;
            else
                return false;
        }

        public bool IsLastYear()
        {
            DateTime dtDate;
            DateTime dtSDate;

            dtDate = m_pObjIFY.YREndDate;
            dtSDate = m_pObjIFY.YRStartDate;

            if (m_dtDeemedEndDate <= dtDate && m_dtDeemedEndDate >= dtSDate)
                return true;
            else
                return false;
        }

        public bool IsInBegInfoYear(DateTime calcDate, IBAPeriodDeprItem pObjStartPoint)
        {
            IBACalendar pObjCalendar;
            IBAFiscalYear pObjFY;
            DateTime adjStartDate = DateTime.MinValue;
            DateTime adjEndDate = DateTime.MinValue;
            DateTime dtStart;
            DateTime dtEnd;
            bool hr;

            if (pObjStartPoint != null)
            {
                adjStartDate = pObjStartPoint.StartDate;
                adjEndDate = pObjStartPoint.EndDate;
            }

            if (adjEndDate == adjStartDate)
            {
                pObjCalendar = m_pObjSchedule.Calendar;
                if (!(hr = pObjCalendar.GetFiscalYear(adjEndDate, out pObjFY)))
                {
                    return false;
                }

                dtStart = pObjFY.YRStartDate;
                dtEnd = pObjFY.YREndDate;
                if (dtStart <= calcDate && calcDate <= dtEnd)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool UpdatePDItemlistForDisp(IBADeprScheduleItem pObjSch,
                                    DateTime dtDispDate,
                                    ref double dblDispAmt,
                                    DateTime dtDeemedEndDate,
                                    string sCalcFlags,
                                    List<IBAPeriodDeprItem> pColList,
                                    double dAdjustAmount,
                                    DateTime dtAdjPeriodStart,
                                    double dStartingPersonalAmount,
                                    IBAPeriodDeprItem pObjStartPoint)
        {
            DeprAllocator	    pObjDA = new DeprAllocator();
            IBAPeriodDeprItem	pObjPDItem;
            IBAPeriodDeprItem	pObjItem;
	        IBACalendar		    pObjCalendar;
	        IBAFiscalYear		pObjIFY;
            IBACalcPeriod		pObjPeriod;
	        bool			    hr;
	        DateTime            dtDisposalYearBegin;
	        DateTime            dtDisposalYearEnd;
	        DateTime            dtDisposalPeriodEnd;
	        DateTime            dtDisposalPeriodStart;
	        DateTime            pisDate;
	        decimal				curTmp1;
	        decimal				curTmp2;
	        decimal				curTmp3;
	        double              dblTmp1;
	        short				iFYNum1;
	        short               iDisposalYearNumber;
	        short               iDisposalWeight;
	        int 				iCount;
	        PeriodDeprItem      pdObj;
	        int                 i;
	        bool				adjApplied = false;
	        DateTime            dtAdjStart = DateTime.MinValue;
	        DateTime            dtAdjEnd = DateTime.MinValue;

		    pisDate = pObjSch.PlacedInServiceDate;
		    pObjDA.PlacedInService = pisDate;
		    pObjDA.DeemedEndDate = dtDeemedEndDate;
		    pObjDA.DisposalDate = dtDispDate;

            pObjCalendar = pObjSch.Calendar;
	        pObjDA.Calendar = pObjCalendar;
	        pObjDA.PeriodDeprItemList = pColList;

	        //
	        // Determine the period start for the disposal date
	        //
	        pObjCalendar.GetFiscalYear(dtDispDate, out pObjIFY);
	        dtDisposalYearBegin = pObjIFY.YRStartDate;
	        dtDisposalYearEnd = pObjIFY.YREndDate;
	        iDisposalYearNumber = pObjIFY.FYNum;
	        pObjIFY.GetPeriod(dtDispDate, out pObjPeriod);
	        dtDisposalPeriodStart = pObjPeriod.PeriodStart;;
	        dtDisposalPeriodEnd = pObjPeriod.PeriodEnd;;
	        iDisposalWeight = pObjPeriod.Weight;

	        if ( dtDisposalYearBegin <= dtAdjPeriodStart && dtDisposalYearEnd >= dtAdjPeriodStart )
	        {
		        IBACalcPeriod pObjAdjPeriod;

		        //
		        // We have an adjustment in this year, therefore determine the start and
		        // end period dates.
		        //
                pObjIFY.GetPeriod(dtAdjPeriodStart, out pObjAdjPeriod);
                dtAdjStart = pObjAdjPeriod.PeriodStart;
                dtAdjEnd = pObjAdjPeriod.PeriodEnd;
	        }
	        //
	        // Filter off the items that start after the disposal start date.
	        //
	        iCount = pColList.Count;

	        if (dblDispAmt >= 0.0)
	        {
		        for (i = iCount - 1; i >= 0; i--)
		        {
			        DateTime dtStart;
			        DateTime dtEnd;

			        pObjItem = null;
                    pObjItem = pColList[i];

                    dtStart = pObjItem.StartDate;
                    dtEnd   = pObjItem.EndDate;

			        if ( dtStart > dtDisposalPeriodStart && i > 0 )
			        {
				        pColList.RemoveAt(i);
				        iCount--;
			        }
			        else if (dtStart == dtDisposalPeriodStart )
			        {
				        //
				        // Get out of the loop.  We need to have the disposal period object at the
				        // end of the list.
				        //
				        break;
			        }
			        else if ( dtStart < dtDisposalPeriodStart )
			        {
				        if ( dtEnd >= dtDisposalPeriodStart )
				        {
					        IBAPeriodDeprItem newItem;
					        IBAPeriodDeprItem junkItem;

					        if (dtEnd >= new DateTime(3000,1,1))
					        {
						        short iNum1, iNum2;

						        pObjCalendar.GetFiscalYearNum(dtStart, out iNum1);
							    pObjCalendar.GetFiscalYearNum(dtDisposalPeriodStart, out iNum2);
							      
						        if ( iNum1 != iNum2 )
						        {
							        IBAPeriodDeprItem item;
							        if (!(hr = pObjItem.Clone(out item)))
                                        return hr;
								    pObjItem.EndDate = dtDisposalYearBegin.AddDays(- 1);
								    item.StartDate = dtDisposalYearBegin;
								    AppendPeriodDeprItem(pColList,item);
								     
							        i = iCount;
							        iCount++;
							        pObjItem = null;
							        pObjItem = item;
						        }
					        }

					        //
					        // It appears that this PD item needs to be split.  Therefore split it.
					        // Then get rid of the current item in the list and add "newItem".  Also
					        // add "junkItem" so that we have the PD information that we need later.
					        //
					        pObjDA.SplitPDItem(pObjItem, dtDisposalPeriodStart,  out newItem, out junkItem);
						    pColList.RemoveAt(i);
						    AppendPeriodDeprItem(pColList, newItem);
						    AppendPeriodDeprItem(pColList, junkItem);
					        iCount++;
				        }
				        break;
			        }
		        }
	        }
	        else
	        {
		        for (i = iCount - 1; i >= 0; i--)
		        {
			        DateTime dtStart;
			        DateTime dtEnd;

 			        pObjItem = null;
                    pObjItem = pColList[i];

                    dtStart = pObjItem.StartDate;
                    dtEnd   = pObjItem.EndDate;
			        if ( dtStart > dtDisposalPeriodStart && i > 0 )
			        {
				        pColList.RemoveAt(i);
				        iCount--;
			        }
			        else if (dtStart == dtDisposalPeriodStart )
			        {
				        // reset end date to disposal period end
				        pObjItem.EndDate = dtDisposalPeriodEnd;
					    pObjItem.TotalPeriodWeights = iDisposalWeight;
				        break;
			        }
			        else if ( dtStart < dtDisposalPeriodStart )
			        {
				        if ( dtEnd >= dtDisposalPeriodStart )
				        {
					        IBAPeriodDeprItem newItem;
					        IBAPeriodDeprItem junkItem;
					        short iNum1, iNum2;

					        pObjItem.EndDate = dtDisposalPeriodEnd;

					        if (dtEnd >= new DateTime(3000,1,1))
					        {
						        pObjCalendar.GetFiscalYearNum(dtStart, out iNum1);
						        pObjCalendar.GetFiscalYearNum(dtDisposalPeriodStart, out iNum2);
						        if ( iNum1 != iNum2 )
						        {
							        IBAPeriodDeprItem item;
							        pObjItem.Clone(out item);
							        pObjItem.EndDate = dtDisposalYearBegin.AddDays(- 1);
							        item.StartDate = dtDisposalYearBegin;
							        AppendPeriodDeprItem(pColList,item);
				
							        i = iCount;
							        iCount++;
							        pObjItem = null;
							        pObjItem = item;
						        }
					        }
					        //
					        // It appears that this PD item needs to be split.  Therefore split it.
					        // Then get rid of the current item in the list and add "newItem".  Also
					        // add "junkItem" so that we have the PD information that we need later.
					        //
					        if (dtDisposalYearBegin != dtDisposalPeriodStart)
					        {
						        pObjDA.SplitPDItem(pObjItem, dtDisposalPeriodStart, out newItem, out junkItem);
						        pColList.RemoveAt(i);
						        AppendPeriodDeprItem(pColList,newItem);
						        junkItem.TotalPeriodWeights = iDisposalWeight;
						        AppendPeriodDeprItem(pColList,junkItem);
 
                                iCount++;
					        }

				        }
				        break;
			        }
		        }
		        if ( iCount <= 0 )
		        {
			        //
			        // No, we did not have a PD item.  PROBLEM!PROBLEM!!PROBLEM!!!
			        //
        		}	
	
		        pObjPDItem = null;
                pObjPDItem  = pColList[iCount-1];

                curTmp1 = pObjPDItem.DeprAmount;
                curTmp2 = (decimal)pObjPDItem.EndDateYTDExpense;
                pObjPDItem.CalcFlags = sCalcFlags;

                if ( CurrencyHelper.CurrencyToDouble(curTmp1) != 0 )
			        dblDispAmt = CurrencyHelper.CurrencyToDouble(curTmp1);
		        else
			        dblDispAmt = CurrencyHelper.CurrencyToDouble(curTmp2);
		
	            goto finishyear;
	        }

	        //
	        // Now we have a list of PD items that ends just before the disposal period.  Create
	        // the new PD item to add to the list.
	        //
	        pObjPDItem = null;
		    PeriodDeprItem obj = new PeriodDeprItem();
            pObjPDItem = (IBAPeriodDeprItem) obj;
		    pObjPDItem.StartDate = dtDisposalPeriodStart;
		    pObjPDItem.EndDate = dtDisposalPeriodEnd;
		    pObjPDItem.FYNum = iDisposalYearNumber;
            pObjPDItem.TotalPeriodWeights = iDisposalWeight;
		    pObjPDItem.CalcFlags = sCalcFlags;

	        //
	        // Make sure that we have PD items to work with.
	        //
	        if ( iCount <= 0 )
	        {
		        //
		        // No, we did not have a PD item.  Therefore create an empty one.
		        //
	        }
	        else
	        {
		        DateTime dtStart;
		        DateTime dtEnd;

		        //
		        // The last PD item in the list contains the information for the disposal period.
		        // Get it and transfer the appropriate calc information into the new PD item.
		        //
		        pObjItem = null;
                pObjItem = pColList[iCount - 1];

                dtStart = pObjItem.StartDate;
                dtEnd   = pObjItem.EndDate;
             
		        if ( dtStart <= dtDisposalPeriodStart && dtEnd >= dtDisposalPeriodEnd )
		        {
			        //
			        // We found the correct period.  Transfer the information.
			        //
			        curTmp1 = pObjItem.BeginYearAccum;
				    pObjPDItem.BeginYearAccum = curTmp1;
				    pObjPDItem.EndDateBeginYearAccum = curTmp1;
				    curTmp1 = pObjItem.BeginYTDExpense;
				    pObjPDItem.BeginYTDExpense = curTmp1;
                    curTmp2 = pObjItem.AdjustAmount;			// KENT fix MS MC -00039

			        CurrencyHelper.DoubleToCurrency(dblDispAmt - CurrencyHelper.CurrencyToDouble(curTmp1), out curTmp2);

			        pObjPDItem.DeprAmount = curTmp2;
				    curTmp1 = pObjItem.EndDatePersonalUseAccum;
				    curTmp2 = pObjItem.EndDateYTDPersonalUse;
				    pObjPDItem.EndDatePersonalUseAccum = curTmp1;
				    pObjPDItem.EndDateYTDPersonalUse = curTmp2;
				    dblTmp1 = pObjItem.PersonalUseAmount;
                    pObjPDItem.PersonalUseAmount = dblTmp1;
		        }
		        else
		        {
			        //
			        // We did not find the correct PD item.  Therefore we need to get the
			        // information we can get, and create the depr information for the disposal
			        // period.
			        //
                    iFYNum1 = pObjItem.FYNum;

			        if ( iFYNum1 == iDisposalYearNumber )
			        {
				        if ( dtStart > dtDisposalPeriodStart )
				        {
					        curTmp1 = 0;

					        pObjPDItem.BeginYearAccum = curTmp1;
						    pObjPDItem.EndDateBeginYearAccum = curTmp1;
						    pObjPDItem.BeginYTDExpense = curTmp1;

					        CurrencyHelper.DoubleToCurrency(dblDispAmt, out curTmp2);
					        pObjPDItem.DeprAmount = curTmp2;
				        }
				        else
				        {
					        curTmp1 = pObjItem.BeginYearAccum;
					        pObjPDItem.BeginYearAccum = curTmp1;
					        pObjPDItem.EndDateBeginYearAccum = curTmp1;
					        curTmp1 = pObjItem.BeginYTDExpense;
					        curTmp2 = pObjItem.DeprAmount;
	
					        CurrencyHelper.DoubleToCurrency(CurrencyHelper.CurrencyToDouble(curTmp1) + CurrencyHelper.CurrencyToDouble(curTmp2), out curTmp2);
					        pObjPDItem.BeginYTDExpense = curTmp2;
			    	        CurrencyHelper.DoubleToCurrency(dblDispAmt - CurrencyHelper.CurrencyToDouble(curTmp1), out curTmp2);
					        pObjPDItem.DeprAmount = curTmp2;
				        }
			        }
			        else
			        {
				        curTmp1 = pObjItem.BeginYearAccum;
					    curTmp2 = pObjItem.BeginYTDExpense;
                        curTmp3 = pObjItem.DeprAmount;
				        CurrencyHelper.DoubleToCurrency(CurrencyHelper.CurrencyToDouble(curTmp1) + CurrencyHelper.CurrencyToDouble(curTmp2) + CurrencyHelper.CurrencyToDouble(curTmp3), out curTmp1);
				        CurrencyHelper.DoubleToCurrency(0, out curTmp2);
				        CurrencyHelper.DoubleToCurrency(dblDispAmt, out curTmp3);
				        pObjPDItem.BeginYearAccum = curTmp1;
					    pObjPDItem.EndDateBeginYearAccum = curTmp1;
					    pObjPDItem.BeginYTDExpense = curTmp2;
					    pObjPDItem.DeprAmount = curTmp3;
			        }
		        }
	        }

	        if ( iCount > 0 )
	        {
		        //
		        // If we have any items in the list, delete the item that holds the disposal period
		        // information from the list.  It is not the disposal record.
		        //
                pColList.RemoveAt(iCount - 1);
	        }
	        //
	        // Make sure that the disposal information is more than a penny.  If it is only a penny,
	        // truncate it.
	        //
	        curTmp1 = pObjPDItem.DeprAmount;

            if( Math.Abs(CurrencyHelper.CurrencyToDouble(curTmp1)) < 0.01 )
	        {
		        CurrencyHelper.DoubleToCurrency(0, out curTmp1);
                pObjPDItem.DeprAmount = curTmp1;
            }
	        //
	        // Now add the disposal record to the list.
	        //
            AppendPeriodDeprItem(pColList,pObjPDItem);

finishyear:
	        //
	        // Check to see if the disposal period is the last period in the year.  If not,
	        // we need to create another PD Item that finishes the year.
	        //
	        if ( dtDisposalPeriodEnd < dtDisposalYearEnd )
	        {
		        short iPdWeights;
		        decimal curTmp;

		        pObjItem = null;
			    PeriodDeprItem obj1 = new PeriodDeprItem();
                pObjItem = (IBAPeriodDeprItem)obj1;

		        pObjItem.StartDate = dtDisposalPeriodEnd.AddDays(1);
			    pObjItem.EndDate = dtDisposalYearEnd;
			    pObjItem.FYNum = iDisposalYearNumber;
			    pObjIFY.GetPeriodWeights(dtDisposalPeriodEnd .AddDays(1), dtDisposalYearEnd, out iPdWeights);
			    pObjItem.TotalPeriodWeights = iPdWeights;
			    pObjItem.EntryType = PERIODDEPRITEM_ENTRYTYPE.PERIODDEPRITEM_NORMAL;

		        curTmp = pObjPDItem.BeginYearAccum;
			    pObjItem.BeginYearAccum = curTmp;
			    pObjItem.EndDateBeginYearAccum = curTmp;

		        CurrencyHelper.DoubleToCurrency(0, out curTmp);
		        pObjItem.EndDateDeferredAccum = curTmp;

		        CurrencyHelper.DoubleToCurrency(0, out curTmp);
		        pObjItem.EndDateDeferredAccum = curTmp;

		        CurrencyHelper.DoubleToCurrency(dblDispAmt, out curTmp);
		        pObjItem.BeginYTDExpense = curTmp;

		        CurrencyHelper.DoubleToCurrency(0, out curTmp);
		        pObjItem.DeprAmount = curTmp;
			    pObjItem.EndDateYTDDeferred = curTmp;
			    pObjItem.CalcFlags = sCalcFlags;
			    AppendPeriodDeprItem(pColList,pObjItem);
    //			 !(hr = FixForAdjustments (pObjItem, pColList, dtAdjStart, dtAdjEnd,
    //								dAdjustAmount, dStartingPersonalAmount, pObjStartPoint, out adjApplied)) )
	        }

             return true;
        }


        public double CalculateDispDeprForBeginInfoYear(List<IBAPeriodDeprItem> pColList, IBAPeriodDeprItem pObjStartPoint)
        {
            IBAPeriodDeprItem	pObjItem;
            int				    iCount;
            DateTime			dtStart;
            DateTime			dtEnd;
            DateTime			adjStartDate = DateTime.MinValue;
            DateTime			adjEndDate = DateTime.MinValue;
            decimal				cyPeriodDepr;
            int					i;

            if ( m_pObjDeprMethod == null || m_pObjConvention == null)
                return 0.0;

            iCount = pColList.Count;

            if ( pObjStartPoint != null )
            {
                adjStartDate = pObjStartPoint.StartDate;
                adjEndDate = pObjStartPoint.EndDate;
                cyPeriodDepr = Convert.ToDecimal(pObjStartPoint.EndPeriodAccum);
            }

            for (i = iCount - 1; i >= 0; i--)
            {
                pObjItem = null;

                pObjItem = pColList[i];

                dtStart = pObjItem.StartDate;
                dtEnd = pObjItem.EndDate;

                if ( adjStartDate == adjEndDate && adjStartDate == dtStart.AddDays(-1) && adjEndDate <= dtEnd )
                {
                    cyPeriodDepr = Convert.ToDecimal (pObjItem.YTDExpense);
                    double			dLife;
                    double			dYearElapsed;
                    double			dFactor;

                    dLife = m_pObjDeprMethod.Life;
                    dYearElapsed = m_pObjDeprMethod.YearElapsed;
                    m_pObjConvention.GetDisposalYearFactor(dLife - dYearElapsed, m_dtDispDate, out dFactor);
                    return CurrencyHelper.CurrencyToDouble(cyPeriodDepr) * dFactor;
                }
            }
            return 0.0;
        }


        public bool GeneratePeriodDeprItem(IBAFiscalYear pObjIFY,
                                double dBeginYearAccumAmt,
                                double dActualDeprAmt,
                                List<IBAPeriodDeprItem> pColPItemList,
                                string szCalcFlags,
                                double deferredAccum,
                                double deferredYTD,
                                double dTotalPersonalAmount,
                                double dAnnualPersonalAmount,
                                bool bLuxuryAutoLimited,
                                double dAdjustAmount,
                                DateTime dtAdjPeriodStart,
                                double dStartingPersonalAmount,
                                IBAPeriodDeprItem pObjStartPoint)
        {
            //generate pd items for the first year
            if (IsFirstYear())
                return GenerateFirstYearPDItem(pObjIFY, dBeginYearAccumAmt, dActualDeprAmt, pColPItemList, szCalcFlags, deferredAccum, deferredYTD, dTotalPersonalAmount, dAnnualPersonalAmount, dAdjustAmount, dtAdjPeriodStart, dStartingPersonalAmount, pObjStartPoint);
            //	} // KENT fix NFaus-00114

            //generate pd items for the last year
            if (IsLastYear() && !bLuxuryAutoLimited)
                return GenerateLastYearPDItem(pObjIFY, dBeginYearAccumAmt, dActualDeprAmt, pColPItemList, szCalcFlags, deferredAccum, deferredYTD, dTotalPersonalAmount, dAnnualPersonalAmount, dAdjustAmount, dtAdjPeriodStart, dStartingPersonalAmount, pObjStartPoint);

            //generate pd items for normal/short year
            return GeneratePDItem(pObjIFY, dBeginYearAccumAmt, dActualDeprAmt, pColPItemList, szCalcFlags, deferredAccum, deferredYTD, dTotalPersonalAmount, dAnnualPersonalAmount, dAdjustAmount, dtAdjPeriodStart, dStartingPersonalAmount, pObjStartPoint);
        }

        public bool GeneratePDItem(IBAFiscalYear pObjIFY,
                                double dBeginYearAccumAmt,
                                double dActualDeprAmt,
                                List<IBAPeriodDeprItem> pColPItemList,
                                string szCalcFlags,
                                double deferredAccum,
                                double deferredYTD,
                                double dTotalPersonalAmount,
                                double dAnnualPersonalAmount,
                                double dAdjustAmount,
                                DateTime dtAdjPeriodStart,
                                double dStartingPersonalAmount,
                                IBAPeriodDeprItem pObjStartPoint)
        {
            IBAPeriodDeprItem pObjPDItem;
            decimal curTmp;
            DateTime dtSDate;
            DateTime dtEDate;
            DateTime dtPISStart;
            short iFYNum;
            short iWgt;
            short iWgtAdj;
            bool hr;
            PeriodDeprItem obj;
            bool adjApplied = false;
            DateTime dtAdjStart = DateTime.MinValue;
            DateTime dtAdjEnd = DateTime.MinValue;

            if (pColPItemList == null)
                return false;

            if (pObjIFY == null)
                return false;

            obj = new PeriodDeprItem();
            pObjPDItem = (IBAPeriodDeprItem)obj;

            dtSDate = pObjIFY.YRStartDate;
            dtEDate = pObjIFY.YREndDate;
            iFYNum = pObjIFY.FYNum;
            pObjIFY.GetTotalFiscalYearPeriodWeights(out iWgt);

            if (dtSDate <= dtAdjPeriodStart && dtEDate >= dtAdjPeriodStart)
            {
                IBACalcPeriod pObjAdjPeriod;

                //
                // We have an adjustment in this year, therefore determine the start and
                // end period dates.
                //
                pObjIFY.GetPeriod(dtAdjPeriodStart, out pObjAdjPeriod);
                dtAdjStart = pObjAdjPeriod.PeriodStart;
                dtAdjEnd = pObjAdjPeriod.PeriodEnd;
            }

            if (iWgt == 0)
                return false;

            //	{
            //		IBACalcPeriod> pObjPeriod;
            //
            //		// If we are in the first year, and the PIS date is after the FY Begin, write out a blank 
            //		// PeriodDeprItem so that the depreciation starts after the PIS date.
            //		if ( !(hr = pObjIFY.GetPeriod(m_dtPISDate, out pObjPeriod)) ||
            //			 !(hr = pObjPeriod.PeriodStart(&dtPISStart)) ||
            //			 !(hr = pObjIFY.GetPreviousPeriodWeights(m_dtPISDate, out iWgtAdj)) )
            //		{
            iWgtAdj = 0;
            dtPISStart = dtSDate;
            //		}
            //	}

            //use the input infor to build the pd item, only one item
            pObjPDItem.StartDate = (dtPISStart);
            pObjPDItem.EndDate = (dtEDate);
            pObjPDItem.FYNum = (iFYNum);
            pObjPDItem.TotalPeriodWeights = (short)(iWgt - iWgtAdj);
            pObjPDItem.EntryType = (PERIODDEPRITEM_ENTRYTYPE.PERIODDEPRITEM_NORMAL);

            CurrencyHelper.DoubleToCurrency(dBeginYearAccumAmt, out curTmp);
            pObjPDItem.BeginYearAccum = (curTmp);
            pObjPDItem.EndDateBeginYearAccum = (curTmp);

            CurrencyHelper.DoubleToCurrency(deferredAccum, out curTmp);
            pObjPDItem.EndDateDeferredAccum = (curTmp);

            CurrencyHelper.DoubleToCurrency(dTotalPersonalAmount, out curTmp);
            pObjPDItem.EndDatePersonalUseAccum = (curTmp);

            CurrencyHelper.DoubleToCurrency(0, out curTmp);
            pObjPDItem.BeginYTDExpense = (curTmp);

            CurrencyHelper.DoubleToCurrency(dActualDeprAmt, out curTmp);
            pObjPDItem.DeprAmount = (curTmp);

            CurrencyHelper.DoubleToCurrency(deferredYTD, out curTmp);
            pObjPDItem.EndDateYTDDeferred = (curTmp);

            CurrencyHelper.DoubleToCurrency(dAnnualPersonalAmount + dStartingPersonalAmount, out curTmp);
            pObjPDItem.EndDateYTDPersonalUse = (curTmp);
            pObjPDItem.PersonalUseAmount = (dAnnualPersonalAmount);

            pObjPDItem.CalcFlags = (szCalcFlags);

            if (!(hr = FixForAdjustments(pObjPDItem, pColPItemList, dtAdjStart, dtAdjEnd,
                                    dAdjustAmount, dStartingPersonalAmount, pObjStartPoint, out adjApplied)))
                return hr;

            return true;
        }

        public bool GenerateFirstYearPDItem(IBAFiscalYear pObjIFY,
                                        double dBeginYearAccumAmt,
                                        double dActualDeprAmt,
                                        List<IBAPeriodDeprItem> pColPItemList,
                                        string szCalcFlags,
                                        double deferredAccum,
                                        double deferredYTD,
                                        double dTotalPersonalAmount,
                                        double dAnnualPersonalAmount,
                                        double dAdjustAmount,
                                        DateTime dtAdjPeriodStart,
                                        double dStartingPersonalAmount,
                                        IBAPeriodDeprItem pObjStartPoint)
        {
            double dDeprAmt = 0;
            double dFraction = 0;
            double dDeferredAmt = 0;
            double dPersonalAmt = 0;
            DateTime dtFSStartDate = DateTime.MinValue;
            DateTime dtFSEndDate = DateTime.MinValue;
            short iFSTPWeights = 0;
            DateTime dtRSStartDate = DateTime.MinValue;
            DateTime dtRSEndDate = DateTime.MinValue;
            short iRSTPWeights = 0;
            bool bRet;
            decimal curTmp;
            short iFYNum;
            short iPdWeights;
            IBAPeriodDeprItem pObjPDItem;
            IBACalcPeriod pObjIPd;
            bool hr;
            DateTime dtDt;
            bool adjApplied = false;
            DateTime dtAdjStart = DateTime.MinValue;
            DateTime dtAdjEnd = DateTime.MinValue;
            DateTime dtYrEnd;


            if (pColPItemList == null)
                return false;

            if (pObjIFY == null)
                return false;

            dtDt = pObjIFY.YRStartDate;
            dtYrEnd = pObjIFY.YREndDate;
            iFYNum = pObjIFY.FYNum;

            m_pObjConvention.IsSplitNeeded(dtDt, out bRet);

            if (dtDt <= dtAdjPeriodStart && dtYrEnd >= dtAdjPeriodStart)
            {
                IBACalcPeriod pObjAdjPeriod;

                //
                // We have an adjustment in this year, therefore determine the start and
                // end period dates.
                //

                pObjIFY.GetPeriod(dtAdjPeriodStart, out pObjAdjPeriod);
                dtAdjStart = pObjAdjPeriod.PeriodStart;
                dtAdjEnd = pObjAdjPeriod.PeriodEnd;
            }

            if (bRet)
            {
                PeriodDeprItem obj;
                //split needed, then let the m_pObjConvention to calc the fraction for each periods
                if (!(hr = m_pObjConvention.GetFirstYearSegmentInfo(ref dFraction,
                        ref dtFSStartDate, ref dtFSEndDate, ref iFSTPWeights,
                        ref dtRSStartDate, ref dtRSEndDate, ref iRSTPWeights, out bRet)))
                    return false;

                obj = new PeriodDeprItem();
                pObjPDItem = (IBAPeriodDeprItem)obj;
                iFYNum = pObjIFY.FYNum;

                if (iFSTPWeights == 0)
                    return false;

                //the first period
                pObjPDItem.StartDate = (dtFSStartDate);
                pObjPDItem.EndDate = (dtFSEndDate);
                pObjPDItem.FYNum = (iFYNum);
                pObjPDItem.TotalPeriodWeights = (iFSTPWeights);
                pObjPDItem.EntryType = PERIODDEPRITEM_ENTRYTYPE.PERIODDEPRITEM_FIRSTPERIOD;

                CurrencyHelper.DoubleToCurrency(dBeginYearAccumAmt, out curTmp);
                pObjPDItem.BeginYearAccum = (curTmp);
                pObjPDItem.EndDateBeginYearAccum = (curTmp);

                CurrencyHelper.DoubleToCurrency(deferredAccum, out curTmp);
                pObjPDItem.EndDateDeferredAccum = (curTmp);

                CurrencyHelper.DoubleToCurrency(dTotalPersonalAmount, out curTmp);
                pObjPDItem.EndDatePersonalUseAccum = (curTmp);

                CurrencyHelper.DoubleToCurrency(0, out curTmp);
                pObjPDItem.BeginYTDExpense = (curTmp);

                dDeprAmt = CurrencyHelper.FormatCurrency(dFraction * dActualDeprAmt);
                CurrencyHelper.DoubleToCurrency(dDeprAmt /*+ dS179Amount*/, out curTmp);
                pObjPDItem.DeprAmount = (curTmp);

                dDeferredAmt = CurrencyHelper.FormatCurrency(dFraction * deferredYTD);
                CurrencyHelper.DoubleToCurrency(dDeferredAmt, out curTmp);
                pObjPDItem.EndDateYTDDeferred = (curTmp);

                dPersonalAmt = CurrencyHelper.FormatCurrency(dFraction * dAnnualPersonalAmount + dStartingPersonalAmount);
                CurrencyHelper.DoubleToCurrency(dPersonalAmt, out curTmp);
                pObjPDItem.EndDateYTDPersonalUse = (curTmp);
                pObjPDItem.PersonalUseAmount = (CurrencyHelper.FormatCurrency(dFraction * dAnnualPersonalAmount));

                pObjPDItem.CalcFlags = (szCalcFlags);

                if (!(hr = FixForAdjustments(pObjPDItem, pColPItemList, dtAdjStart, dtAdjEnd,
                                        dAdjustAmount, dStartingPersonalAmount, pObjStartPoint, out adjApplied)))
                    return hr;
                //
                // If the adjustment was applied, update the YTD amount for any PD items that follow.
                //
                if (adjApplied)
                {
                    dDeprAmt += dAdjustAmount;
                }

                if (dtRSStartDate < dtRSEndDate)
                {
                    pObjPDItem = null;
                    obj = new PeriodDeprItem();
                    pObjPDItem = (IBAPeriodDeprItem)obj;

                    if (iRSTPWeights == 0)
                        return false;

                    //the second period
                    pObjPDItem.StartDate = (dtRSStartDate);
                    pObjPDItem.EndDate = (dtRSEndDate);
                    pObjPDItem.FYNum = (iFYNum);
                    pObjPDItem.TotalPeriodWeights = (iRSTPWeights);
                    pObjPDItem.EntryType = (PERIODDEPRITEM_ENTRYTYPE.PERIODDEPRITEM_NORMAL);

                    CurrencyHelper.DoubleToCurrency(dBeginYearAccumAmt, out curTmp);
                    pObjPDItem.BeginYearAccum = (curTmp);
                    pObjPDItem.EndDateBeginYearAccum = (curTmp);

                    CurrencyHelper.DoubleToCurrency(deferredAccum + dDeferredAmt, out curTmp);
                    pObjPDItem.EndDateDeferredAccum = (curTmp);

                    CurrencyHelper.DoubleToCurrency(dTotalPersonalAmount, out curTmp);
                    pObjPDItem.EndDatePersonalUseAccum = (curTmp);

                    CurrencyHelper.DoubleToCurrency(dDeprAmt /*+ dS179Amount*/, out curTmp);
                    pObjPDItem.BeginYTDExpense = (curTmp);

                    CurrencyHelper.DoubleToCurrency(dActualDeprAmt - dDeprAmt, out curTmp);
                    pObjPDItem.DeprAmount = (curTmp);

                    CurrencyHelper.DoubleToCurrency(deferredYTD - dDeferredAmt, out curTmp);
                    pObjPDItem.EndDateYTDDeferred = (curTmp);

                    CurrencyHelper.DoubleToCurrency(dAnnualPersonalAmount + dStartingPersonalAmount, out curTmp);
                    pObjPDItem.EndDateYTDPersonalUse = (curTmp);
                    pObjPDItem.PersonalUseAmount = (CurrencyHelper.FormatCurrency((1 - dFraction) * dAnnualPersonalAmount));

                    pObjPDItem.CalcFlags = (szCalcFlags);

                    if (!(hr = FixForAdjustments(pObjPDItem, pColPItemList, dtAdjStart, dtAdjEnd,
                                            dAdjustAmount, dStartingPersonalAmount, pObjStartPoint, out adjApplied)))
                        return hr;

                }
                else
                    dtRSEndDate = dtFSEndDate;
                //
                // If the end date is before the end of the year, put out another
                // item to finish the year.
                //
                if (dtRSEndDate < dtYrEnd)
                {
                    pObjPDItem = null;
                    obj = new PeriodDeprItem();
                    pObjPDItem = (IBAPeriodDeprItem)obj;

                    pObjPDItem.StartDate = (dtRSEndDate.AddDays(1));
                    pObjPDItem.EndDate = (dtYrEnd);
                    iFYNum = pObjIFY.FYNum;
                    pObjPDItem.FYNum = (iFYNum);
                    pObjIFY.GetPeriodWeights(dtRSEndDate.AddDays(1), dtYrEnd, out iPdWeights);
                    pObjPDItem.TotalPeriodWeights = (iPdWeights);
                    pObjPDItem.EntryType = (PERIODDEPRITEM_ENTRYTYPE.PERIODDEPRITEM_NORMAL);

                    CurrencyHelper.DoubleToCurrency(dBeginYearAccumAmt, out curTmp);
                    pObjPDItem.BeginYearAccum = (curTmp);
                    pObjPDItem.EndDateBeginYearAccum = (curTmp);

                    CurrencyHelper.DoubleToCurrency(deferredAccum + deferredYTD, out curTmp);
                    pObjPDItem.EndDateDeferredAccum = (curTmp);

                    CurrencyHelper.DoubleToCurrency(dTotalPersonalAmount, out curTmp);
                    pObjPDItem.EndDatePersonalUseAccum = (curTmp);

                    CurrencyHelper.DoubleToCurrency(dAnnualPersonalAmount + dStartingPersonalAmount, out curTmp);
                    pObjPDItem.EndDateYTDPersonalUse = (curTmp);

                    if (adjApplied)
                        dActualDeprAmt += dAdjustAmount;

                    CurrencyHelper.DoubleToCurrency(dActualDeprAmt, out curTmp);
                    pObjPDItem.BeginYTDExpense = (curTmp);

                    CurrencyHelper.DoubleToCurrency(0, out curTmp);
                    pObjPDItem.DeprAmount = (curTmp);
                    pObjPDItem.EndDateYTDDeferred = (curTmp);
                    pObjPDItem.CalcFlags = (szCalcFlags);
                    if (!(hr = FixForAdjustments(pObjPDItem, pColPItemList, dtAdjStart,
                                                   dtAdjEnd, dAdjustAmount, dStartingPersonalAmount, pObjStartPoint, out adjApplied)))
                        return hr;

                }
            }
            else
            {
                //no split needed, only one item
                PeriodDeprItem obj;
                short pisWeight = 0;
                DateTime dtStartPeriodDate = DateTime.MinValue;
                DateTime dtEndPeriodDate = DateTime.MinValue;
                double dDeprAdjustment = 0;
                double dDeferredAdjustment = 0;
                double dPersonalUseAdjustment = 0;
                DateTime dtYearStart;
                DateTime dtYearEnd;
                DateTime dtPISPeriodStart;
                DateTime dtPISPeriodEnd;
                IBACalcPeriod pObjPISPeriod;
                bool bBuildSecondItem = true;

                dtYearStart = pObjIFY.YRStartDate;
                dtYearEnd = pObjIFY.YREndDate;

                //
                // Determine the Placed In Service date period information.
                //
                if (m_dtPISDate < dtYearStart || m_dtPISDate > dtYearEnd)
                {
                    dtPISPeriodStart = DateTime.MinValue;
                    dtPISPeriodEnd = DateTime.MinValue;
                }
                else
                {
                    pObjIFY.GetPeriod(m_dtPISDate, out pObjPISPeriod);
                    if (pObjPISPeriod != null)
                    {
                        dtPISPeriodStart = pObjPISPeriod.PeriodStart;
                        dtPISPeriodEnd = pObjPISPeriod.PeriodEnd;
                    }
                    else
                    {
                        dtPISPeriodStart = DateTime.MinValue;
                        dtPISPeriodEnd = DateTime.MinValue;
                    }
                }

                //
                // Now determine the appropriate start date information.
                //
                //		if ( dtStartPeriodDate > dtYearEnd )
                dtStartPeriodDate = m_dtPISDate;

                dtEndPeriodDate = dtYearEnd;
                if (dtEndPeriodDate > m_dtDeemedEndDate)
                    dtEndPeriodDate = m_dtDeemedEndDate;

                //
                // Now see if we have Section 179 that needs to be allocated to the PIS period.
                //
                if (false /*dS179Amount > 0*/ )
                {
                    //
                    // Make sure that we are in the Placed In Service fiscal year.
                    //
                    if (m_dtPISDate >= dtYearStart && m_dtPISDate <= dtYearEnd)
                    {
                        double factor;

                        //
                        // At this point we have determined that we have section 179 and we are in the placed in service year.
                        // Therefore we need to create a PDItem for the Placed In Service period that contains the depr for 
                        // PIS period + the Section 179 amount.
                        //

                        obj = new PeriodDeprItem();
                        pObjPDItem = (IBAPeriodDeprItem)obj;

                        pisWeight = pObjPISPeriod.Weight;
                        pObjPDItem.StartDate = (dtPISPeriodStart);
                        pObjPDItem.EndDate = (dtPISPeriodEnd);
                        iFYNum = pObjIFY.FYNum;
                        pObjPDItem.FYNum = (iFYNum);
                        pObjPDItem.TotalPeriodWeights = (pisWeight);
                        pObjPDItem.EntryType = (PERIODDEPRITEM_ENTRYTYPE.PERIODDEPRITEM_NORMAL);

                        //
                        // Now we need to determine the calc year end date, and get the total period weights.  This is needed
                        // so that we can allocate the correct amount of depr to the PIS period.
                        //
                        if (!(hr = pObjIFY.GetPeriodWeights(dtPISPeriodStart, dtEndPeriodDate, out iPdWeights)))
                            return hr;

                        //
                        // Now we need to set the non-changing portions of the PD Item.
                        //
                        CurrencyHelper.DoubleToCurrency(dBeginYearAccumAmt, out curTmp);
                        pObjPDItem.BeginYearAccum = (curTmp);
                        pObjPDItem.EndDateBeginYearAccum = (curTmp);

                        CurrencyHelper.DoubleToCurrency(dTotalPersonalAmount, out curTmp);
                        pObjPDItem.EndDatePersonalUseAccum = (curTmp);

                        CurrencyHelper.DoubleToCurrency(deferredAccum, out curTmp);
                        pObjPDItem.EndDateDeferredAccum = (curTmp);

                        CurrencyHelper.DoubleToCurrency(0, out curTmp);
                        pObjPDItem.BeginYTDExpense = (curTmp);
                        pObjPDItem.CalcFlags = (szCalcFlags);

                        //
                        // Now we need to do the allocation of the depr amounts that need to be allocated to this period.
                        //
                        if (iPdWeights == 0)
                            factor = 1;
                        else
                            factor = pisWeight / iPdWeights;

                        dDeferredAdjustment = CurrencyHelper.FormatCurrency(deferredYTD * factor);
                        CurrencyHelper.DoubleToCurrency(dDeferredAdjustment, out curTmp);
                        pObjPDItem.EndDateYTDDeferred = (curTmp);

                        dPersonalUseAdjustment = CurrencyHelper.FormatCurrency(dAnnualPersonalAmount * factor);
                        CurrencyHelper.DoubleToCurrency(dPersonalUseAdjustment, out curTmp);
                        pObjPDItem.EndDateYTDPersonalUse = (curTmp);

                        dDeprAdjustment = CurrencyHelper.FormatCurrency(dActualDeprAmt * factor);
                        CurrencyHelper.DoubleToCurrency(dDeprAdjustment, out curTmp);
                        pObjPDItem.DeprAmount = (curTmp);

                        //
                        // Now we are ready to add this item to the list of PD items.
                        //
                        if (!(hr = FixForAdjustments(pObjPDItem, pColPItemList, dtAdjStart, dtAdjEnd,
                                                dAdjustAmount, dStartingPersonalAmount, pObjStartPoint, out adjApplied)))
                            return hr;

                        dtStartPeriodDate = dtPISPeriodEnd.AddDays(1);
                        if (dtPISPeriodEnd == dtYearEnd)
                            bBuildSecondItem = false;
                    } // in the PIS year
                } // has s179 amount

                //
                // Now see if we still need to create the second PD (or first if no S179) item.
                //
                if (bBuildSecondItem)
                {
                    obj = new PeriodDeprItem();
                    pObjPDItem = (IBAPeriodDeprItem)obj;

                    pObjPDItem.StartDate = (dtStartPeriodDate);
                    pObjPDItem.EndDate = (dtEndPeriodDate);
                    iFYNum = pObjIFY.FYNum;
                    pObjPDItem.FYNum = (iFYNum);

                    if (!(hr = pObjIFY.GetPeriodWeights(dtStartPeriodDate, dtEndPeriodDate, out iPdWeights)))
                        return hr;

                    if (iPdWeights == 0)
                        return false;

                    pObjPDItem.TotalPeriodWeights = (iPdWeights);
                    pObjPDItem.EntryType = (PERIODDEPRITEM_ENTRYTYPE.PERIODDEPRITEM_NORMAL);

                    CurrencyHelper.DoubleToCurrency(dBeginYearAccumAmt, out curTmp);
                    pObjPDItem.BeginYearAccum = (curTmp);
                    pObjPDItem.EndDateBeginYearAccum = (curTmp);

                    CurrencyHelper.DoubleToCurrency(deferredAccum + dDeferredAdjustment, out curTmp);
                    pObjPDItem.EndDateDeferredAccum = (curTmp);

                    CurrencyHelper.DoubleToCurrency(deferredYTD - dDeferredAdjustment, out curTmp);
                    pObjPDItem.EndDateYTDDeferred = (curTmp);

                    CurrencyHelper.DoubleToCurrency(dTotalPersonalAmount + dPersonalUseAdjustment, out curTmp);
                    pObjPDItem.EndDatePersonalUseAccum = (curTmp);

                    CurrencyHelper.DoubleToCurrency(dAnnualPersonalAmount - dPersonalUseAdjustment + dStartingPersonalAmount, out curTmp);
                    pObjPDItem.EndDateYTDPersonalUse = (curTmp);
                    pObjPDItem.PersonalUseAmount = (dAnnualPersonalAmount - dPersonalUseAdjustment);

                    //
                    // If the adjustment was applied, update the YTD amount for any PD items that follow.
                    //
                    if (adjApplied)
                        CurrencyHelper.DoubleToCurrency(dDeprAdjustment + dAdjustAmount /*+ dS179Amount*/, out curTmp);
                    else
                        CurrencyHelper.DoubleToCurrency(dDeprAdjustment /*+ dS179Amount*/, out curTmp);
                    pObjPDItem.BeginYTDExpense = (curTmp);

                    CurrencyHelper.DoubleToCurrency(dActualDeprAmt - dDeprAdjustment, out curTmp);
                    pObjPDItem.DeprAmount = (curTmp);

                    pObjPDItem.CalcFlags = (szCalcFlags);
                    if (!(hr = FixForAdjustments(pObjPDItem, pColPItemList, dtAdjStart,
                                                    dtAdjEnd, dAdjustAmount, dStartingPersonalAmount, pObjStartPoint, out adjApplied)))
                        return hr;
                }
                //
                // If the end date is before the end of the year, put out another
                // item to finish the year.
                //
                if (dtEndPeriodDate < dtYrEnd)
                {
                    pObjPDItem = null;
                    obj = new PeriodDeprItem();
                    pObjPDItem = (IBAPeriodDeprItem)obj;

                    pObjPDItem.StartDate = (dtEndPeriodDate.AddDays(1));
                    pObjPDItem.EndDate = (dtYrEnd);
                    iFYNum = pObjIFY.FYNum;
                    pObjPDItem.FYNum = (iFYNum);
                    pObjIFY.GetPeriodWeights(dtEndPeriodDate.AddDays(1), dtYrEnd, out iPdWeights);
                    pObjPDItem.TotalPeriodWeights = (iPdWeights);
                    pObjPDItem.EntryType = (PERIODDEPRITEM_ENTRYTYPE.PERIODDEPRITEM_NORMAL);

                    CurrencyHelper.DoubleToCurrency(dBeginYearAccumAmt, out curTmp);
                    pObjPDItem.BeginYearAccum = (curTmp);
                    pObjPDItem.EndDateBeginYearAccum = (curTmp);

                    CurrencyHelper.DoubleToCurrency(deferredAccum + deferredYTD, out curTmp);
                    pObjPDItem.EndDateDeferredAccum = (curTmp);

                    CurrencyHelper.DoubleToCurrency(dTotalPersonalAmount, out curTmp);
                    pObjPDItem.EndDatePersonalUseAccum = (curTmp);

                    CurrencyHelper.DoubleToCurrency(dAnnualPersonalAmount + dStartingPersonalAmount, out curTmp);
                    pObjPDItem.EndDateYTDPersonalUse = (curTmp);

                    if (adjApplied)
                        dActualDeprAmt += dAdjustAmount;

                    CurrencyHelper.DoubleToCurrency(dActualDeprAmt, out curTmp);
                    pObjPDItem.BeginYTDExpense = (curTmp);

                    CurrencyHelper.DoubleToCurrency(0, out curTmp);
                    pObjPDItem.DeprAmount = (curTmp);
                    pObjPDItem.EndDateYTDDeferred = (curTmp);
                    pObjPDItem.CalcFlags = (szCalcFlags);
                    if (!(hr = FixForAdjustments(pObjPDItem, pColPItemList, dtAdjStart,
                                                    dtAdjEnd, dAdjustAmount, dStartingPersonalAmount, pObjStartPoint, out adjApplied)))
                        return hr;

                }
            }
            return true;
        }

        public bool GenerateLastYearPDItem(IBAFiscalYear pObjIFY,
                                        double dBeginYearAccumAmt,
                                        double dActualDeprAmt,
                                        List<IBAPeriodDeprItem> pColPItemList,
                                        string szCalcFlags,
                                        double deferredAccum,
                                        double deferredYTD,
                                        double dTotalPersonalAmount,
                                        double dAnnualPersonalAmount,
                                        double dAdjustAmount,
                                        DateTime dtAdjPeriodStart,
                                        double dStartingPersonalAmount,
                                        IBAPeriodDeprItem pObjStartPoint)
        {
            double dDeprAmt = 0;
            double dDeferredAmt = 0;
            double dPersonalAmt = 0;
            double dFraction = 0;
            DateTime dtFSStartDate = DateTime.MinValue;
            DateTime dtFSEndDate = DateTime.MinValue;
            DateTime dtFYEndDate = DateTime.MinValue;
            short iFSTPWeights = 0;
            DateTime dtRSStartDate = DateTime.MinValue;
            DateTime dtRSEndDate = DateTime.MinValue;
            short iRSTPWeights = 0;
            bool bRet;
            decimal curTmp;
            short iFYNum;
            short iPdWeights;
            DateTime dtSDate;
            DateTime dtEDate;
            IBAPeriodDeprItem pObjPDItem;
            IBACalcPeriod pObjIPd;
            bool hr;
            DateTime dtDt;
            IBADeprTableSupport tableSupp = (m_pObjDeprMethod) as IBADeprTableSupport;
            bool bInPostRecovery = false;
            bool adjApplied = false;
            DateTime dtAdjStart = DateTime.MinValue;
            DateTime dtAdjEnd = DateTime.MinValue;

            if (pColPItemList == null)
                return false;

            if (pObjIFY == null)
                return false;


            dtDt = pObjIFY.YRStartDate;
            dtFYEndDate = pObjIFY.YREndDate;
            m_pObjConvention.IsSplitNeeded(dtDt, out bRet);

            if (dtDt <= dtAdjPeriodStart && dtFYEndDate >= dtAdjPeriodStart)
            {
                IBACalcPeriod pObjAdjPeriod;

                //
                // We have an adjustment in this year, therefore determine the start and
                // end period dates.
                //
                pObjIFY.GetPeriod(dtAdjPeriodStart, out pObjAdjPeriod);
                dtAdjStart = pObjAdjPeriod.PeriodStart;
                dtAdjEnd = pObjAdjPeriod.PeriodEnd;
            }

            if (tableSupp != null)
            {
                bInPostRecovery = tableSupp.InPostRecovery;
            }

            if (bRet)
            {
                PeriodDeprItem obj;

                //split needed, then let the mobjConvention to cal the fraction for each periods
                if (!(hr = m_pObjConvention.GetLastYearSegmentInfo(ref dFraction,
                        ref dtFSStartDate, ref dtFSEndDate, ref iFSTPWeights,
                        ref dtRSStartDate, ref dtRSEndDate, ref iRSTPWeights, out bRet)))
                    return false;

                obj = new PeriodDeprItem();
                pObjPDItem = (IBAPeriodDeprItem)obj;

                iFYNum = pObjIFY.FYNum;

                if (iFSTPWeights == 0)
                    return false;

                if (dtFSStartDate > dtRSStartDate)
                {
                    //the first period
                    pObjPDItem.StartDate = (dtRSStartDate);
                    pObjPDItem.EndDate = (dtRSEndDate);
                    pObjPDItem.FYNum = (iFYNum);
                    pObjPDItem.TotalPeriodWeights = (iRSTPWeights);
                    pObjPDItem.EntryType = (PERIODDEPRITEM_ENTRYTYPE.PERIODDEPRITEM_NORMAL);

                    CurrencyHelper.DoubleToCurrency(dBeginYearAccumAmt, out curTmp);
                    pObjPDItem.BeginYearAccum = (curTmp);
                    pObjPDItem.EndDateBeginYearAccum = (curTmp);

                    CurrencyHelper.DoubleToCurrency(deferredAccum, out curTmp);
                    pObjPDItem.EndDateDeferredAccum = (curTmp);

                    CurrencyHelper.DoubleToCurrency(dTotalPersonalAmount, out curTmp);
                    pObjPDItem.EndDatePersonalUseAccum = (curTmp);

                    CurrencyHelper.DoubleToCurrency(0, out curTmp);
                    pObjPDItem.BeginYTDExpense = (curTmp);

                    dDeprAmt = CurrencyHelper.FormatCurrency(dFraction * dActualDeprAmt);
                    CurrencyHelper.DoubleToCurrency(dActualDeprAmt - dDeprAmt, out curTmp);
                    pObjPDItem.DeprAmount = (curTmp);

                    dDeferredAmt = CurrencyHelper.FormatCurrency(dFraction * deferredYTD);
                    CurrencyHelper.DoubleToCurrency(deferredYTD - dDeferredAmt, out curTmp);
                    pObjPDItem.EndDateYTDDeferred = (curTmp);

                    dPersonalAmt = CurrencyHelper.FormatCurrency(dFraction * dAnnualPersonalAmount);
                    CurrencyHelper.DoubleToCurrency(dAnnualPersonalAmount - dPersonalAmt + dStartingPersonalAmount, out curTmp);
                    pObjPDItem.EndDateYTDPersonalUse = (curTmp);
                    pObjPDItem.PersonalUseAmount = (dPersonalAmt);

                    pObjPDItem.CalcFlags = (szCalcFlags);

                    if (!(hr = FixForAdjustments(pObjPDItem, pColPItemList, dtAdjStart, dtAdjEnd,
                                            dAdjustAmount, dStartingPersonalAmount, pObjStartPoint, out adjApplied)))
                        return hr;
                }

                pObjPDItem = null;
                obj = new PeriodDeprItem();
                pObjPDItem = (IBAPeriodDeprItem)obj;

                if (iFSTPWeights == 0)
                    return false;

                //the second period
                pObjPDItem.StartDate = (dtFSStartDate);
                pObjPDItem.EndDate = (dtFSEndDate);
                pObjPDItem.FYNum = (iFYNum);
                pObjPDItem.TotalPeriodWeights = (iFSTPWeights);
                pObjPDItem.EntryType = (PERIODDEPRITEM_ENTRYTYPE.PERIODDEPRITEM_LASTPERIOD);

                CurrencyHelper.DoubleToCurrency(dBeginYearAccumAmt, out curTmp);
                pObjPDItem.BeginYearAccum = (curTmp);
                pObjPDItem.EndDateBeginYearAccum = (curTmp);

                CurrencyHelper.DoubleToCurrency(deferredAccum + dDeferredAmt, out curTmp);
                pObjPDItem.EndDateDeferredAccum = (curTmp);

                CurrencyHelper.DoubleToCurrency(dTotalPersonalAmount, out curTmp);
                pObjPDItem.EndDatePersonalUseAccum = (curTmp);

                if (dFraction == 1)
                {
                    //only one item
                    CurrencyHelper.DoubleToCurrency(0, out curTmp);
                    pObjPDItem.BeginYTDExpense = (curTmp);

                    CurrencyHelper.DoubleToCurrency(dActualDeprAmt, out curTmp);
                    pObjPDItem.DeprAmount = (curTmp);

                    CurrencyHelper.DoubleToCurrency(deferredYTD, out curTmp);
                    pObjPDItem.EndDateYTDDeferred = (curTmp);

                    CurrencyHelper.DoubleToCurrency(dAnnualPersonalAmount + dStartingPersonalAmount, out curTmp);
                    pObjPDItem.EndDateYTDPersonalUse = (curTmp);
                    pObjPDItem.PersonalUseAmount = (dAnnualPersonalAmount);
                }
                else
                {
                    if (adjApplied)
                        CurrencyHelper.DoubleToCurrency(dActualDeprAmt - dDeprAmt + dAdjustAmount, out curTmp);
                    else
                        CurrencyHelper.DoubleToCurrency(dActualDeprAmt - dDeprAmt, out curTmp);
                    pObjPDItem.BeginYTDExpense = (curTmp);
                    pObjPDItem.PersonalUseAmount = (dAnnualPersonalAmount);

                    CurrencyHelper.DoubleToCurrency(dAnnualPersonalAmount + dStartingPersonalAmount, out curTmp);
                    pObjPDItem.EndDateYTDPersonalUse = (curTmp);

                    CurrencyHelper.DoubleToCurrency(deferredYTD - dDeferredAmt, out curTmp);
                    pObjPDItem.EndDateYTDDeferred = (curTmp);

                    CurrencyHelper.DoubleToCurrency(dDeprAmt, out curTmp);
                    pObjPDItem.DeprAmount = (curTmp);
                    if (adjApplied)
                        dActualDeprAmt += dAdjustAmount;
                }

                pObjPDItem.CalcFlags = (szCalcFlags);
                if (!(hr = FixForAdjustments(pObjPDItem, pColPItemList, dtAdjStart, dtAdjEnd,
                                        dAdjustAmount, dStartingPersonalAmount, pObjStartPoint, out adjApplied)))
                    return hr;
                //
                // If the end date is before the end of the year, put out another
                // item to finish the year.
                //
                pObjPDItem = null;
                pObjPDItem = pColPItemList[pColPItemList.Count - 1];
                DateTime dtTmp;
                dtTmp = pObjPDItem.EndDate;
                dtFSEndDate = dtTmp;

                if (dtFSEndDate < dtFYEndDate)
                {
                    // KENT start fix RM Gr-00245
                    decimal ccurTmp = (decimal)pObjPDItem.EndDateYTDExpense;
                    dActualDeprAmt = CurrencyHelper.CurrencyToDouble(ccurTmp);
                    // KENT end

                    pObjPDItem = null;
                    obj = new PeriodDeprItem();
                    pObjPDItem = (IBAPeriodDeprItem)obj;

                    pObjPDItem.StartDate = (dtFSEndDate.AddDays(1));
                    pObjPDItem.EndDate = (dtFYEndDate);
                    iFYNum = pObjIFY.FYNum;
                    pObjPDItem.FYNum = (iFYNum);
                    pObjIFY.GetPeriodWeights(dtFSEndDate.AddDays(1), dtFYEndDate, out iPdWeights);
                    pObjPDItem.TotalPeriodWeights = (iPdWeights);
                    pObjPDItem.EntryType = (PERIODDEPRITEM_ENTRYTYPE.PERIODDEPRITEM_NORMAL);

                    CurrencyHelper.DoubleToCurrency(dBeginYearAccumAmt, out curTmp);
                    pObjPDItem.BeginYearAccum = (curTmp);
                    pObjPDItem.EndDateBeginYearAccum = (curTmp);

                    CurrencyHelper.DoubleToCurrency(deferredAccum + deferredYTD, out curTmp);
                    pObjPDItem.EndDateDeferredAccum = (curTmp);

                    CurrencyHelper.DoubleToCurrency(dTotalPersonalAmount + dAnnualPersonalAmount + dStartingPersonalAmount, out curTmp);
                    pObjPDItem.EndDatePersonalUseAccum = (curTmp);

                    if (adjApplied)
                        dActualDeprAmt += dAdjustAmount;

                    CurrencyHelper.DoubleToCurrency(dActualDeprAmt, out curTmp);
                    pObjPDItem.BeginYTDExpense = (curTmp);

                    CurrencyHelper.DoubleToCurrency(0, out curTmp);
                    pObjPDItem.DeprAmount = (curTmp);
                    pObjPDItem.EndDateYTDDeferred = (curTmp);
                    pObjPDItem.CalcFlags = (szCalcFlags);
                    if (!(hr = FixForAdjustments(pObjPDItem, pColPItemList, dtAdjStart,
                                                   dtAdjEnd, dAdjustAmount, dStartingPersonalAmount, pObjStartPoint, out adjApplied)))
                        return hr;
                }
            }
            else
            {
                PeriodDeprItem obj;

                //no split needed, only one item
                obj = new PeriodDeprItem();
                pObjPDItem = (IBAPeriodDeprItem)obj;

                dtSDate = pObjIFY.YRStartDate;

                pObjPDItem.StartDate = (dtSDate);

                if (bInPostRecovery)
                {
                    dtEDate = dtFYEndDate;
                    pObjPDItem.EndDate = (dtEDate);
                }
                else
                {
                    pObjIFY.GetPeriod(m_dtDeemedEndDate, out pObjIPd);
                    dtEDate = pObjIPd.PeriodEnd;
                    pObjPDItem.EndDate = (dtEDate);
                }

                iFYNum = pObjIFY.FYNum;
                pObjPDItem.FYNum = (iFYNum);

                if (!(hr = pObjIFY.GetPeriodWeights(dtSDate, dtEDate, out iPdWeights)))
                    return hr;
                pObjPDItem.TotalPeriodWeights = (iPdWeights);
                pObjPDItem.EntryType = (PERIODDEPRITEM_ENTRYTYPE.PERIODDEPRITEM_NORMAL);

                CurrencyHelper.DoubleToCurrency(dBeginYearAccumAmt, out curTmp);
                pObjPDItem.BeginYearAccum = (curTmp);
                pObjPDItem.EndDateBeginYearAccum = (curTmp);

                CurrencyHelper.DoubleToCurrency(deferredAccum, out curTmp);
                pObjPDItem.EndDateDeferredAccum = (curTmp);

                CurrencyHelper.DoubleToCurrency(dTotalPersonalAmount, out curTmp);
                pObjPDItem.EndDatePersonalUseAccum = (curTmp);

                CurrencyHelper.DoubleToCurrency(0, out  curTmp);
                pObjPDItem.BeginYTDExpense = (curTmp);

                CurrencyHelper.DoubleToCurrency(dActualDeprAmt, out curTmp);
                pObjPDItem.DeprAmount = (curTmp);

                CurrencyHelper.DoubleToCurrency(deferredYTD, out curTmp);
                pObjPDItem.EndDateYTDDeferred = (curTmp);

                CurrencyHelper.DoubleToCurrency(dAnnualPersonalAmount + dStartingPersonalAmount, out curTmp);
                pObjPDItem.EndDateYTDPersonalUse = (curTmp);
                pObjPDItem.PersonalUseAmount = (dAnnualPersonalAmount);

                pObjPDItem.CalcFlags = (szCalcFlags);
                if (!(hr = FixForAdjustments(pObjPDItem, pColPItemList, dtAdjStart, dtAdjEnd,
                                        dAdjustAmount, dStartingPersonalAmount, pObjStartPoint, out adjApplied)))
                    return hr;

                //
                // If the end date is before the end of the year, put out another
                // item to finish the year.
                //
                pObjPDItem = null;
                DateTime dtTmp;
                dtTmp = pColPItemList[pColPItemList.Count - 1].EndDate;
                dtEDate = dtTmp;
                if (dtEDate < dtFYEndDate)
                {
                    decimal cyActualDeprTaken;
                    DateTime lastEndDate;

                    //pObjPDItem = null;

                    lastEndDate = pColPItemList[pColPItemList.Count - 1].EndDate;
                    cyActualDeprTaken = (decimal)pColPItemList[pColPItemList.Count - 1].EndDateYTDExpense;
                    //pObjPDItem = null;

                    if (lastEndDate == dtFYEndDate)		// KENT fix JL Ru - 00061
                        return true;


                    obj = new PeriodDeprItem();
                    pObjPDItem = (IBAPeriodDeprItem)obj;

                    pObjPDItem.StartDate = (dtEDate.AddDays(1));
                    pObjPDItem.EndDate = (dtFYEndDate);
                    iFYNum = pObjIFY.FYNum;
                    pObjPDItem.FYNum = (iFYNum);
                    pObjIFY.GetPeriodWeights(dtEDate.AddDays(1), dtFYEndDate, out iPdWeights);
                    pObjPDItem.TotalPeriodWeights = (iPdWeights);
                    pObjPDItem.EntryType = (PERIODDEPRITEM_ENTRYTYPE.PERIODDEPRITEM_NORMAL);

                    CurrencyHelper.DoubleToCurrency(dBeginYearAccumAmt, out curTmp);
                    pObjPDItem.BeginYearAccum = (curTmp);
                    pObjPDItem.EndDateBeginYearAccum = (curTmp);

                    CurrencyHelper.DoubleToCurrency(deferredAccum + deferredYTD, out curTmp);
                    pObjPDItem.EndDateDeferredAccum = (curTmp);

                    CurrencyHelper.DoubleToCurrency(dTotalPersonalAmount, out curTmp);
                    pObjPDItem.EndDatePersonalUseAccum = (curTmp);

                    CurrencyHelper.DoubleToCurrency(dAnnualPersonalAmount + dStartingPersonalAmount, out curTmp);
                    pObjPDItem.EndDateYTDPersonalUse = (curTmp);

                    if (adjApplied)
                        dActualDeprAmt += dAdjustAmount;

                    pObjPDItem.BeginYTDExpense = (cyActualDeprTaken);

                    CurrencyHelper.DoubleToCurrency(0, out curTmp);
                    pObjPDItem.DeprAmount = (curTmp);
                    pObjPDItem.EndDateYTDDeferred = (curTmp);
                    pObjPDItem.CalcFlags = (szCalcFlags);
                    if (!(hr = FixForAdjustments(pObjPDItem, pColPItemList, dtAdjStart,
                                                   dtAdjEnd, dAdjustAmount, dStartingPersonalAmount, pObjStartPoint, out adjApplied)))
                        return hr;

                }
            }

            return true;
        }


        public bool GeneratePDItemTrailer(IBAFiscalYear pObjIFY,
                                        double dblBeginYearAccumAmt,
                                        List<IBAPeriodDeprItem> pColPItemList,
                                        string szCalcFlags,
                                        double deferredAccum,
                                        DateTime dtAdjPeriodStart,
                                        double dAdjustAmount)
        {
            IBAPeriodDeprItem pObjPDItem;
            decimal curTmp;
            DateTime dtSDate;
            DateTime dtEDate;
            DateTime dtAdjStart = DateTime.MinValue;
            DateTime dtAdjEnd = DateTime.MinValue;
            short iFYNum = 0;
            short iWgt;
            bool hr;
            PeriodDeprItem obj;
            bool adjApplied = false;

            if (pColPItemList == null)
                return false;

            if (pObjIFY == null)
                return false;
            
            obj = new PeriodDeprItem();
            pObjPDItem = (IBAPeriodDeprItem) obj;
            dtSDate = pObjIFY.YREndDate;
            iFYNum = pObjIFY.FYNum;
            pObjIFY.GetTotalFiscalYearPeriodWeights(out iWgt);

            dtEDate = new DateTime(3000,1,1);
            dtSDate = dtSDate.AddDays(1);

            if ( dtSDate <= dtAdjPeriodStart && dtEDate >= dtAdjPeriodStart )
            {
                IBACalcPeriod pObjAdjPeriod;

                //
                // We have an adjustment in this year, therefore determine the start and
                // end period dates.
                //
                pObjIFY.GetPeriod(dtAdjPeriodStart, out pObjAdjPeriod);
                dtAdjStart = pObjAdjPeriod.PeriodStart;
                dtAdjEnd = pObjAdjPeriod.PeriodEnd;
            }

            if( iWgt == 0 )
                return false;

            {
                long count = pColPItemList.Count;

                if (count > 0)
                {
                    dtSDate = pColPItemList[pColPItemList.Count-1].EndDate;
                    dtSDate = dtSDate.AddDays(1);
                }
            }

            //use the input infor to build the pd item, only one item
            pObjPDItem.StartDate = dtSDate;
            pObjPDItem.EndDate = dtEDate;
            pObjPDItem.FYNum = Convert.ToInt16(iFYNum + 1);
            pObjPDItem.TotalPeriodWeights = 0;
            pObjPDItem.EntryType = PERIODDEPRITEM_ENTRYTYPE.PERIODDEPRITEM_NORMAL;

            CurrencyHelper.DoubleToCurrency(dblBeginYearAccumAmt, out curTmp);
            pObjPDItem.BeginYearAccum = curTmp;
            pObjPDItem.EndDateBeginYearAccum = curTmp;

            CurrencyHelper.DoubleToCurrency(deferredAccum, out curTmp);
            pObjPDItem.EndDateDeferredAccum = curTmp;

            CurrencyHelper.DoubleToCurrency(0, out curTmp);
            pObjPDItem.BeginYTDExpense = curTmp;

            CurrencyHelper.DoubleToCurrency(0, out curTmp);
            pObjPDItem.DeprAmount = curTmp;

            CurrencyHelper.DoubleToCurrency(0, out curTmp);
            pObjPDItem.EndDateYTDDeferred = curTmp;

            pObjPDItem.CalcFlags = szCalcFlags;

            if ( !(hr = FixForAdjustments (pObjPDItem, pColPItemList, dtAdjStart, dtAdjEnd,
                                    dAdjustAmount, 0, null, out adjApplied)) )
                return hr;
           
            return true;
        }

        bool FixForAdjustments(IBAPeriodDeprItem pObjPDItem,
                                        List<IBAPeriodDeprItem> pObjItemList,
                                        DateTime dtAdjStart,
                                        DateTime dtAdjEnd,
                                        double dAdjAmount,
                                        double dStartingPersonalAmount,
                                        IBAPeriodDeprItem pObjStartPoint,
                                        out bool adjApplied)
        {
            bool hr;
            double Life;
            DateTime periodStart;
            DateTime periodEnd;
            IBAPeriodDeprItem left;
            IBAPeriodDeprItem right;
            IBAPeriodDeprItem begRight;
            decimal cyAdjAmount;
            decimal cyDeprAmount = 0; ;
            bool bProcessingSpecialLuxAuto = false;
            string flags;

            adjApplied = false;

            if ( pObjPDItem == null || pObjItemList == null )
                return false;

            periodStart = pObjPDItem.StartDate;
            periodEnd = pObjPDItem.EndDate;
            flags = pObjPDItem.CalcFlags;
            Life = m_pObjSchedule.DeprLife;

            bProcessingSpecialLuxAuto = (flags != null && flags.Length>=1 && (flags)[0] != 0 && (flags.Contains('A')));
            if ( bProcessingSpecialLuxAuto )
            {
                if ( flags[(flags.Length) - 1] == 'A' )
                    flags = "";

                pObjPDItem.CalcFlags = (flags);
            }
            //
            // First set the remaining life field properly.
            //
            if ( periodEnd < m_dtDeemedEndDate )
            {
                Life = (m_dtDeemedEndDate - periodEnd).TotalDays / (m_dtDeemedEndDate - m_dtDeemedStartDate).TotalDays * Life;
            }
            else
            {
                Life = 0;
            }
            
            pObjPDItem.RemainingLife= (Life);

            //
            // If there is a starting point, and it is within this period depr item, adjust the 
            // current period depr item for it.
            //
            if ( pObjStartPoint != null)
            {
                DateTime startDate;
                DateTime prevStartDate;
                IBACalendar pObjCalendar;
                IBAFiscalYear pObjFY;
                DateTime YearEnd;
                decimal cyPeriodDepr;

                prevStartDate = pObjStartPoint.StartDate;
                startDate = pObjStartPoint.EndDate;
                cyPeriodDepr = (decimal) pObjStartPoint.EndPeriodAccum;
                pObjCalendar = m_pObjSchedule.Calendar;

                if ( startDate > DateTime.MinValue && (startDate > m_dtPISDate || CurrencyHelper.CurrencyToDouble(cyPeriodDepr) > 0))
                {
                    if ( !(hr = pObjCalendar.GetFiscalYear(startDate, out pObjFY)) )
                        return hr;

                    YearEnd = pObjFY.YREndDate;
                    // If true, we do not need this pd item.  Therefore do not save it.
                    //SAI	If the begin End Date is after the Deemed End Date, reset period depr item and return
                    //Fixed bug RMGr-00199, Adoni-00005
                    if ( startDate >= periodEnd )
                    {
                        if (Life != 0 )
                            return true;

                        decimal                        cyDeprAmt;
                        decimal                        cyAdjustAmount;
                        decimal                        cyTmp1;
                        decimal                        cyTmp2;

                        pObjPDItem.StartDate = (startDate.AddDays(1));
                        pObjPDItem.EndDate = (YearEnd);

                        cyDeprAmount = pObjPDItem.DeprAmount;
                        cyTmp1 = pObjPDItem.BeginYTDExpense;
                        cyAdjustAmount = pObjPDItem.AdjustAmount;

                        CurrencyHelper.DoubleToCurrency(CurrencyHelper.CurrencyToDouble(cyTmp1) + CurrencyHelper.CurrencyToDouble(cyDeprAmount) +
                                CurrencyHelper.CurrencyToDouble(cyAdjustAmount), out cyTmp1);
                        CurrencyHelper.DoubleToCurrency(0.0, out cyTmp2);

                        // START KENT to fix JL RU -00047
                        // startDate is the prev perioditem end date
                        // modify the start date of the current perioditem(pObjPDItem) by startDate + 1
                        // if startDate != YearEnd, that means the pObjPDItem and pObjStartPoint
                        // are in the same year then the BeginYTDExpense of the current pd item is 
                        // the PeriodExpense of the prev pditem.
                        if ( startDate != YearEnd )	
                        {
                            cyTmp1 = pObjStartPoint.BeginYearAccum;
                            CurrencyHelper.DoubleToCurrency(CurrencyHelper.CurrencyToDouble(cyPeriodDepr) - CurrencyHelper.CurrencyToDouble(cyTmp1), out cyTmp1);
                            pObjPDItem.BeginYTDExpense = (cyTmp1); 
                            pObjPDItem.DeprAmount = (cyTmp2);
                            pObjPDItem.AdjustAmount = (cyTmp2);
                        }
                        else // End KENT 
                        {
                            pObjPDItem.BeginYTDExpense = (cyTmp1); 
                            pObjPDItem.DeprAmount = (cyTmp2);
                            pObjPDItem.AdjustAmount = (cyTmp2);
                        }
                        //deferred stuff here
                        cyTmp1 = pObjPDItem.EndDateDeferredAccum;
                        cyTmp2 = pObjPDItem.EndDateYTDDeferred;

                        CurrencyHelper.DoubleToCurrency(CurrencyHelper.CurrencyToDouble(cyTmp1) + CurrencyHelper.CurrencyToDouble(cyTmp2), out cyTmp1);
                        CurrencyHelper.DoubleToCurrency(0.0, out cyTmp2);

                        pObjPDItem.EndDateDeferredAccum = (cyTmp1);
                        pObjPDItem.EndDateYTDDeferred = (cyTmp2);;
                        pObjPDItem.EndDateYTDPersonalUse = (cyTmp2);
                        pObjPDItem.PersonalUseAmount = (0.0);

                        return AppendPeriodDeprItem(pObjItemList,pObjPDItem);
                    }

                    startDate = startDate.AddDays(1);

                    if ( ((prevStartDate == startDate.AddDays(-1) && startDate >= periodStart && prevStartDate != YearEnd ) || 
                            (startDate > periodStart)) && 
                         startDate < periodEnd )

                    {
                        DeprAllocator obj;
                        decimal cyBegAccum;
                        decimal cyEndDateAccum;
                        decimal cystartAccum;
                        decimal cystartYTD;
                        decimal cyleftPersUse;
                        decimal cyrightPersUse;
                        double dRightPersUse;
                        double dLeftPersUse;
                        short iLeftWeight;
                        short iRightWeight;

                        obj = new DeprAllocator();

                        obj.Calendar = (pObjCalendar);
                        obj.PlacedInService = (m_dtPISDate);
                        obj.DeemedEndDate = (m_dtDeemedEndDate);
                        obj.DisposalDate = (m_dtDispDate);
                       
                        obj.SplitPDItem(pObjPDItem, startDate, out left, out begRight);
                        cystartAccum = pObjStartPoint.EndDateBeginYearAccum;
                        cystartYTD = Convert.ToDecimal (pObjStartPoint.EndDateYTDExpense);
                        cyBegAccum = begRight.BeginYearAccum;
                        cyDeprAmount = begRight.DeprAmount;
                        begRight.BeginYearAccum = cystartAccum;
                        begRight.BeginYTDExpense = cystartYTD;
                        cyleftPersUse = left.EndDateYTDPersonalUse;
                        cyrightPersUse = begRight.EndDateYTDPersonalUse;
                        iLeftWeight = left.TotalPeriodWeights;
                        iRightWeight = begRight.TotalPeriodWeights;

                        dRightPersUse = CurrencyHelper.CurrencyToDouble(cyrightPersUse);
                        dLeftPersUse = CurrencyHelper.CurrencyToDouble(cyleftPersUse);
                        CurrencyHelper.DoubleToCurrency (dRightPersUse - dLeftPersUse + dStartingPersonalAmount * iLeftWeight / (iLeftWeight + iRightWeight), out cyrightPersUse);

                        begRight.EndDateYTDPersonalUse = cyrightPersUse;

                        left = null;
                        obj = null;

                        if ( periodEnd <= YearEnd )
                        {
                            begRight.EndDateBeginYearAccum = cystartAccum;
                        }
                        else
                        {
                            cyEndDateAccum = begRight.EndDateBeginYearAccum;

                            CurrencyHelper.DoubleToCurrency(CurrencyHelper.CurrencyToDouble(cyEndDateAccum) - CurrencyHelper.CurrencyToDouble(cyBegAccum) + CurrencyHelper.CurrencyToDouble(cystartAccum), out cyEndDateAccum);
                            begRight.EndDateBeginYearAccum = cyEndDateAccum;
                        }
				
                        pObjPDItem = begRight;

                        periodStart = pObjPDItem.StartDate;
                        periodEnd = pObjPDItem.EndDate;
                    }
                    else
                    {
                        cyDeprAmount = pObjPDItem.DeprAmount;
                    }
                }
                else
                {
                    cyDeprAmount = pObjPDItem.DeprAmount;
                }
            }
            //
            // If there is no adjustment amount, or the adjustment start or end dates are
            // invalid, just append the period to the list and return.
            //
            if (dtAdjEnd <= DateTime.MinValue || dtAdjStart <= DateTime.MinValue)
            {
                return AppendPeriodDeprItem(pObjItemList,pObjPDItem);
            }
            if (Math.Abs(dAdjAmount) < 0.01 && !bProcessingSpecialLuxAuto)
            {
                return AppendPeriodDeprItem(pObjItemList, pObjPDItem);
            }


            //
            // If the adjustment is not within this period, add the period to the list
            // and return.
            //
            if ( periodStart > dtAdjEnd || periodEnd < dtAdjStart )
            {
                return AppendPeriodDeprItem(pObjItemList,pObjPDItem);
            }

            //ignore adjustment if disposal date falls in adjustment period---Does not seem to work---SAI
	
        //	if (  m_dtDispDate <= dtAdjEnd && m_dtDispDate >= dtAdjStart )
        //	{
        //		return AppendPeriodDeprItem(pObjItemList,pObjPDItem);
        //	}
            //
            // From this point on, we know that the adjustment will happen in this period.
            // Therefore, tell the caller that the adjustment has happened.
            //
            adjApplied = true;
            CurrencyHelper.DoubleToCurrency (dAdjAmount, out cyAdjAmount);

            //
            // Now we need to see if the adjustment period is enclosed in current calc period.  
            // If so add the adjustment to the period, add the period
            // to the list and return.
            //
            if ( periodStart == dtAdjStart && periodEnd >= dtAdjEnd )
            {
                pObjPDItem.AdjustAmount = (cyAdjAmount);
                if ( !(hr = AppendPeriodDeprItem(pObjItemList, pObjPDItem)) )
                    return hr;
                return true;
            }

            DeprAllocator obj1 = new DeprAllocator();

            obj1.Calendar = m_pObjSchedule.Calendar;
            obj1.PlacedInService = (m_dtPISDate);
            obj1.DeemedEndDate = (m_dtDeemedEndDate);
            obj1.DisposalDate = (m_dtDispDate);

            //
            // At this point we have determined that the adjustment occurs within this period and
            // that the period is longer than the adjustment period.
            // Now we need to see which way it applies (beginning, end, or middle).
            //
            if ( periodStart == dtAdjStart )
            { 
                decimal cyTmp;

                //
                // The adjustment occurs in the beginning of this item.  Split this period into two
                // parts and update the left part, because it is the adjustment period.
                //
                if ( !(hr = obj1.SplitPDItem(pObjPDItem, dtAdjEnd.AddDays (1), out left, out right)) )
                    return hr;
                left.AdjustAmount = cyAdjAmount;

                if (bProcessingSpecialLuxAuto)
                {
                    decimal zero;

                    zero = 0;
                    left.AdjustAmount = cyDeprAmount;
                    left.DeprAmount = zero;
                    cyTmp = Convert.ToDecimal(right.EndDateYTDExpense);
                    right.BeginYTDExpense = cyTmp;
                    right.DeprAmount = zero;
                }
        //			 !(hr = left.DeprAmount(&cyTmp)) )
        //			return hr;
        //
        //		DoubleToCurrency(CurrencyToDouble(cyTmp) + dAdjAmount, cyTmp);
        //
        //		if ( !(hr = left.put_DeprAmount(cyTmp)) ||
                if ( left != null )
                {
                    if ( !(hr = AppendPeriodDeprItem(pObjItemList,left)) )
                        return hr;
                }

                if ( right != null )
                {
                    cyTmp = right.BeginYTDExpense;

                    CurrencyHelper.DoubleToCurrency(CurrencyHelper.CurrencyToDouble(cyTmp) + dAdjAmount, out cyTmp);

                    right.BeginYTDExpense = cyTmp;
                    if (!(hr = AppendPeriodDeprItem(pObjItemList, right)) )
                    {
                        obj1 = null;
                        return hr;
                    }
                }
            }
            else if ( periodEnd == dtAdjEnd )
            {
                //		decimal cyTmp;

                //
                // The adjustment occurs in the end of this item.  Split the period into two parts
                // and update the right part, because it is the adjustment period.
                //
                if ( !(hr = obj1.SplitPDItem(pObjPDItem, dtAdjStart, out left, out right)))
                {
                    obj1 = null;
                    return hr;
                }
        //			 !(hr = right.DeprAmount(&cyTmp)) )
        //			return hr;
        //
        //		DoubleToCurrency(CurrencyToDouble(cyTmp) + dAdjAmount, cyTmp);
        //
        //		if ( !(hr = right.put_DeprAmount(cyTmp)) ||
                right.AdjustAmount = cyAdjAmount;
                if ( !(hr = AppendPeriodDeprItem(pObjItemList,left)) ||
                     !(hr = AppendPeriodDeprItem(pObjItemList,right)) )
                {
                    obj1 = null;
                    return hr;
                }
            }
            else
            {
                decimal cyTmp;
                IBAPeriodDeprItem left2;
                IBAPeriodDeprItem right2;

                //
                // The adjustment occurs in the middle of this item.  Split the period into three
                // periods (left, left2, and right2).  right is a temporary period that we should 
                // change or append to the list.  left2 is the period where the adjustment occurs.
                //

                if ( !(hr = obj1.SplitPDItem3ways(pObjPDItem, dtAdjStart, dtAdjEnd.AddDays (1), out left, out left2, out right2)) )
                {
                    obj1 = null;
                    return hr;
                }
        //			 !(hr = left2.DeprAmount(&cyTmp)) )
        //			return hr;
        //
        //		CurrencyHelper.DoubleToCurrency(CurrencyToDouble(cyTmp) + dAdjAmount, cyTmp);
        //
        //		if ( !(hr = left2.put_DeprAmount(cyTmp)) ||
                left2.AdjustAmount = cyAdjAmount;
                if ( !(hr = AppendPeriodDeprItem(pObjItemList,left)) ||
                     !(hr = AppendPeriodDeprItem(pObjItemList,left2)))
                {
                    obj1 = null;
                    return hr;
                }
                cyTmp = right2.BeginYTDExpense;
  
                CurrencyHelper.DoubleToCurrency(CurrencyHelper.CurrencyToDouble(cyTmp) + dAdjAmount, out cyTmp);

                right2.BeginYTDExpense = cyTmp;
                if ( !(hr = AppendPeriodDeprItem(pObjItemList,right2)) )
                {
                    obj1 = null;
                    return hr;
                }
                if (bProcessingSpecialLuxAuto)
                {
                    decimal zero;

                    zero = 0;
                    left2.AdjustAmount = cyDeprAmount;
                    left2.DeprAmount = zero;
                    cyTmp = Convert.ToDecimal (right2.EndDateYTDExpense);
                    right2.BeginYTDExpense = cyTmp;
                    right2.DeprAmount = zero;
                }
            }
            obj1 = null;
            flags = string.Empty;

            return true;
        }

 
        bool AppendPeriodDeprItem(List<IBAPeriodDeprItem> pColList, IBAPeriodDeprItem newItem)
        {
            return AppendPeriodDeprItem(pColList, newItem, false);
        }

        bool AppendPeriodDeprItem(List<IBAPeriodDeprItem> pColList,IBAPeriodDeprItem newItem, bool disableLimitCheck/*=false*/)
        {
	        double maxTotalDepr = 0.0;
	        double endTotalPeriodAccum = 0.0;
	        double endTotalPeriodDepr = 0.0;
	        double deprAmount = 0.0;
	        double adjustAmount = 0.0;
	        double dPersonalDeprAmount = 0.0;
	        double endTotalYTDExpense = 0.0;
	        double dAmt = 0;

	        decimal cyEndPeriodAccum;
	        decimal cyEndPersonalUseAccum;
	        decimal cyEndYTDPersonalUse;
	        decimal cyDeprAmount;
	        decimal cyAdjustAmount;
	        decimal cyYTDExpense;

	        IBAPeriodDeprItem pObjLastPDItem;
	        IBAPeriodDeprItem pdItem1 = null;
            IBAPeriodDeprItem pdItem2 = null;
	        short lastFYNum;
	        short newFYNum;
	        int count = 0;
	        decimal curTmp1;
	        decimal curTmp2;
	        decimal cyZero;
	        bool hr;

        	CurrencyHelper.DoubleToCurrency(0.0, out cyZero);

        	count = pColList.Count;

	        if (count > 0)
	        {
                pObjLastPDItem = pColList[count - 1];
                lastFYNum = pObjLastPDItem.FYNum;
                newFYNum = newItem.FYNum;

		        if (lastFYNum == newFYNum)
		        {
			        curTmp1 = pObjLastPDItem.EndDateBeginYearAccum;
			        newItem.EndDateBeginYearAccum = (curTmp1);
				    newItem.BeginYearAccum = (curTmp1);
				    cyEndYTDPersonalUse = pObjLastPDItem.EndDateYTDPersonalUse;
				    dPersonalDeprAmount = newItem.PersonalUseAmount;
				    curTmp2 = (decimal) pObjLastPDItem.YTDExpense;
				    newItem.BeginYTDExpense = (curTmp2);

			        CurrencyHelper.DoubleToCurrency(CurrencyHelper.CurrencyToDouble(cyEndYTDPersonalUse)+dPersonalDeprAmount,out cyEndYTDPersonalUse);

			        newItem.EndDateYTDPersonalUse = (cyEndYTDPersonalUse);
		        }
		        else if (lastFYNum + 1 == newFYNum)
		        {
			        curTmp1 = (decimal) pObjLastPDItem.EndPeriodAccum;
			        newItem.EndDateBeginYearAccum = (curTmp1);
			        newItem.BeginYearAccum = (curTmp1);
			        cyEndYTDPersonalUse = pObjLastPDItem.EndDateYTDPersonalUse;
			        cyEndPersonalUseAccum = pObjLastPDItem.EndDatePersonalUseAccum;
			        dPersonalDeprAmount = newItem.PersonalUseAmount;
			        newItem.BeginYTDExpense = (cyZero);

			        CurrencyHelper.DoubleToCurrency(CurrencyHelper.CurrencyToDouble(cyEndPersonalUseAccum)+CurrencyHelper.CurrencyToDouble(cyEndYTDPersonalUse),
								        out cyEndPersonalUseAccum);
			        CurrencyHelper.DoubleToCurrency(dPersonalDeprAmount,out cyEndYTDPersonalUse);

                    newItem.EndDatePersonalUseAccum = (cyEndPersonalUseAccum);
			        newItem.EndDateYTDPersonalUse = (cyEndYTDPersonalUse);
		        }
		        else
		        {
			        //we should not come here
		        }

		        if (pObjLastPDItem != null)
		        {
                    pObjLastPDItem = null;
		        }
	        }

	        maxTotalDepr = m_pObjDeprMethod.TotalDepreciationAllowed;
            cyEndPeriodAccum = (decimal)newItem.EndPeriodAccum;
            cyYTDExpense = (decimal)newItem.YTDExpense;
	        cyEndYTDPersonalUse = newItem.EndDateYTDPersonalUse;
	        cyEndPersonalUseAccum = newItem.EndDatePersonalUseAccum;
	        dPersonalDeprAmount = newItem.PersonalUseAmount;
	        cyDeprAmount = newItem.DeprAmount;
	        cyAdjustAmount = newItem.AdjustAmount;

            if (Fix168KAuto && (m_eLFlag == LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_LUXURYCAR || m_eLFlag == LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_LTTRUCKSANDVANS)) 
		        maxTotalDepr = maxTotalDepr + PostUsageDeduction - AutoPostUsageDeduction;

	        endTotalYTDExpense = CurrencyHelper.CurrencyToDouble(cyYTDExpense) + CurrencyHelper.CurrencyToDouble(cyEndYTDPersonalUse);
	        endTotalPeriodAccum = CurrencyHelper.CurrencyToDouble(cyEndPeriodAccum) + 
						          CurrencyHelper.CurrencyToDouble(cyEndPersonalUseAccum) +
						          CurrencyHelper.CurrencyToDouble(cyEndYTDPersonalUse);
	        endTotalPeriodDepr = CurrencyHelper.CurrencyToDouble(cyDeprAmount) + dPersonalDeprAmount;
	        deprAmount = CurrencyHelper.CurrencyToDouble(cyDeprAmount);
	        adjustAmount = CurrencyHelper.CurrencyToDouble(cyAdjustAmount);

            if (disableLimitCheck == false && m_eLFlag != LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_NOTAPPLY && m_dAutoLimit > 0.0)
	        {
		        if (Math.Abs(endTotalYTDExpense - m_dAutoLimit) < 1.0)
		        {
			        //don't do anything
		        }
		        else if (endTotalYTDExpense > m_dAutoLimit)
		        {
			        double beginTotalYTDExpense = endTotalYTDExpense - endTotalPeriodDepr - adjustAmount;

			        if (beginTotalYTDExpense + adjustAmount >= m_dAutoLimit - 1.0)
			        {
				        deprAmount = 0;
				        dPersonalDeprAmount = 0.0;
				        adjustAmount = m_dAutoLimit - beginTotalYTDExpense;
				        CurrencyHelper.DoubleToCurrency(deprAmount, out cyDeprAmount);
				        CurrencyHelper.DoubleToCurrency(adjustAmount, out cyAdjustAmount);
                        CurrencyHelper.DoubleToCurrency(CurrencyHelper.CurrencyToDouble(cyEndYTDPersonalUse) - dPersonalDeprAmount, 
									        out cyEndYTDPersonalUse);

                        //update all the ones modified above

                        newItem.DeprAmount = (cyZero);
                        newItem.PersonalUseAmount = (0.0);
                        newItem.EndDateYTDPersonalUse = (cyEndYTDPersonalUse);
                        newItem.AdjustAmount = (cyAdjustAmount);

                        //reget everything back to local variables and recalculate totals again

                        cyEndPeriodAccum = (decimal) newItem.EndPeriodAccum;
                        cyYTDExpense = (decimal)newItem.YTDExpense;
                        cyEndYTDPersonalUse = newItem.EndDateYTDPersonalUse;
                        cyEndPersonalUseAccum = newItem.EndDatePersonalUseAccum;
                        dPersonalDeprAmount = newItem.PersonalUseAmount;
                        cyDeprAmount = newItem.DeprAmount;
                        cyAdjustAmount = newItem.AdjustAmount;

				        endTotalYTDExpense = CurrencyHelper.CurrencyToDouble(cyYTDExpense) + CurrencyHelper.CurrencyToDouble(cyEndYTDPersonalUse);
				        endTotalPeriodAccum = CurrencyHelper.CurrencyToDouble(cyEndPeriodAccum) + 
									          CurrencyHelper.CurrencyToDouble(cyEndPersonalUseAccum) +
									          CurrencyHelper.CurrencyToDouble(cyEndYTDPersonalUse);
				        endTotalPeriodDepr = CurrencyHelper.CurrencyToDouble(cyDeprAmount) + dPersonalDeprAmount;
				        deprAmount = CurrencyHelper.CurrencyToDouble(cyDeprAmount);
				        adjustAmount = CurrencyHelper.CurrencyToDouble(cyAdjustAmount);
			        }
			        else
			        {
				        double remainingDepr = (m_dAutoLimit - (beginTotalYTDExpense + adjustAmount));
                        SplitLastPeriodDeprItem(remainingDepr, ref newItem, out pdItem1, out pdItem2);
				        if (!(hr = AppendPeriodDeprItem(pColList,newItem,true)))
					        return hr;

				        if (pdItem1 != null )
				        {
					        if (!(hr = AppendPeriodDeprItem(pColList,pdItem1,true)))
						        return hr;
				        }
				        if (pdItem2 != null )
				        {
					        if (!(hr = AppendPeriodDeprItem(pColList,pdItem2,true)))
						        return hr;
				        }
				        return true;
			        }
		        }
	        }

	        if (endTotalYTDExpense > 10)
		        dAmt = 1;
	        else
		        dAmt = .1;

	        if (endTotalPeriodAccum > maxTotalDepr + 0.019)
	        {

		        if (endTotalPeriodDepr >= endTotalPeriodAccum - maxTotalDepr)
		        {
			        endTotalPeriodDepr -= (endTotalPeriodAccum - maxTotalDepr);
                    SplitLastPeriodDeprItem(endTotalPeriodDepr, ref newItem, out pdItem1, out pdItem2);
		        }
		        else
		        {
			        //Not sure what to do. This case should not happen unless 
			        // asset was overdepreciated in the previous run....
			        // treat the overdepreciated amount as negative adjustment
			        adjustAmount -= (endTotalPeriodAccum - maxTotalDepr - endTotalPeriodDepr);
			        deprAmount = 0.0;
			        CurrencyHelper.DoubleToCurrency(deprAmount, out cyDeprAmount);
			        CurrencyHelper.DoubleToCurrency(adjustAmount, out cyAdjustAmount);
                    CurrencyHelper.DoubleToCurrency(CurrencyHelper.CurrencyToDouble(cyEndYTDPersonalUse) - dPersonalDeprAmount, 
									        out cyEndYTDPersonalUse);

                    newItem.DeprAmount = (cyZero);
                    newItem.PersonalUseAmount = (0.0);
                    newItem.EndDateYTDPersonalUse = (cyEndYTDPersonalUse);
                    newItem.AdjustAmount = (cyAdjustAmount);
		        }

	        }
	        else if (endTotalPeriodAccum > maxTotalDepr - dAmt)
	        {
		        //adjust only Depr Amount, not personal or adjustment amount
		        deprAmount += (maxTotalDepr - endTotalPeriodAccum);
		        CurrencyHelper.DoubleToCurrency(deprAmount, out cyDeprAmount);

		        newItem.DeprAmount = (cyDeprAmount);
	        }

	        pColList.Add(newItem);

	        if (pdItem1 != null )
	        {
                pColList.Add(pdItem1);
	        }
	        if (pdItem2 != null )
	        {
                pColList.Add(pdItem2);
	        }
	        return true;
        }

        bool SplitLastPeriodDeprItem(double remainingAmt, ref IBAPeriodDeprItem newItem,
                                                          out IBAPeriodDeprItem pditem1,
                                                          out IBAPeriodDeprItem pditem2)
        {
	        IBACalcPeriod   pObjPeriod;
	        DateTime	dtStart;
	        DateTime	dtEnd;
            decimal     cyDeprAmount = 0;
            decimal     cyPersonalYTDAmount = 0;
            decimal     cyYtdExp = 0;
            decimal     cyZero = 0;
	        double      dDeprAmount;
	        double      dPersonalDeprAmount;
	        double      dPersonalYTDAmount;
	        double		dTotalDeprAmount;
	        double		dCumulativeAmt = 0;
	        short		iPdWeights;
	        short		iCumPdWeights = 0;
	        short		iTotalPeriodWeights;
	        short       iPdNum;
	        short       iNumPeriods;
	        short		iCount = 0;
	        short		noOfItems = 0;
            double      item1TotalDepr = 0.0;
            double      item1PersonalYTD = 0.0;

	        DateTime    item1EndDate = DateTime.MinValue;

            double      item2TotalDepr = 0.0;
            DateTime    item2StartDate = DateTime.MinValue;
            DateTime    item2EndDate = DateTime.MinValue;
            DateTime    lastItemStartDate = DateTime.MinValue;
            DateTime    dtFYEnd = DateTime.MinValue;

	        bool		hr;

            pditem1 = null;
            pditem2 = null;

 	        CurrencyHelper.DoubleToCurrency(0.0, out cyZero);

	        dtStart = newItem.StartDate;
	        dtEnd = newItem.EndDate;
	        cyDeprAmount = newItem.DeprAmount;
	        dPersonalDeprAmount = newItem.PersonalUseAmount;
	        cyPersonalYTDAmount = newItem.EndDateYTDPersonalUse;
	        iTotalPeriodWeights = newItem.TotalPeriodWeights;

		    if ( !(hr = m_pObjIFY.GetPeriod(dtStart, out pObjPeriod)) )
                return false;

		    dtFYEnd = m_pObjIFY.YREndDate;
		    m_pObjIFY.GetNumPeriods(out iNumPeriods);
		    iPdNum = pObjPeriod.PeriodNum;

	        dDeprAmount = CurrencyHelper.CurrencyToDouble(cyDeprAmount);
	        dPersonalYTDAmount = CurrencyHelper.CurrencyToDouble(cyPersonalYTDAmount);
	        dTotalDeprAmount = dDeprAmount + dPersonalDeprAmount;
					   
	        //defensive check
	        if ( dTotalDeprAmount <= remainingAmt )
		        return true;

	        do
	        {
		        DateTime periodEnd;
		        DateTime periodStart;

		        pObjPeriod = null;
		        if (!(hr = m_pObjIFY.GetPeriodByNum(iPdNum, out pObjPeriod)) )
                    return false;

			    periodEnd = pObjPeriod.PeriodEnd;
			    periodStart = pObjPeriod.PeriodStart;
			    iPdWeights = pObjPeriod.Weight;

		        dCumulativeAmt = dTotalDeprAmount * (double)(iCumPdWeights + iPdWeights) /
											        (double)iTotalPeriodWeights ;

		        if (dCumulativeAmt > remainingAmt)
		        {
			        if (iCumPdWeights == 0)
			        {
				        //create one pd item with the entire remaining amount
				        noOfItems = 1;
				        item1TotalDepr = remainingAmt;
				        item1EndDate = periodEnd;

				        break;
			        }
			        else
			        {
				        //create two pd items
				        noOfItems = 2;
                        item1TotalDepr = dTotalDeprAmount * (double)(iCumPdWeights) / (double)iTotalPeriodWeights;
				        item1TotalDepr = CurrencyHelper.FormatCurrency(item1TotalDepr);
				        item1EndDate = periodStart.AddDays(-1);

				        item2TotalDepr = remainingAmt - item1TotalDepr;
				        item2StartDate = periodStart;
				        item2EndDate = periodEnd;

				        break;
			        }
		        }
		        else if (dCumulativeAmt > remainingAmt - 1.0)
		        {
			        //create one pd item with the entire remaining amount
			        noOfItems = 1;
			        item1TotalDepr = remainingAmt;
			        item1EndDate = periodEnd;

			        break;
		        }
		
		        iCumPdWeights += iPdWeights;
		        iPdNum ++;
	        } while ( (iCumPdWeights < iTotalPeriodWeights) && (iPdNum <= iNumPeriods));

	        pObjPeriod = null;

	        if (noOfItems == 0)
	        {
		        return true; //no changes
	        }

	        if (noOfItems >= 1)
	        {
                double item1Depr = 0.0;
                double item1PersonalDepr = 0.0;

		        if (dPersonalDeprAmount == 0.0)
		        {
			        item1Depr = item1TotalDepr;
			        item1PersonalDepr = 0.0;
		        }
		        else
		        {
			        item1Depr = item1TotalDepr * dDeprAmount / (dDeprAmount+dPersonalDeprAmount);
			        item1Depr = CurrencyHelper.FormatCurrency(item1Depr);
			        item1PersonalDepr = item1TotalDepr - item1Depr;
		        }
		        item1PersonalYTD = dPersonalYTDAmount - dPersonalDeprAmount + item1PersonalDepr;

		        CurrencyHelper.DoubleToCurrency(item1Depr, out cyDeprAmount);
                CurrencyHelper.DoubleToCurrency(item1PersonalYTD, out cyPersonalYTDAmount);

                if (!(hr = m_pObjIFY.GetPeriodWeights(dtStart, item1EndDate, out iPdWeights)) )
                    return false;

                newItem.EndDate = (item1EndDate);
                newItem.DeprAmount = (cyDeprAmount);
                newItem.PersonalUseAmount = (item1PersonalDepr);
                newItem.EndDateYTDPersonalUse = (cyPersonalYTDAmount);
                newItem.TotalPeriodWeights = (iPdWeights);
                cyYtdExp = (decimal) newItem.YTDExpense;

		        lastItemStartDate = item1EndDate.AddDays( +1);

	        } 
	
	        if (noOfItems == 2)
	        {
                if (!(hr = newItem.Clone(out pditem1)))
			        return hr;

		        double item2Depr;
		        double item2PersonalDepr;
		        double item2PersonalYTD;

		        if (dPersonalDeprAmount == 0.0)
		        {
			        item2Depr = item2TotalDepr;
			        item2PersonalDepr = 0.0;
		        }
		        else
		        {
			        item2Depr = item2TotalDepr * dDeprAmount / (dDeprAmount+dPersonalDeprAmount);
                    item2Depr = CurrencyHelper.FormatCurrency(item2Depr);
			        item2PersonalDepr = item2TotalDepr - item2Depr;
		        }

		        item2PersonalYTD = item1PersonalYTD + item2PersonalDepr;

                CurrencyHelper.DoubleToCurrency(item2Depr, out cyDeprAmount);
                CurrencyHelper.DoubleToCurrency(item2PersonalYTD, out cyPersonalYTDAmount);

                if (!(hr = m_pObjIFY.GetPeriodWeights(item2StartDate, item2EndDate, out iPdWeights)) )
                    return hr;

                pditem1.StartDate = (item2StartDate);
                pditem1.EndDate = (item2EndDate);
                pditem1.DeprAmount = (cyDeprAmount);
                pditem1.AdjustAmount = (cyZero);
                pditem1.PersonalUseAmount = (item2PersonalDepr);
                pditem1.EndDateYTDPersonalUse = (cyPersonalYTDAmount);
                pditem1.BeginYTDExpense = (cyYtdExp);
                cyYtdExp = (decimal) pditem1.YTDExpense;
                pditem1.TotalPeriodWeights = (iPdWeights);

                lastItemStartDate = item2EndDate.AddDays(+1);
	        }

            //if the lastItemSTartDate is within the current fiscal year
            //add dummy period depr item till the end of original period
            if (lastItemStartDate < dtFYEnd)
            {
                if (pditem1 == null)
                {
                    if (!(hr = newItem.Clone(out pditem2)) )
                        return hr;
                }
                else
                {
                    if (!(hr = pditem1.Clone(out pditem2)))
                        return hr;
                }

                if (!(hr = m_pObjIFY.GetPeriodWeights(lastItemStartDate, dtEnd, out iPdWeights)) )
                    return hr;

                pditem2.StartDate = (lastItemStartDate);
                pditem2.EndDate = (dtEnd);
                pditem2.DeprAmount = (cyZero);
                pditem2.AdjustAmount = (cyZero);
                pditem2.PersonalUseAmount = (0.0);
                pditem2.BeginYTDExpense = (cyYtdExp);
                pditem2.TotalPeriodWeights = (iPdWeights);
            }

	        return true;
        }
    }
}
