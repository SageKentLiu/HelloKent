using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFACalcEngine 
{
    class AltACRSFormula : IBADeprMethod
    {
        private double m_dAdjustedCost;
        private double m_dPostUsedDeduction;
        private double m_dSalvageDeduction;
        private double m_dPriorAccum;
        private double m_dYearElapsed;
        private double m_dLife;
        private double m_dDBPercent;
        private double m_dFiscalYearFraction;
        private DateTime m_dtDeemedStartDate;
        private DateTime m_dtDeemedEndDate;
        private short m_iYearNum;
        private string m_parentFlags;
        private DISPOSALOVERRIDETYPE m_disposalOverride;
        private bool m_bUseFirstYearFactor;

        public AltACRSFormula()
        {
            m_dAdjustedCost = 0;
            m_dPostUsedDeduction = 0;
            m_dSalvageDeduction = 0;
            m_dPriorAccum = 0;
            m_dYearElapsed = 0;
            m_dLife = 0;
            m_dDBPercent = 0;
            m_dFiscalYearFraction = 0;
            m_dtDeemedStartDate = DateTime.MinValue;
            m_dtDeemedEndDate = DateTime.MinValue;
            m_iYearNum = 0;
            m_parentFlags = "";
        }

        public double AdjustedCost
        {
            get
            {
                return m_dAdjustedCost;
            }
            set
            {
                m_dAdjustedCost = value;
            }
        }

        public double PostUsageDeduction
        {
            get
            {
                return m_dPostUsedDeduction;
            }
            set
            {
                m_dPostUsedDeduction = value;
            }
        }

        public double SalvageDeduction
        {
            get
            {
                return 0;
            }
            set
            {
            }
        }

        public double PriorAccum
        {
            get
            {
                return m_dPriorAccum;
            }
            set
            {
                m_dPriorAccum = value;
            }
        }

        public double YearElapsed
        {
            get
            {
                return m_dYearElapsed;
            }
            set
            {
                m_dYearElapsed = value;
            }
        }

        public short YearNum
        {
            get
            {
                return m_iYearNum;
            }
            set
            {
                m_iYearNum = value;
            }
        }

        public bool IsFiscalYearBased
        {
            get { return false; }
        }

        public double Life
        {
            get
            {
                return m_dLife;
            }
            set
            {
                m_dLife = value;
            }
        }

        public double DBPercent
        {
            get
            {
                return m_dDBPercent;
            }
            set
            {
                m_dDBPercent = value;
            }
        }

        public double FiscalYearFraction
        {
            get
            {
                return m_dFiscalYearFraction;
            }
            set
            {
                m_dFiscalYearFraction = value;
            }
        }

        public DateTime DeemedStartDate
        {
            get
            {
                return m_dtDeemedStartDate;
            }
            set
            {
                m_dtDeemedStartDate = value;
            }
        }

        public DateTime DeemedEndDate
        {
            get
            {
                return m_dtDeemedEndDate;
            }
            set
            {
                m_dtDeemedEndDate = value;
            }
        }

        public double CalculateAnnualDepr()
        {
            // TODO: Add your implementation code here
            double dBasis;

            if (m_dLife <= 0)
            {
                return 0;
            }

            dBasis = Basis;
            return dBasis / m_dLife;
        }

        public double Basis
        {
            get
            {
                return m_dAdjustedCost - m_dPostUsedDeduction;
            }
        }

        public double RemainingDeprAmt
        {
            get
            {
                return m_dAdjustedCost - m_dPostUsedDeduction - m_dPriorAccum;
            }
        }

        public bool GetAvgConvention(IBADeprScheduleItem schedule, ref string pVal)
        {
	        string tmp;
	        DateTime PlacedInServiceDate;
            double deprLife;
	        bool LowIncomeHousing;
	        bool PersonalProperty;
	        bool PublicUtility;
	        bool hr;

	        if (pVal == null)
		        return false ;
	        if (schedule == null)
		        return false;

            deprLife            = schedule.DeprLife;
            LowIncomeHousing    = schedule.LowIncomeHousingFlag;
	        PersonalProperty    = schedule.PersonalPropertyFlag;
	        PublicUtility       = schedule.PublicUtilityFlag;
	        PlacedInServiceDate = schedule.PlacedInServiceDate;

	        if ( PersonalProperty || PublicUtility )
                tmp = "HYmb";
            else if ( LowIncomeHousing )
                tmp = "FMmb";
            else if (deprLife < 17 || (((int)(deprLife + 0.01)) == 18 && PlacedInServiceDate < new DateTime(1984, 6,23)) ||
                     (((int)(deprLife + 0.01)) > 19 && ((int)(deprLife + 0.01)) <= 35 && PlacedInServiceDate < new DateTime(1984,6,23)) ||
                     (deprLife > 35.01 && PlacedInServiceDate < new DateTime(1984,6,23)) )
		        tmp = "FMmb";
	        else
		        tmp = "MMmb";

	        pVal = tmp;
	        return true;
        }

        public string BaseShortName
        {
            get { return "ASF"; }
        }

        public string BaseLongName
        {
            get { return "Alternate ACRS Formula"; }
        }

        public string ParentFlags
        {
            get
            {
                return m_parentFlags;
            }
            set
            {
                m_parentFlags = value;
            }
        }

        public bool Initialize(IBADeprScheduleItem schedule, IBAAvgConvention convention)
        {
            DateTime PlacedInServiceDate;
   	        bool LowIncomeHousing;
	        bool PersonalProperty;
	        bool PublicUtility;
	        bool OutsideUS;

            if (schedule == null)
                return false;

            DBPercent = schedule.DeprPercent;
            YearElapsed = 0;
            Life = schedule.DeprLife;
            SalvageDeduction = schedule.SalvageDeduction;
            AdjustedCost = schedule.AdjustedCost;
            PostUsageDeduction = schedule.PostUsageDeduction;

            LowIncomeHousing    = schedule.LowIncomeHousingFlag;
            PersonalProperty    = schedule.PersonalPropertyFlag;
            PublicUtility       = schedule.PublicUtilityFlag;
            OutsideUS           = schedule.UsedOutsideTheUS;
            PlacedInServiceDate = schedule.PlacedInServiceDate;

            m_disposalOverride = DISPOSALOVERRIDETYPE.disposaloverride_Normal;
            m_bUseFirstYearFactor = false;

            if (LowIncomeHousing)
            {
                // nothing special here
            }
            else if ((PersonalProperty || PublicUtility))
            {
                m_disposalOverride = DISPOSALOVERRIDETYPE.disposaloverride_NoneInYear;
                m_bUseFirstYearFactor = true;
            }
            
            return true;
        }

        public DISPOSALOVERRIDETYPE DisposalOverride
        {
            get { return m_disposalOverride; }
        }

        public bool UseFirstYearFactor
        {
            get { return true; }
        }

        public double TotalDepreciationAllowed
        {
            get
            {
                return Basis;
            }
        }
    }
}
