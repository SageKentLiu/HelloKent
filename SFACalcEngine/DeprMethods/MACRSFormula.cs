using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFACalcEngine
{
    class MACRSFormula : IBADeprMethod, IBASwitchDepr
    {
        IBADeprMethod m_ddbMethod;
        IBASwitchDepr m_ddbSwitch;

        private string m_parentFlags;

        public MACRSFormula()
        {
            m_ddbMethod = new DecliningBalanceMethod();
            m_ddbSwitch = m_ddbMethod as IBASwitchDepr;
            m_ddbSwitch.SwitchRequired = true; 
        }


        #region IBADeprMethod Members

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

        public bool IsFiscalYearBased
        {
            get { return m_ddbMethod.IsFiscalYearBased; }
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

        public string BaseShortName
        {
            get { return "MF"; }
        }

        public string BaseLongName
        {
            get { return "MACRS Formula"; }
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
                basis = Basis;
                return basis;
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

            if (m_ddbMethod != null)
            {
                if (!(hr = m_ddbMethod.Initialize(schedule, convention)))
                    return hr;
                m_ddbMethod.SalvageDeduction = (0);
            }
            return true;
        }

        public double CalculateAnnualDepr()
        {
            return m_ddbMethod.CalculateAnnualDepr();

        }

        public bool GetAvgConvention(IBADeprScheduleItem schedule, ref string pVal)
        {
            string tmp;
            tmp = schedule.AvgConvention;
            tmp += "mb";
            pVal = tmp;
            return true;
        }

        #endregion

        public bool SwitchRequired
        {
            get
            {
                return true;
            }
            set
            {
                m_ddbSwitch.SwitchRequired = true;
            }
        }

        public short SwitchYearNum
        {
            get
            {
                return (short)m_ddbSwitch.SwitchYearNum;
            }
            set
            {
                m_ddbSwitch.SwitchYearNum = value;
            }
        }

        public bool IsShortYear
        {
            get
            {
                return m_ddbSwitch.IsShortYear;
            }
            set
            {
                m_ddbSwitch.IsShortYear = value;
            }
        }

        public IBADeprMethod SwitchMethod
        {
            get
            {
                return m_ddbSwitch.SwitchMethod;
            }
            set
            {
                m_ddbSwitch.SwitchMethod = value;
            }
        }

        public bool CheckForSwitch
        {
            get
            {
                return m_ddbSwitch.CheckForSwitch;
            }
        }

        public string SwitchMethodName
        {
            get
            {
                return m_ddbSwitch.SwitchMethodName;

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
                return m_ddbSwitch.SwitchFlag;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool GetCurrentMethod(string flags, out IBADeprMethod pVal)
        {

            return m_ddbSwitch.GetCurrentMethod(flags, out pVal);
        }
    }
}
