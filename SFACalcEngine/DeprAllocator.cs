using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFACalendar;

namespace SFACalcEngine
{
    public class DeprAllocator
    {
        IBACalendar m_pObjCalendar;
        List<IBAPeriodDeprItem> m_pObjList;
        DateTime m_dtPISDate;
        DateTime m_dtDeemedEndDate;
        DateTime m_dtDisposalDate;

        public DateTime PlacedInService
        {
            get { return m_dtPISDate; }
            set { m_dtPISDate = value; }
        }

        public DateTime DeemedEndDate
        {
            get { return m_dtDeemedEndDate; }
            set { m_dtDeemedEndDate = value; }
        }

        public DateTime DisposalDate
        {
            get { return m_dtDisposalDate; }
            set { m_dtDisposalDate = value; }
        }

        public IBACalendar Calendar
        {
            set { m_pObjCalendar = value; }
        }

        public List<IBAPeriodDeprItem> PeriodDeprItemList
        {
            get { return m_pObjList; }
            set { m_pObjList = value; }
        }

        public bool SplitPDItem(IBAPeriodDeprItem source, DateTime rightDate, out IBAPeriodDeprItem left, out IBAPeriodDeprItem right)
        {
            right = null;
            left = null;

	        if ( source == null )
		        return false;
	        if ( left == null || right == null )
		        return false;
//	        return source->Split2ways(rightDate, m_pObjCalendar, m_dtPISDate, m_dtDeemedEndDate, left, right);
            return true;
        }

        public bool SplitPDItem3ways(IBAPeriodDeprItem source, DateTime middleStart, DateTime rightStart, out IBAPeriodDeprItem left, out IBAPeriodDeprItem middle, out IBAPeriodDeprItem right)
        {
            right = null;
            left = null;
            middle = null;

            if (source == null)
		        return false;
            if (left == null || right == null || middle == null)
		        return false;
            //return source->Split3ways(middleStart, rightStart, m_pObjCalendar, m_dtPISDate, m_dtDeemedEndDate, left, middle, right);
            return true;
        }
    }
}
