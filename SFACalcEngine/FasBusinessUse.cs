using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFACalcEngine
{
    class FasBusinessUse
    {
        DateTime m_EffectiveDate;
        double m_BusinessUse;
        double m_InvestmentUse;

        public DateTime EffectiveDate { get { return m_EffectiveDate; } set { m_EffectiveDate = value; } }
        public double BusinessUsePercent { get { return m_BusinessUse; } set { m_BusinessUse = value; } }
        public double InvestmentUsePercent { get { return m_InvestmentUse; } set { m_InvestmentUse = value; } }
    }
}
