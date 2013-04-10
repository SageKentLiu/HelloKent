using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFACalcEngine
{
    class DecliningBalanceMethodNoSwitch : IBADeprMethod
    {
        IBADeprMethod m_ddbMethod;
        IBASwitchDepr m_ddbSwitch;
        string m_parentFlags;

        public DecliningBalanceMethodNoSwitch()
        {
            m_ddbMethod = new DecliningBalanceMethod();
            m_ddbSwitch = m_ddbMethod as IBASwitchDepr;
            m_ddbSwitch.SwitchRequired = false; 
        }

        public double AdjustedCost
        {
            get
            {
                return m_ddbMethod.AdjustedCost;
            }
            set
            {
                m_ddbMethod.AdjustedCost = value;
            }
        }

        public double PostUsageDeduction
        {
            get
            {
                return m_ddbMethod.PostUsageDeduction;
            }
            set
            {
                m_ddbMethod.PostUsageDeduction = value;
            }
        }

        public double SalvageDeduction
        {
            get
            {
                return m_ddbMethod.SalvageDeduction;
            }
            set
            {
                m_ddbMethod.SalvageDeduction = value;
            }
        }

        public double PriorAccum
        {
            get
            {
                return m_ddbMethod.PriorAccum;
            }
            set
            {
                m_ddbMethod.PriorAccum = value;
            }
        }

        public double YearElapsed
        {
            get
            {
                return m_ddbMethod.YearElapsed;
            }
            set
            {
                m_ddbMethod.YearElapsed = value;
            }
        }

        public short YearNum
        {
            get
            {
                return m_ddbMethod.YearNum;
            }
            set
            {
                m_ddbMethod.YearNum = value;
            }
        }

        public bool IsFiscalYearBased
        {
            get { return m_ddbMethod.IsFiscalYearBased; }
        }

        public double Life
        {
            get
            {
                return m_ddbMethod.Life;
            }
            set
            {
                m_ddbMethod.Life = value;
            }
        }

        public double DBPercent
        {
            get
            {
                return m_ddbMethod.DBPercent;
            }
            set
            {
                m_ddbMethod.DBPercent = value;
            }
        }

        public double FiscalYearFraction
        {
            get
            {
                return m_ddbMethod.FiscalYearFraction;
            }
            set
            {
                m_ddbMethod.FiscalYearFraction = value;
            }
        }

        public DateTime DeemedStartDate
        {
            get
            {
                return m_ddbMethod.DeemedStartDate;
            }
            set
            {
                m_ddbMethod.DeemedStartDate = value;
            }
        }

        public DateTime DeemedEndDate
        {
            get
            {
                return m_ddbMethod.DeemedEndDate;
            }
            set
            {
                m_ddbMethod.DeemedEndDate = value;
            }
        }

        public double CalculateAnnualDepr()
        {
            return m_ddbMethod.CalculateAnnualDepr();
        }

        public double Basis
        {
            get
            {
                return m_ddbMethod.Basis;
            }
        }

        public double RemainingDeprAmt
        {
            get
            {
                return m_ddbMethod.RemainingDeprAmt;
            }
        }

        public bool GetAvgConvention(IBADeprScheduleItem schedule, ref string pVal)
        {
            return false;
        }

        public string BaseShortName
        {
            get { return "DBn"; }
        }

        public string BaseLongName
        {
            get { return "Declining Balance (No Switch)"; }
        }

        public string ParentFlags
        {
            get
            {
                return m_ddbMethod.ParentFlags;
            }
            set
            {
                m_ddbMethod.ParentFlags = value;
            }
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
            return true;
        }

        public DISPOSALOVERRIDETYPE DisposalOverride
        {
            get { return DISPOSALOVERRIDETYPE.disposaloverride_Normal; }
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
    }
}
