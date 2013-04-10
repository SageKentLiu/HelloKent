using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFACalcEngine
{
    public class PeriodDetailDeprInfo : IBAPeriodDetailDeprInfo
    {
        DateTime m_FiscalYearStartDate;
        DateTime m_FiscalYearEndDate;

        decimal m_FiscalYearBeginAccum;
        decimal m_FiscalYearEndAccum;
        decimal m_FiscalYearDeprAmount;

        DateTime m_PeriodStartDate;
        DateTime m_PeriodEndDate;
                       
        decimal  m_PeriodBeginAccum;
        decimal  m_PeriodEndAccum;
        decimal  m_PeriodDeprAmount;

        string m_CalcFlags;

        public PeriodDetailDeprInfo()
        {
        }


        public DateTime FiscalYearStartDate { get { return m_FiscalYearStartDate; } set { m_FiscalYearStartDate = value; } }
        public DateTime FiscalYearEndDate { get { return m_FiscalYearEndDate; } set { m_FiscalYearEndDate = value; } }

        public decimal FiscalYearBeginAccum { get { return m_FiscalYearBeginAccum; } set { m_FiscalYearBeginAccum = value; } }
        public decimal FiscalYearEndAccum { get { return m_FiscalYearEndAccum; } set { m_FiscalYearEndAccum = value; } }
        public decimal FiscalYearDeprAmount { get { return m_FiscalYearDeprAmount; } set { m_FiscalYearDeprAmount = value; } }

        public DateTime PeriodStartDate  { get { return m_PeriodStartDate; }  set { m_PeriodStartDate = value; } }
        public DateTime PeriodEndDate    { get { return m_PeriodEndDate; }    set { m_PeriodEndDate = value; } }
        public decimal  PeriodBeginAccum { get { return m_PeriodBeginAccum; } set { m_PeriodBeginAccum = value; } }
        public decimal  PeriodEndAccum   { get { return m_PeriodEndAccum; }   set { m_PeriodEndAccum = value; } }
        public decimal  PeriodDeprAmount { get { return m_PeriodDeprAmount; } set { m_PeriodDeprAmount = value; } }

        public string CalcFlags { get { return m_CalcFlags; } set { m_CalcFlags = value; } }
    }
}
