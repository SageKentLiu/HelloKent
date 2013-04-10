using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFACalcEngine
{
    class DecliningBalanceMethod : IBADeprMethod, IBASwitchDepr
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

        double m_dAnuualRate;
        int m_iSwitchYearNum;
        bool m_bSwitchRequired;
        bool m_bIsShortYear;

        IBADeprMethod m_pObjSwitchMethod;

        private string m_parentFlags;

        public DecliningBalanceMethod()
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
            m_dAnuualRate = 0;
            m_iSwitchYearNum = 0;
            m_bSwitchRequired = true;
            m_bIsShortYear = false;
            m_pObjSwitchMethod = null;
            m_parentFlags = "";
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
                return m_dAdjustedCost - m_dPostUsedDeduction;
            }
        }

        public double RemainingDeprAmt
        {
            get
            {
                double remainingDeprAmt;
                remainingDeprAmt = m_dAdjustedCost - m_dPostUsedDeduction -
                                   m_dSalvageDeduction - m_dPriorAccum;
                return remainingDeprAmt;
            }
        }

        public string BaseShortName
        {
            get { return "DBs"; }
        }

        public string BaseLongName
        {
            get { return "Declining Balance (Switch)"; }
        }

        public bool UseFirstYearFactor
        {
            get { return true; }
        }

        public double TotalDepreciationAllowed
        {
            get
            {
                double basis;
                double salvage;

                basis = Basis;
                salvage = SalvageDeduction;

                return basis - salvage;
            }
        }

        public DISPOSALOVERRIDETYPE DisposalOverride
        {
            get { return DISPOSALOVERRIDETYPE.disposaloverride_Normal; }
        }

        public bool Initialize(IBADeprScheduleItem schedule, IBAAvgConvention convention)
        {
            bool hr;

            if (schedule == null)
                return false;

            DBPercent = schedule.DeprPercent;
            YearElapsed = 0;
            Life = schedule.DeprLife;
            SalvageDeduction = schedule.SalvageDeduction;
            AdjustedCost = schedule.AdjustedCost;
            PostUsageDeduction = schedule.PostUsageDeduction;



            if (m_pObjSwitchMethod != null)
                if (!(hr = m_pObjSwitchMethod.Initialize(schedule, convention)))
                    return hr;

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

            m_dAnuualRate = (m_dDBPercent * 0.01) / m_dLife;
            return (dBasis - m_dPriorAccum) * m_dAnuualRate;

        }

        public bool GetAvgConvention(IBADeprScheduleItem schedule, ref string pVal)
        {
            return false;
        }

        #endregion

        public bool SwitchRequired
        {
            get
            {
                return m_bSwitchRequired;
            }
            set
            {
                m_bSwitchRequired = value;
            }
        }

        public short SwitchYearNum
        {
            get
            {
                return (short)m_iSwitchYearNum;
            }
            set
            {
                m_iSwitchYearNum = value;
            }
        }

        public bool IsShortYear
        {
            get
            {
                return m_bIsShortYear;
            }
            set
            {
                m_bIsShortYear = value;
            }
        }

        public IBADeprMethod SwitchMethod
        {
            get
            {
                IBADeprMethod pVal = m_pObjSwitchMethod as IBADeprMethod;

                if (pVal == null)
                    return null;

                pVal.AdjustedCost = (m_dAdjustedCost);
                pVal.PostUsageDeduction = (m_dPostUsedDeduction);
                pVal.SalvageDeduction = (m_dSalvageDeduction);
                pVal.PriorAccum = (m_dPriorAccum);
                pVal.Life = (m_dLife);
                pVal.YearElapsed = (m_dYearElapsed);
                pVal.DeemedStartDate = (m_dtDeemedStartDate);
                pVal.DeemedEndDate = (m_dtDeemedEndDate);
                pVal.YearNum = (m_iYearNum);
                pVal.DBPercent = (m_dDBPercent);
                pVal.FiscalYearFraction = (m_dFiscalYearFraction);
                return pVal;
            }
            set
            {
                string parentFlags;
                string flags;
                bool hr;

                m_pObjSwitchMethod = value;
                if (value == null)
                    return;
                parentFlags = ParentFlags;
                flags = SwitchFlag;
                parentFlags += flags;

                m_pObjSwitchMethod.ParentFlags = (parentFlags);
            }
        }

        public bool CheckForSwitch
        {
            get
            {
                IBADeprMethod pObjDeprMethod;
                IBADeprMethod pObjSwitchMethos;
                double dblAnuDeprAmt;
                double dblSAnuDeprAmt;

                pObjDeprMethod = this;
                dblAnuDeprAmt = pObjDeprMethod.CalculateAnnualDepr();
                pObjSwitchMethos = SwitchMethod;
                dblSAnuDeprAmt = pObjSwitchMethos.CalculateAnnualDepr();

                if (CurrencyHelper.FormatCurrency(dblAnuDeprAmt) < CurrencyHelper.FormatCurrency(dblSAnuDeprAmt))
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }

        public string SwitchMethodName
        {
            get
            {
                return "RV";

            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string SwitchFlag
        {
            get
            {
                return "s";
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool GetCurrentMethod(string flags, out IBADeprMethod pVal)
        {
            pVal = null;
            string switchFlag;
            IBASwitchDepr swIface;
            bool hr;

            if (flags == null)
                return false;

            switchFlag = SwitchFlag;

            if (flags.Contains(switchFlag))
            {
                if (m_pObjSwitchMethod == null)
                    throw new Exception("Flags indicate that a switch is needed, but the switch interface is not initialized.");

                //
                // Now set up the method that we are switching to.
                //
                m_pObjSwitchMethod.AdjustedCost = (m_dAdjustedCost);
                m_pObjSwitchMethod.PostUsageDeduction = (m_dPostUsedDeduction);
                m_pObjSwitchMethod.SalvageDeduction = (m_dSalvageDeduction);
                m_pObjSwitchMethod.PriorAccum = (m_dPriorAccum);
                m_pObjSwitchMethod.Life = (m_dLife);
                m_pObjSwitchMethod.YearElapsed = (m_dYearElapsed);
                m_pObjSwitchMethod.DeemedStartDate = (m_dtDeemedStartDate);
                m_pObjSwitchMethod.DeemedEndDate = (m_dtDeemedEndDate);
                m_pObjSwitchMethod.YearNum = (m_iYearNum);
                m_pObjSwitchMethod.DBPercent = (m_dDBPercent);
                m_pObjSwitchMethod.FiscalYearFraction = (m_dFiscalYearFraction);

                swIface = m_pObjSwitchMethod as IBASwitchDepr;
                if (swIface == null)
                {
                    pVal = m_pObjSwitchMethod as IBADeprMethod;
                    return true;
                }
                return swIface.GetCurrentMethod(flags, out pVal);
            }
            else
            {
                pVal = this as IBADeprMethod;
                return true;
            }
        }
    }
}
