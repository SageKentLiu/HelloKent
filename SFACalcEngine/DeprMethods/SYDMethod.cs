using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFACalcEngine
{
    class SYDMethod : IBADeprMethod
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
        private double m_dSYD;
        
        public SYDMethod()
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
	        m_dSYD = 0;
	        m_parentFlags = "";
	        m_VintageAccount = false;
        }

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

        public bool IsFiscalYearBased
        {
            get { return true; }
        }

        public double Life
        {
            get
            {
                return m_dLife;
            }
            set
            {
                int		iIntPart;
                double	dblFrcPart;
                if ( value <= 0 )
                    throw new Exception ("Invalid value in SYDMethod Life");

                m_dLife = value;
    
                iIntPart = Convert.ToInt32(m_dLife);
                dblFrcPart = m_dLife - iIntPart;
                m_dSYD = (iIntPart * (iIntPart + 1) / 2) + (iIntPart + 1) * dblFrcPart;
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

        public double CalculateAnnualDepr()
        {
            int			iCurCalYear;
            double		dblCurCalYearUsed;
            double		dblFractor;
            double		dblFYDepr;
    
            iCurCalYear = ((int)(m_dYearElapsed)) + 1;
            dblCurCalYearUsed = m_dYearElapsed - ((int)(m_dYearElapsed));
            if( m_dFiscalYearFraction > iCurCalYear - m_dYearElapsed )
            {
		        dblFractor = Convert.ToDouble(iCurCalYear) - m_dYearElapsed;
	        }
	        else
            {
		        dblFractor = m_dFiscalYearFraction;
	        }
            dblFYDepr = CalendarDepr(iCurCalYear) * dblFractor;
            if( m_dFiscalYearFraction > iCurCalYear - m_dYearElapsed )
	        {
                dblFYDepr = dblFYDepr + (m_dFiscalYearFraction - Convert.ToDouble(iCurCalYear) + m_dYearElapsed) * 
                            CalendarDepr(iCurCalYear + 1);
            }
            return dblFYDepr;
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
	            if ( m_VintageAccount )
	            {
		            return m_dAdjustedCost - m_dPostUsedDeduction - 
			                m_dPriorAccum;
	            }
	            else
	            {
		            return m_dAdjustedCost - m_dPostUsedDeduction - 
			                m_dSalvageDeduction - m_dPriorAccum;
	            }
            }
        }

        public bool GetAvgConvention(IBADeprScheduleItem schedule, ref string pVal)
        {
            return false;
        }

       public string BaseShortName
        {
            get { return "SYD"; }
        }

        public string BaseLongName
        {
            get { return "Sum Of the Years Digits"; }
        }

        double CalendarDepr(int iYearNum) 
        {
            double dblRLife;
	        double dblBasis;
    
            if( m_dSYD <= 0 )
	        {
		        return 0;
            }
    
            dblRLife = m_dLife + 1.0 - iYearNum;
	        dblBasis = Basis;
            if( dblRLife < 1.0 )
            {
		        return dblBasis / m_dSYD;
	        }
	        else
            {
		        return dblBasis * dblRLife / m_dSYD;
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
            m_VintageAccount = schedule.VintageAccountFlag;

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
                double basis = Basis;
                double salvage = SalvageDeduction;

                if (m_VintageAccount)
                {
                    return basis - salvage;
                }
                else
                    return basis;
            }
        }
    }
}
