using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFABusinessTypes;
using SFACalendar;

namespace SFACalcEngine
{
    public enum PropType
    {
        PersonalGeneral = 0,
        PersonalListed,
        VintageAccount,
        RealGeneral,
        RealListed,
        RealConservation,
        RealEnergy,
        RealFarms,
        Automobile,
        RealLowIncomeHousing,
        Amortizable,
        Depreciable,
        NonDepreciable,
        LtTrucksAndVans
    };

    public enum iTCCodes 
    {
        NEW_PROP_FULL = 1,
        NEW_PROP_RED,
        USED_PROP_FULL,
        USED_PROP_RED,
        THIRTY_YR_RE,
        FORTY_YR_RE,
        CERT_HISTORIC,
        NON_HISTORIC,
        BIOMASS_PROP,
        INTER_CITY_BUS,
        HYDRO_GEN_PLANT,
        OCEAN_THRM_PROP,
        SOLAR_ENERGY,
        WIND_PROP,
        GEO_THRM_PROP,
        ITCP,
        ITCQ,
        ITCR,
        NO_ITC,
        ITCS,
        ITCT,
        ITCU,
        ITCV,
        ITCW,
        ITCY           
    }

    public class BAFASDeprScheduleItem : IBADeprScheduleItem
    {
        double m_dblSection179;
        DateTime m_dtPlacedInServiceDate;
        DateTime m_dtDisposalDate;
        DateTime m_dtDeemStartDate;
        DateTime m_dtDeemEndDate;
        double m_dblDeprLife;
        short m_iDeprPercent;
        bool m_bDBSwitch;
        bool m_bBuildAllPDItems;
        string m_szDeprMethod;
        string m_szAvgConvention;
        SFACalendar.IBACalendar m_Calendar;
        double m_dblSalvageDeduction;
        double m_dblBasisAdjustment;
        double m_dblSection179A;
        double m_dblElectricCar;
        double m_dblITCBasisReduction;
        bool m_bTestMode;
        bool m_RVCalc;

        short m_PropertyType;
        bool m_BonusFlag;
        double m_BonusAmount;
        DateTime m_LastCalcDate;
        bool m_isMidQtrUsedDefault;
        bool m_isMidQtr;
        LUXURYLIMIT_TYPE m_luxuryLimit;

        short m_Bonus911Percent;
        double m_Bonus911Amount;
        double m_stored168KAmt;
        short m_ZoneCode;

        double m_ClassLife;
        bestAssetImportCodeEnum m_ImportCode;
        bestAssetITCTypeEnum m_ITCCode;
        bestAssetDeprAdjustmentConventionEnum m_ApplyAdjustment;
        bpblBookTypeEnum m_bookType;
        double m_AcquisitionValue;
        double m_ITCAmount;
        double m_ITCReduce;
        bestAssetCreationCodeEnum m_creationCode;
        DateTime m_TransferInDate;
        DateTime m_EffectiveTransferInDate;
        DateTime m_TransferOutDate;
        bestAssetActivityTypeEnum m_activityCode;
        bool m_reduceBasisByITC = true;
        //double[MAXPOINTS]                  m_Pcts;
        short m_PctCount;
        short m_PctsUsed;
        List<FasBusinessUse> m_BusUseList = new List<FasBusinessUse>();
        double m_rvLife;
        double m_rvBasis;
        bool m_UsedOutsideUS;
        double m_ACELife;
        decimal m_ACEBasis;
        bool m_IsChildOfTransfer;

        public short PropertyType
        {
            get { return m_PropertyType; }
            set
            {
                m_PropertyType = value;
                if (m_PropertyType == (short)PropType.Automobile)
                {
                    m_luxuryLimit = LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_LUXURYCAR;
                }
                else if (m_PropertyType == (short)PropType.LtTrucksAndVans)
                {
                    m_luxuryLimit = LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_LTTRUCKSANDVANS;
                }
                else
                {
                    m_luxuryLimit = LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_NOTAPPLY;
                }
            }
        }

        public double AcquisitionValue
        {
            get { return m_AcquisitionValue; }
            set { m_AcquisitionValue = value; }
        }

        public bool UseACEHandling
        {
            get
            {
                bool pVal = false; 
                
                if (m_bookType == bpblBookTypeEnum.bpblBookACEBook)
                {
            //        *pVal = VARIANT_TRUE;

		            DateTime				dtACEStart;
		            short					iAceTransitionYear = 0;
		            short					PISYearNumber;
		            IBAFiscalYear         	fy;
		            bool					hr;


		            if (!( hr = m_Calendar.GetFiscalYearNum(m_dtPlacedInServiceDate, out PISYearNumber)))
			            return hr;

		            //
		            // Here we need to determine the year number of the ACE transition.  ACE begins 
		            // on the first year that starts on or after 1/1/1990.
		            //
		            if ( !(hr = m_Calendar.GetFiscalYear(new DateTime(1990, 1,1), out fy)) )
                    {
			            //
			            // If we got here, we can not have ACE transitions.  It probably means that 
			            // the Business started after this date.
			            //
                        iAceTransitionYear = -1;
                    }
		            dtACEStart = fy.YRStartDate;
		            iAceTransitionYear = fy.FYNum;
			
		            //
		            // If the year start is on 1/1/1990, the transition year is one year earlier.
		            //
		            if ( dtACEStart == new DateTime(1990, 1,1) )
			            iAceTransitionYear--;

		            //
		            // Now that we have the transition year number, get the actual ACE start date.
		            //
		            if ( iAceTransitionYear > 0 )
		            {
			            fy = null;
                        if (!(hr = m_Calendar.GetFiscalYearByNum((short)(iAceTransitionYear + 1), out fy)))
				            dtACEStart = dtACEStart.AddDays (-1);
				             dtACEStart = fy.YRStartDate;
		            }
		            //
		            // Now we need to see if we are already in ACE.  If so we need to change the 
		            // depr method accordingly.
		            //
		            if ( m_dtPlacedInServiceDate < new DateTime(1981,1,1) )
		            {
			            // No depr method changes allowed here.
			            pVal = false; // reset the flag because it is not needed.
		            }
		            else if ( PISYearNumber > iAceTransitionYear )
		            {
			            // No changes here, the input D&V should already have handled this.
			            pVal = false; // reset the flag because it is not needed.
		            }
		            else
		            {
			            if ( string.Compare(m_szDeprMethod, "AST") == 0 || string.Compare(m_szDeprMethod, "ASF") == 0 ||
				             string.Compare(m_szDeprMethod, "AT")  == 0 || string.Compare(m_szDeprMethod, "MT")  == 0 ||
				             string.Compare(m_szDeprMethod, "MF")  == 0 || string.Compare(m_szDeprMethod, "MAT") == 0 ||
				             string.Compare(m_szDeprMethod, "MAF") == 0 || string.Compare(m_szDeprMethod, "MST") == 0 ||
				             string.Compare(m_szDeprMethod, "MSF") == 0 )
			            {
				            pVal = true;
			            }
			            else
			            {
				            // Not ACRS or MACRS, therefore ACE handling not needed.
				            pVal = false; // reset the flag because it is not needed.
			            }
		            }

                }
                else
                    pVal = false;
                return pVal;
            }
        }

        public decimal ACEBasis
        {
            get { return m_ACEBasis; }
            set { m_ACEBasis = value; }
        }

        public double ACELife
        {
            get { return m_ACELife; }
            set { m_ACELife = value; }
        }

        public bool Bonus911Flag
        {
            get { return (m_Bonus911Percent > 0) ? true : false; }
            set { throw new NotImplementedException(); }
        }

        public double Stored168KAmount
        {
            get { return m_stored168KAmt; }
            set { m_stored168KAmt = value; }
        }

        public double Section179
        {
            get { return m_dblSection179; }
            set { m_dblSection179 = value; }
        }

        public double Section179A
        {
            get { return m_dblSection179A; }
            set { m_dblSection179A = value; }
        }

        public double ADSLife
        {
            get { return m_ClassLife; }
            set { m_ClassLife = value; }
        }

        public DateTime PlacedInServiceDate
        {
            get { return m_dtPlacedInServiceDate; }
            set {  m_dtPlacedInServiceDate = value; }
        }

        public DateTime DispDate
        {
            get
            {
                if (m_activityCode == bestAssetActivityTypeEnum.acWholeTransferDisposed ||
                    m_activityCode == bestAssetActivityTypeEnum.acPartialTransferDisposed)
                    return DateTime.MinValue;
                else
                    return m_dtDisposalDate;
            }
            set { m_dtDisposalDate = value; }
        }

        public DateTime DeemStartDate
        {
            get { return m_dtDeemStartDate; }
            set { m_dtDeemStartDate = value; }
        }

        public DateTime DeemEndDate
        {
            get { return m_dtDeemEndDate; }
            set { m_dtDeemEndDate = value; }
        }

        public double DeprLife
        {
            get { return m_dblDeprLife; }
            set {  m_dblDeprLife = value; }
        }

        public short DeprPercent
        {
            get { return m_iDeprPercent; }
            set { m_iDeprPercent = value; }
        }

        public bool DBSwitch
        {
            get { return m_bDBSwitch; }
            set { m_bDBSwitch = value; }
        }

        public bool BuildAllPDItems
        {
            get { return m_bBuildAllPDItems; }
            set { m_bBuildAllPDItems = value; }
        }

        public string DeprMethod
        {
            get { return m_szDeprMethod; }
            set 
            {
                m_bDBSwitch = false;
                if (string.Compare(value, "DBs") == 0 || string.Compare(value, "MF") == 0)
                    m_bDBSwitch = true;
                if (string.Compare(m_szDeprMethod, value) != 0)
                    m_szDeprMethod = value; 
            }
        }

        public string AvgConvention
        {
            get { return m_szAvgConvention; }
            set { m_szAvgConvention = value; }
        }

        public bpblBookTypeEnum BookType
        {
            get { return m_bookType; }
            set { m_bookType = value; }
        }

        public SFACalendar.IBACalendar Calendar
        {
            get
            {
                if (m_Calendar == null)
                {
                    m_Calendar = new SFACalendar.BAFASCalendar();
                }
                return m_Calendar;
            }
            set { m_Calendar = null; m_Calendar = value; }

        }

        public double AdjustedCost
        {
            get
            {
                double adjCost;
                double PostUse;

                //    m_helper->CalcBasisComponents(this, adjCost, PostUse);
                CalcBasisComponents(out adjCost, out PostUse);
                return adjCost;
            }
        }

        public double PostUsageDeduction
        {
            get
            {
                double adjCost;
                double PostUse;

                //    m_helper->CalcBasisComponents(this, adjCost, PostUse);
                CalcBasisComponents(out adjCost, out PostUse);
                return PostUse;
            }
        }

        public double SalvageDeduction
        {
            get { return m_dblSalvageDeduction; }
            set { m_dblSalvageDeduction = value; }
        }

        public double BasisAdjustment
        {
            get { return m_dblBasisAdjustment; }
            set { m_dblBasisAdjustment = value; }
        }

        public double ElectricCar
        {
            get { return m_dblElectricCar; }
            set { m_dblElectricCar = value; }
        }

        public double ITCBasisReduction
        {
            get { return m_dblITCBasisReduction; }
            set { m_dblITCBasisReduction = value; }
        }

        public bool BonusFlag 
        {
            get { return m_BonusFlag; }
            set { m_BonusFlag = value; }
        }

        public double BonusAmount 
        {
            get { return m_BonusAmount; }
            set { m_BonusAmount = value; }
        }

        public double Bonus911Amount
        {
            get { return m_Bonus911Amount; }
            set { m_Bonus911Amount = value; }
        }

        public DateTime LastCalcDate 
        { 
            get { return m_LastCalcDate; }
            set { m_LastCalcDate = value; } 
        }

        public bool isMidQtrUsedDefault 
        {
            get { return m_isMidQtrUsedDefault; }
            set { m_isMidQtrUsedDefault = value; }
        }

        public bool isMidQtr 
        {
            get { return m_isMidQtr; }
            set { m_isMidQtr = value; }
        }

        public bestAssetImportCodeEnum ImportCode
        {
            get { return m_ImportCode; }
            set { m_ImportCode = value; }
        }

        public bestAssetITCTypeEnum ITCCode
        {
            get { return m_ITCCode; }
            set { m_ITCCode = value; }
        }

        public bestAssetDeprAdjustmentConventionEnum ApplyAdjustment
        {
            get { return m_ApplyAdjustment; }
            set { m_ApplyAdjustment = value; }
        }

        public double ITCAmount 
        {
            get { return m_ITCAmount; }
            set { m_ITCAmount = value; }
        }

        public double ITCReduce 
        {
            get
            {
                if (m_reduceBasisByITC)
                    return m_ITCReduce;
                else
                    return 0;
            }
            set { m_ITCReduce = value; }
        }

        public short Bonus911Percent
        {
            get { return m_Bonus911Percent; }
            set { m_Bonus911Percent = value; }
        }

        public short ZoneCode
        {
            get { return m_ZoneCode; }
            set { m_ZoneCode = value; }
        }

        public bool VintageAccountFlag
        {
            get
            {
                return (m_PropertyType == 2 ? true : false);
            }
        }

        public bool LowIncomeHousingFlag
        {
            get
            {
                return (m_PropertyType == 9 ? true : false);
            }
        }

        public bool PublicUtilityFlag
        {
            get { return false; }
        }

        public bool PersonalPropertyFlag
        {
            get
            {
                return ((m_PropertyType == 0 || m_PropertyType == 1 || m_PropertyType == 8) ? true : false);
            }
        }

        public bool AddBusinessUseEntry(DateTime fyBegin, double dBusinessUsePct, double dInvestmentUsePct)
        {
            if (m_BusUseList == null)
                throw new Exception("Business use list not initialized.");

            FasBusinessUse buObj = new FasBusinessUse();
            buObj.EffectiveDate = fyBegin;
            buObj.BusinessUsePercent = dBusinessUsePct;
            buObj.InvestmentUsePercent = dInvestmentUsePct;

            int count = m_BusUseList.Count;
            //for (int i = 0; i < count; i++)
            //{
            //    if (m_BusUseList[i].EffectiveDate >= fyBegin)
            //        m_BusUseList.RemoveRange(i, count - 1);
            //}
            //m_BusUseList.Add(buObj);

            for (int i = 0; i < count; i++)
            {
                if (m_BusUseList[i].EffectiveDate > fyBegin)
                {
                    m_BusUseList.Insert(i, buObj);
                    return true;
                }
                else if (m_BusUseList[i].EffectiveDate == fyBegin)
                {
                    m_BusUseList.RemoveAt(i);
                    m_BusUseList.Insert(i, buObj);
                    return true;
                }
            }
            m_BusUseList.Add(buObj);

            return true;
        }

        public  bool GetBusinessUse(DateTime fyEnd, ref double dBusinessUsePct, ref double dInvestmentUsePct)
        {
            FasBusinessUse lastBU = new FasBusinessUse();

            dBusinessUsePct = 1.0;
            dInvestmentUsePct = 0.0;

            if (m_BusUseList == null)
                return true;

            int count = m_BusUseList.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                if (m_BusUseList[i].EffectiveDate <= fyEnd)
                {
                    dBusinessUsePct = m_BusUseList[i].BusinessUsePercent;
                    dInvestmentUsePct = m_BusUseList[i].InvestmentUsePercent;
                    return true;
                }
            }

            return true;
        }

        public void GetPeriodDeprMgr()
        {
            throw new NotImplementedException();
        }

        public void ReleaseDeprMethod()
        {
            throw new NotImplementedException();
        }

        public void ReleaseAvgConvention()
        {
            throw new NotImplementedException();
        }

        public LUXURYLIMIT_TYPE GetLuxuryFlag
        {
            get 
            {
                string deprMethod;

                deprMethod = DeprMethod;

                if ( string.Compare(deprMethod, "MT") == 0 || string.Compare(deprMethod, "MF") == 0 || string.Compare(deprMethod, "MAT") == 0 ||
                     string.Compare(deprMethod, "MAF") == 0 || string.Compare(deprMethod, "MST") == 0 || string.Compare(deprMethod, "MSF") == 0 ||
                     string.Compare(deprMethod, "AT") == 0 || string.Compare(deprMethod, "AST") == 0 || string.Compare(deprMethod, "ASF") == 0 )
                {
                    return m_luxuryLimit;
                }
                else
                    return LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_NOTAPPLY;
            }
            set { m_luxuryLimit = value; }
        }

        void CalcBasisComponents (out double adjCost, out double PostUse)
        {
            adjCost = 0;
            PostUse = 0;
            adjCost = m_AcquisitionValue - m_dblBasisAdjustment - m_dblSection179A - m_dblElectricCar;
            PostUse = m_dblSection179;
            if ( m_reduceBasisByITC )
                //PostUse += m_dblITCBasisReduction;
                PostUse += m_ITCReduce;
            if ( m_BonusFlag )
                PostUse += m_BonusAmount;
        }

        bestAssetCreationCodeEnum CreationCode
        {
            get { return m_creationCode; }
            set { m_creationCode = value; }
        }

        DateTime TransferInDate
        {
            get { return m_TransferInDate; }
            set { m_TransferInDate = value; }
        }

        DateTime EffectiveTransferInDate
        {
            get { return m_EffectiveTransferInDate; }
            set { m_EffectiveTransferInDate = value; }
        }

        DateTime TransferOutDate
        {
            get { return m_TransferOutDate; }
            set { m_TransferOutDate = value; }
        }

        bestAssetActivityTypeEnum ActivityCode
        {
            get { return m_activityCode; }
            set { m_activityCode = value; }
        }

        public bool ReduceBasisByITC
        {
            get { return m_reduceBasisByITC; }
            set { m_reduceBasisByITC = value; }
        }

        public double ReplacementValueLife
        {
            get { return m_rvLife; }
            set { m_rvLife = value; }
        }

        public double ReplacementValueBasis
        {
            get { return m_rvBasis; }
            set { m_rvBasis = value; }
        }

        public bool UsedOutsideTheUS
        {
            get { return m_UsedOutsideUS; }
            set { m_UsedOutsideUS = value; }
        }

        public double CalculateBonus911Amount
        {
            get
            {
                double adjCost = 0;
                double PostUse = 0;
	            double BUsePercent = 1.0;
	            double dAutoLimit = 0.0 ;
                long count;
	            bool hr;
                double pVal = 0;

                m_Bonus911Amount = 0;

	            if (m_Bonus911Percent <= 0) // if the bonus911 Percent is not set, always return ZERO.
		            return pVal;


	            if (m_luxuryLimit == LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_LUXURYCAR || m_luxuryLimit == LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_LTTRUCKSANDVANS)
	            {
		            if(m_dtPlacedInServiceDate > new DateTime(2011,12,31) && m_luxuryLimit == LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_LUXURYCAR)
			            dAutoLimit = 3160.0;
		            else if(m_dtPlacedInServiceDate > new DateTime(2009,12,31) && m_luxuryLimit == LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_LUXURYCAR)
			            dAutoLimit = 3060.0;
		            else if(m_dtPlacedInServiceDate > new DateTime(2008,12,31) && m_luxuryLimit == LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_LUXURYCAR)
			            dAutoLimit = 2960.0;
		            else if(m_dtPlacedInServiceDate > new DateTime(2007,12,31) && m_luxuryLimit == LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_LUXURYCAR)
			            dAutoLimit = 2960.0;
		            else if(m_dtPlacedInServiceDate > new DateTime(2006,12,31) && m_luxuryLimit == LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_LUXURYCAR)
			            dAutoLimit = 3060.0;
		            else if (m_dtPlacedInServiceDate <= new DateTime(2003,12,31))
			            dAutoLimit = 3060.0;
		            else
			            dAutoLimit = 2960.0;

		            if (m_luxuryLimit == LUXURYLIMIT_TYPE.LUXURYLIMITTYPE_LTTRUCKSANDVANS)
		            {
			            if(m_dtPlacedInServiceDate > new DateTime(2011,12,31) )
				            dAutoLimit = 3360.0;
			            else if(m_dtPlacedInServiceDate > new DateTime(2010,12,31) )
				            dAutoLimit = 3260.0;
			            else if(m_dtPlacedInServiceDate > new DateTime(209,12,31) )
				            dAutoLimit = 3160.0;
			            else if(m_dtPlacedInServiceDate > new DateTime(2008,12,31) )
				            dAutoLimit = 3060.0;
			            else if(m_dtPlacedInServiceDate > new DateTime(2007,12,31) )
				            dAutoLimit = 3160.0;
			            else
				            dAutoLimit += 300;
		            }
	            }

	            // PRORATING LUXURY LIMIT FOR SHORT YEARS and add the additional limit  based on PIS Date
	            if (dAutoLimit > 0.0)
	            {
		            IBAFiscalYear fy;
		            double	dFYFactor = 0;

		            if (!(m_Calendar.GetFiscalYear(m_dtPlacedInServiceDate, out fy)) ||
			            !(fy.GetFiscalYearFraction(out dFYFactor)) )
		            {
		            }
		            if (dFYFactor >= 0.0 && dFYFactor < 1.0)
		            {
			            dAutoLimit *= dFYFactor;
		            }

		            if (m_dtPlacedInServiceDate <= new DateTime(2003,5,5))
		            {
			            dAutoLimit += 4600.0;
		            }
		            else if (m_dtPlacedInServiceDate <= new DateTime(2004,12,31))
		            {
			            dAutoLimit += 7650.0;
		            }
		            else if ( new DateTime(2008,1,1) <= m_dtPlacedInServiceDate && m_dtPlacedInServiceDate <= new DateTime(2013,12,31))
		            {
			            if(m_ZoneCode == 0 || m_ZoneCode == 3)
				            dAutoLimit += 8000.0;
		            }
	            }
            /*
                if ( !m_BusUseList )
                    return Error("Business Use List not initialized.");

                if ( FAILED(hr = m_BusUseList->get_Count(&count)) )
		            return hr;

	            if (count > 0)
	            {
		            double busUse = 1.0;
		            double investUse = 0.0;

		            if ( FAILED(hr = GetBusinessUse(m_dtPlacedInServiceDate, &busUse, &investUse)) )
		            {
			            return hr; 
		            }
		            BUsePercent = busUse + investUse;
	            }

                *pVal = 0;
            */
                adjCost = m_AcquisitionValue - m_dblBasisAdjustment - m_dblSection179A - m_dblElectricCar;
                PostUse = m_dblSection179;
                if ( m_reduceBasisByITC && m_ITCAmount != 0.0 && m_ITCReduce != 0.0)
	            {
                    double cyItcRAmt;

                    cyItcRAmt = CalculateITCBasisReductionAmount(false);

                    PostUse += cyItcRAmt;
	            }
                if ( m_BonusFlag )
                    PostUse += m_BonusAmount;

	            if (BUsePercent < 1.0)
	            {
		            m_Bonus911Amount = (( adjCost * BUsePercent - PostUse ) * m_Bonus911Percent)/100;

		            if (dAutoLimit > 0.0 )
			            dAutoLimit *= BUsePercent;
	            }
	            else
	            {
		            m_Bonus911Amount = (( adjCost - PostUse ) * m_Bonus911Percent)/100;
	            }

	            if ( string.Compare(m_szDeprMethod, "SL") == 0 )
	            {
		            m_Bonus911Amount -= (m_dblSalvageDeduction * m_Bonus911Percent)/100;
	            }

	            //temporary fix to handle luxury limits

	            if (dAutoLimit >  0.0 && (m_Bonus911Amount > 0.0))
	            {

		            if ((m_Bonus911Amount + PostUse) > dAutoLimit)
		            {
			            m_Bonus911Amount = dAutoLimit - PostUse;
		            }
	            }

	            //if bonus911 is of different sign as that of acquistion value, make it ZERO.
	            if (Math.Abs(m_AcquisitionValue + m_Bonus911Amount) < Math.Abs(m_AcquisitionValue - m_Bonus911Amount))
		            m_Bonus911Amount = 0.0;

	            m_Bonus911Amount = CurrencyHelper.FormatCurrency (m_Bonus911Amount);

	            pVal = m_Bonus911Amount;
	            return pVal;
            }
        }

        public double CalculateITCBasisReductionAmount(bool AceSwitch)
        {
            double factor;
            double pVal = 0;

	        if (m_ITCAmount == 0.0) //IF itc amount is zero, don't do any further processing.
	        {
		        return 0.0;
	        }

	        if (m_ITCReduce == 0.0) //IF itc reduce is zero, don't do any further processing.
	        {
		        return 0.0;
	        }

            factor = CalculateITCBasisReductionFactor();

            if (AceSwitch == false && m_ImportCode == bestAssetImportCodeEnum.icPCFasImport)
            {
                if ( string.Compare(m_szDeprMethod, "DBn") == 0 ||
                     string.Compare(m_szDeprMethod, "DBs") == 0 ||
                     (string.Compare(m_szDeprMethod, "SL") == 0 && string.Compare(m_szAvgConvention, "FM") != 0) ||
                     string.Compare(m_szDeprMethod, "RV")  == 0 )
                {
                    factor = 0;
                }
            }
	        //Truncate ITC Redn Amt to nearest penny---SAI
            pVal = (factor * m_ITCAmount * 100.0) / 100.0;
            pVal = (factor * m_ITCReduce * 100.0) / 100.0;

            return pVal;
        }

        public iTCCodes _convertITCStatus(bestAssetITCTypeEnum itcCode)
        {
            switch ( itcCode )
            {
                case bestAssetITCTypeEnum.itcNoITC:
                    return iTCCodes.NO_ITC;
                case bestAssetITCTypeEnum.itcNewPropFullCredit:
                    return iTCCodes.NEW_PROP_FULL;
                case bestAssetITCTypeEnum.itcNewPropReducedCredit:
                    return iTCCodes.NEW_PROP_RED;
                case bestAssetITCTypeEnum.itcUsedPropFullCredit:
                    return iTCCodes.USED_PROP_FULL;
                case bestAssetITCTypeEnum.itcUsedPropReducedCredit:
                    return iTCCodes.USED_PROP_RED;
                case bestAssetITCTypeEnum.itcRehab30Year:
                    return iTCCodes.THIRTY_YR_RE;
                case bestAssetITCTypeEnum.itcRehab40Year:
                    return iTCCodes.FORTY_YR_RE;
                case bestAssetITCTypeEnum.itcCertHistoricRehab:
                    return iTCCodes.CERT_HISTORIC;
                case bestAssetITCTypeEnum.itcNonCertHistoricRehab:
                    return iTCCodes.NON_HISTORIC;
                case bestAssetITCTypeEnum.itcBiomass:
                    return iTCCodes.BIOMASS_PROP;
                case bestAssetITCTypeEnum.itcIntercityBuses:
                    return iTCCodes.INTER_CITY_BUS;
                case bestAssetITCTypeEnum.itcHydroelectricGenerating:
                    return iTCCodes.HYDRO_GEN_PLANT;
                case bestAssetITCTypeEnum.itcOceanThermal:
                    return iTCCodes.OCEAN_THRM_PROP;
                case bestAssetITCTypeEnum.itcSolarEnergy:
                    return iTCCodes.SOLAR_ENERGY;
                case bestAssetITCTypeEnum.itcWind:
                    return iTCCodes.WIND_PROP;
                case bestAssetITCTypeEnum.itcGeoThermal:
                    return iTCCodes.GEO_THRM_PROP;
                case bestAssetITCTypeEnum.itcCertHistoricTransition:
                    return iTCCodes.ITCP;
                case bestAssetITCTypeEnum.itcQualifiedProgressExp:
                    return iTCCodes.ITCQ;

                case bestAssetITCTypeEnum.itcSolarEnergyProperty:
                    return iTCCodes.ITCS;
                case bestAssetITCTypeEnum.itcOtherEnergyProperty:
                    return iTCCodes.ITCT;
                case bestAssetITCTypeEnum.itcFuelCellProperty:
                    return iTCCodes.ITCU;
                case bestAssetITCTypeEnum.itcMicroturbineProperty:
                    return iTCCodes.ITCV;
                case bestAssetITCTypeEnum.itcAdvancedCoalProject:
                    return iTCCodes.ITCW;
                case bestAssetITCTypeEnum.itcGasificationProject:
                    return iTCCodes.ITCY;
            }
            return iTCCodes.NO_ITC;
        }

        public double CalculateITCBasisReductionFactor()
        {
            double pVal = 0;

            if (m_reduceBasisByITC)
            {
                pVal = 1.0;
            }

            return pVal;
        }   
    }
}
