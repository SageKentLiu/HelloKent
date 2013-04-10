using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFACalcEngine
{
    class StraightLineMethod : IBADeprMethod
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
        private bool m_VintageAccount;

        public StraightLineMethod()
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
            m_VintageAccount = false;
        }


        #region IBADeprMethod Members

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
                return m_dSalvageDeduction;
            }
            set
            {
                m_dSalvageDeduction = value;
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

        public bool IsFiscalYearBased
        {
            get { return false; }
        }

        public double Basis
        {
            get
            {
                if (m_VintageAccount)
                {
                    return m_dAdjustedCost - m_dPostUsedDeduction;
                }
                else
                {
                    return m_dAdjustedCost - m_dPostUsedDeduction - m_dSalvageDeduction;
                }

            }
        }

        public double RemainingDeprAmt
        {
            get
            {
                double basis = Basis;
                double remainingDeprAmt;
                if (m_dLife > m_dYearElapsed + 0.001)
                {
                    remainingDeprAmt = basis * (m_dLife - m_dYearElapsed) / m_dLife;
                    if (remainingDeprAmt - 1.0 > basis - m_dPriorAccum)
                        remainingDeprAmt = basis - m_dPriorAccum;
                }
                else
                {
                    remainingDeprAmt = basis - m_dPriorAccum;
                }
                return remainingDeprAmt;
            }
        }

        public string BaseShortName
        {
            get { return "SL"; }
        }

        public string BaseLongName
        {
            get { return "Straight Line"; }
        }

        public bool UseFirstYearFactor
        {
            get { return true; }
        }

        public double TotalDepreciationAllowed
        {
            get
            {
                double basis = Basis;
                double salvage = SalvageDeduction;

                if (m_VintageAccount)
                {
                    return basis - salvage;
                }
                return basis;
            }
        }

        public DISPOSALOVERRIDETYPE DisposalOverride
        {
            get { return DISPOSALOVERRIDETYPE.disposaloverride_Normal; }
        }

        public bool Initialize(IBADeprScheduleItem schedule, IBAAvgConvention convention)
        {
            if (schedule == null)
                return false;

            DBPercent = schedule.DeprPercent;
            YearElapsed = 0;
            Life = schedule.DeprLife;
            SalvageDeduction = schedule.SalvageDeduction;
            AdjustedCost = schedule.AdjustedCost;
            PostUsageDeduction = schedule.PostUsageDeduction;
            m_VintageAccount = schedule.VintageAccountFlag;

            return true;
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

        public bool GetAvgConvention(IBADeprScheduleItem schedule, ref string pVal)
        {
            return false;
        }

        #endregion
    }
}
