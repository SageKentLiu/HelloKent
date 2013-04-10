using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFACalendar
{
    class BAFiscalYearHolder
    {
        IBAFiscalYear m_object;
        DateTime m_startDate;
        DateTime m_endDate;
        long m_count;

        public BAFiscalYearHolder()
        {
            m_startDate = DateTime.MinValue;
            m_endDate = DateTime.MinValue;
            m_count = 0;
        }

        public bool HoldMe(IBAFiscalYear newVal)
        {
            m_object = null;
            m_object = newVal;
            m_startDate = DateTime.MinValue;
            m_endDate = DateTime.MinValue;
            m_count = 0;
            if (m_object != null)
            {
                m_startDate = m_object.YRStartDate;
                m_endDate = m_object.YREndDate;
            }
            m_count = 1;
            return true;
        }

        public bool GetObject(DateTime dt, out IBAFiscalYear pVal) // returns E_FAIL if not valid.
        {
            if (dt > DateTime.MinValue && dt >= m_startDate && dt <= m_endDate)
            {
                m_count++;
                pVal = m_object;
                return true;
            }
            pVal = null;
            return false;
        }

        public long get_Count()
        {
            if (m_object == null)
                return 0;
            return m_count;
        }

    }
}
