using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFACalendar
{
    class BAFASCalcPeriod : IBACalcPeriod
    {
        DateTime m_dtStartDate;
        DateTime m_dtEndDate;
        short m_iPeriodNum;
        short m_iWeight;
        bool m_bIsIdle;

        public DateTime PeriodStart
        {
            get
            {
                return m_dtStartDate;
            }
            set
            {
                m_dtStartDate = value;
            }
        }

        public DateTime PeriodEnd
        {
            get
            {
                return m_dtEndDate;
            }
            set
            {
                m_dtEndDate = value;
            }
        }

        public short PeriodNum
        {
            get
            {
                return m_iPeriodNum;
            }
            set
            {
                m_iPeriodNum = value;
            }
        }

        public short Weight
        {
            get
            {
                return m_iWeight;
            }
            set
            {
                m_iWeight = value;
            }
        }

        public bool IsIdle
        {
            get
            {
                return m_bIsIdle;
            }
            set
            {
                m_bIsIdle = value;
            }
        }

        public bool Clear()
        {
            m_dtStartDate = DateTime.MinValue;
            m_dtEndDate = DateTime.MinValue;
            m_iPeriodNum = 0;
            m_iWeight = 0;
            m_bIsIdle = false;

            return true;
        }
    }
}
