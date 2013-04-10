using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFACalcEngine
{
    public class PeriodDeprItem : IBAPeriodDeprItem
    {
        private DateTime m_dtStartDate;
        private DateTime m_dtEndDate;
        private short m_iFYNum;
        private short m_iTotalPeriodWeights;

        private decimal m_curBeginYearAccum;
        private decimal m_curEndDateBeginYearAccum;
        private decimal m_curBeginYTDExpense;
        private decimal m_curDeprAmount;
        private decimal m_curAdjustAmount;
        private decimal m_curSection179Change;
        private decimal m_EndDateDeferredAccum;
        private decimal m_EndDateYTDDeferred;
        private decimal m_calcOverride;
        private decimal m_PersUseAccum;
        private decimal m_PersUseYTD;

        private double m_RemainingLife;
        private double m_PersonalUseAmount;
        private PERIODDEPRITEM_ENTRYTYPE m_eEntryType;
        private string m_sCalcFlags;

        public double m_dDeprAmountMarks;
        public double m_dAdjustAmountMarks;
        public double m_dYTDDeferredMarks;
        public double m_dYTDPersUseMarks;
        public double m_dPersUseMarks;
        public short m_iCountToLeft;
        public short m_iCountToRight;
        public short m_iYearWeight;

        public PeriodDeprItem()
        {
            Clear();
        }

        public DateTime StartDate
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

        public DateTime EndDate
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

        public short FYNum
        {
            get
            {
                return m_iFYNum;
            }
            set
            {
                m_iFYNum = value;
            }
        }

        public short TotalPeriodWeights
        {
            get
            {
                return m_iTotalPeriodWeights;
            }
            set
            {
                m_iTotalPeriodWeights = value;
            }
        }

        public decimal BeginYearAccum
        {
            get
            {
                return m_curBeginYearAccum;
            }
            set
            {
                m_curBeginYearAccum = value;
            }
        }

        public decimal BeginYTDExpense
        {
            get
            {
                return m_curBeginYTDExpense;
            }
            set
            {
                m_curBeginYTDExpense = value;
            }
        }

        public decimal DeprAmount
        {
            get
            {
                return m_curDeprAmount;
            }
            set
            {
                m_curDeprAmount = value;
            }
        }

        public decimal AdjustAmount
        {
            get
            {
                return m_curAdjustAmount;
            }
            set
            {
                m_curAdjustAmount = value;
            }
        }

        public decimal Section179Change
        {
            get
            {
                return m_curSection179Change;
            }
            set
            {
                m_curSection179Change = value;
            }
        }

        public PERIODDEPRITEM_ENTRYTYPE EntryType
        {
            get
            {
                return m_eEntryType;
            }
            set
            {
                m_eEntryType = value;
            }
        }

        public double PeriodExpense
        {
            get
            {
                double dTmp;

                dTmp = (double)((m_curDeprAmount) + (m_curAdjustAmount) + (m_curSection179Change));

                return dTmp;
            }
        }

        public double BeginPeriodAccum
        {
            get
            {
                double dTmp;

                dTmp = (double)((m_curBeginYearAccum) + (m_curBeginYTDExpense));

                return dTmp;
            }

        }

        public double EndPeriodAccum
        {
            get
            {
                double dTmp;
                dTmp = (double)((m_curBeginYearAccum) + (m_curBeginYTDExpense)
                     + (m_curDeprAmount) + (m_curAdjustAmount)
                     + (m_curSection179Change));
                return dTmp;
            }

        }

        public double YTDExpense
        {
            get
            {
                double dTmp;

                dTmp = (double)((m_curBeginYTDExpense) + (m_curDeprAmount)
                     + (m_curAdjustAmount) + (m_curSection179Change));
                return dTmp;
            }

        }

        public double EndDateYTDExpense
        {
            get
            {
                double dTmp;

                dTmp = (double)((m_curBeginYearAccum) + (m_curBeginYTDExpense)
                     + (m_curDeprAmount) + (m_curAdjustAmount)
                     + (m_curSection179Change) - (m_curEndDateBeginYearAccum));
                return dTmp;
            }
        }

        public decimal CalcOverride
        {
            get
            {
                return m_calcOverride;
            }
            set
            {
                m_calcOverride = value;
            }
        }

        public string CalcFlags
        {
            get
            {
                return m_sCalcFlags;
            }
            set
            {
                m_sCalcFlags = value;
            }
        }

        public decimal EndDateBeginYearAccum
        {
            get
            {
                return m_curEndDateBeginYearAccum;
            }
            set
            {
                m_curEndDateBeginYearAccum = value;
            }
        }

        public bool Clear()
        {
 	        m_dtStartDate = DateTime.MinValue; 
	        m_dtEndDate = DateTime.MinValue; 
	        m_iFYNum = 0; 
	        m_iTotalPeriodWeights = 0; 
	        m_curBeginYearAccum = 0; 
	        m_curEndDateBeginYearAccum = 0; 
	        m_curBeginYTDExpense = 0; 
	        m_curDeprAmount = 0; 
	        m_curAdjustAmount = 0; 
	        m_curSection179Change = 0; 
	        m_EndDateDeferredAccum = 0;
	        m_EndDateYTDDeferred = 0;
	        m_calcOverride = 0;
	        m_PersUseAccum = 0;
	        m_PersUseYTD = 0;
	        m_RemainingLife = 0;
            m_eEntryType = PERIODDEPRITEM_ENTRYTYPE.PERIODDEPRITEM_NORMAL; 
	        m_sCalcFlags = String.Empty;
	        m_PersonalUseAmount = 0;
	        m_dDeprAmountMarks = 0;
	        m_dAdjustAmountMarks = 0;
	        m_dYTDDeferredMarks = 0;
	        m_dYTDPersUseMarks = 0;
	        m_dPersUseMarks = 0;
	        m_iCountToLeft = 0;
	        m_iCountToRight = 0;
	        m_iYearWeight = 0;
            return true;
        }

        public decimal EndDateDeferredAccum
        {
            get
            {
                return m_EndDateDeferredAccum;
            }
            set
            {
                m_EndDateDeferredAccum = value;
            }
        }

        public decimal EndDateYTDDeferred
        {
            get
            {
                return m_EndDateYTDDeferred;
            }
            set
            {
                m_EndDateYTDDeferred = value;
            }
        }

        public decimal EndDatePersonalUseAccum
        {
            get
            {
                return m_PersUseAccum;
            }
            set
            {
                m_PersUseAccum = value;
            }
        }

        public decimal EndDateYTDPersonalUse
        {
            get
            {
                return m_PersUseYTD;
            }
            set
            {
                m_PersUseYTD = value;
            }
        }

        public double RemainingLife
        {
            get
            {
                return m_RemainingLife;
            }
            set
            {
                m_RemainingLife = value;
            }
        }

        public double PersonalUseAmount
        {
            get
            {
                return m_PersonalUseAmount;
            }
            set
            {
                m_PersonalUseAmount = value;
            }
        }

        public bool Clone(out IBAPeriodDeprItem pVal)
        {
            pVal = null;
            string sTmp;
            PeriodDeprItem obj;

            obj = new PeriodDeprItem();
            pVal = (IBAPeriodDeprItem)obj;

            obj.m_dDeprAmountMarks = m_dDeprAmountMarks;
            obj.m_dAdjustAmountMarks = m_dAdjustAmountMarks;
            obj.m_dYTDDeferredMarks = m_dYTDDeferredMarks;
            obj.m_dYTDPersUseMarks = m_dYTDPersUseMarks;
            obj.m_dPersUseMarks = m_dPersUseMarks;
            obj.m_iCountToLeft = m_iCountToLeft;
            obj.m_iCountToRight = m_iCountToRight;
            obj.m_iYearWeight = m_iYearWeight;

            pVal.TotalPeriodWeights = TotalPeriodWeights;
            pVal.StartDate = StartDate;
            pVal.EndDate = EndDate;

            pVal.BeginYearAccum = BeginYearAccum;
            pVal.EndDateBeginYearAccum = EndDateBeginYearAccum;
            pVal.EntryType = EntryType;
            pVal.CalcFlags = CalcFlags;
            pVal.FYNum = FYNum;

            pVal.DeprAmount = DeprAmount;
            pVal.AdjustAmount = AdjustAmount;
            pVal.BeginYTDExpense = BeginYTDExpense;

            pVal.EndDateDeferredAccum = EndDateDeferredAccum;
            pVal.EndDateYTDDeferred = EndDateYTDDeferred;

            pVal.EndDatePersonalUseAccum = EndDatePersonalUseAccum;
            pVal.EndDateYTDPersonalUse = EndDateYTDPersonalUse;
            pVal.PersonalUseAmount = PersonalUseAmount;

            return true;
        }
    }
}
