using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

using SFACalendar;

namespace SFACalcEngine
{
    public class ACEHandler
    {
        double m_curACEBasis;
        double m_curACELife;
        double m_dACETransitionYearElapsed;
        bool m_bUseACEHandling;
        DateTime m_PISDate = DateTime.MinValue;
        DateTime m_dtACEStart;
        short m_iAceTransitionYear;
        IBADeprScheduleItem m_pObjSchedule;
        IBAACEInformation m_pObjACEInfo;
        IBACalendar m_pObjCalendar;


        public ACEHandler()
        {
            m_PISDate = DateTime.MinValue;
            m_bUseACEHandling = false;
            m_curACEBasis = 0;
            m_curACELife = 0;
            m_dACETransitionYearElapsed = 0;
            m_iAceTransitionYear = 0;
        }

        public DateTime PISDate
        {
            get { return m_PISDate; }
            set { m_PISDate = value; }
        }

        public IBADeprScheduleItem DeprScheduleItem
        {
            get { return m_pObjSchedule; }
            set { m_pObjSchedule = value; }
        }

        public bool UseACEHandling
        {
            get { return m_bUseACEHandling; }
        }

        public IBACalendar Calendar
        {
            get { return m_pObjCalendar; }
            set { m_pObjCalendar = value; }
        }

        public double ACEBasis
        {
            get { return m_curACEBasis; }
        }

        public double ACELife
        {
            get { return m_curACELife; }
        }

	    public bool FirstACEProcess(short PISYearNumber, short iCurrentYearNumber, ref string deprMethod, ref string flags)
        {
	        string DeprMethod;
	        string Flags;
	        bool hr;

	        if ( m_pObjSchedule == null || m_pObjCalendar == null )
		        throw new Exception("Error in FirstACEProcess.");

	        if ( deprMethod == string.Empty )
	        {
		        DeprMethod = m_pObjSchedule.DeprMethod;
	        }
	        else
	        {
		        DeprMethod = deprMethod;
	        }

	        if ( flags == string.Empty )
	        {
		        Flags = "";
	        }
	        else
	        {
		        Flags = flags;
	        }

	        if ( m_bUseACEHandling )
	        {
		        IBAFiscalYear fy;

		        //
		        // Here we need to determine the year number of the ACE transition.  ACE begins 
		        // on the first year that starts on or after 1/1/1990.
		        //
		        if ( !(hr = m_pObjCalendar.GetFiscalYear(new DateTime(1990,1,1), out fy)) )
                {
                    m_iAceTransitionYear = -1;
                }

		        m_dtACEStart = fy.YRStartDate;
		        m_iAceTransitionYear = fy.FYNum;

		        //
		        // If the year start is on 1/1/1990, the transition year is one year earlier.
		        //
                if (m_dtACEStart == new DateTime(1900, 1, 1))
                {
                    m_iAceTransitionYear--;
                }

		        //
		        // Now that we have the transition year number, get the actual ACE start date.
		        //
		        if ( m_iAceTransitionYear > 0 )
		        {
			        fy = null;
			        if (!(hr = m_pObjCalendar.GetFiscalYearByNum((short)(m_iAceTransitionYear + 1), out fy)) )
                        return hr;
			        m_dtACEStart = fy.YRStartDate;
		        }
		        //
		        // Now we need to see if we are already in ACE.  If so we need to change the 
		        // depr method accordingly.
		        //
		        if ( m_PISDate < new DateTime(1981,1,1) )
		        {
			        // No depr method changes allowed here.
			        m_bUseACEHandling = false; // reset the flag because it is not needed.
		        }
		        else if ( PISYearNumber > m_iAceTransitionYear )
		        {
			        // No changes here, the input D&V should already have handled this.
			        m_bUseACEHandling = false; // reset the flag because it is not needed.
		        }
		        else
		        {
			        if ( string.Compare (DeprMethod, "AST") == 0 || string.Compare (DeprMethod, "ASF") == 0 ||
				         string.Compare (DeprMethod, "AT")  == 0 || string.Compare (DeprMethod, "MT")  == 0 ||
				         string.Compare (DeprMethod, "MF")  == 0 || string.Compare (DeprMethod, "MAT") == 0 ||
				         string.Compare (DeprMethod, "MAF") == 0 || string.Compare (DeprMethod, "MST") == 0 ||
				         string.Compare (DeprMethod, "MSF") == 0 )
			        {
				        decimal cyACEBasis;

				        // Now see if we should already be switched to RV.
				        if ( iCurrentYearNumber > m_iAceTransitionYear + 1 ||
					         string.Compare (Flags, "v") != 0 )
				        {
                            if (string.Compare(Flags, "v") == 0)
						        Flags += "v";
					        DeprMethod = "RV"; // Force the depr method to RV.
				        }
				        cyACEBasis = m_pObjSchedule.ACEBasis;
                        m_curACELife = m_pObjSchedule.ACELife;
				        m_curACEBasis = CurrencyHelper.CurrencyToDouble(cyACEBasis);
			        }
			        else
			        {
				        // Not ACRS or MACRS, therefore ACE handling not needed.
				        m_bUseACEHandling = false; // reset the flag because it is not needed.
			        }
		        }
	        }
            deprMethod = DeprMethod;
	        flags = Flags;
            return true;
        }

	    public bool InitializeACEDeprMethod(short iCurrentYearNumber, string flags, double dYearElapsed, double priorAccum, IBADeprMethod pObjDeprMethod)
        {
	        bool hr;

	        if ( pObjDeprMethod == null || flags == null )
                throw new Exception("Error in InitializeACEDeprMethod.");
	        //
	        // Now we need to adjust the depr values for ACE.
	        //
	        if ( m_bUseACEHandling &&
		         (iCurrentYearNumber > m_iAceTransitionYear + 1 ||
		          string.Compare (flags, "v") != 0) )
	        {
		        double ADSLife;
		        ADSLife = m_pObjSchedule.ADSLife;		// by KENT	fix MS MC-00037
		        pObjDeprMethod.Life = ADSLife;		// by KENT  fix MS MC-00037
		        pObjDeprMethod.YearElapsed = dYearElapsed;
		        pObjDeprMethod.SalvageDeduction = 0;
		        pObjDeprMethod.PriorAccum = priorAccum;
	        }
	        return true;
        }

        public bool CalculateACEYearInformation(CalcEngine calcEngine, IBAAvgConvention pObjAvgConvetion)
        {
            if (calcEngine == null || pObjAvgConvetion == null)
                throw new Exception("Error in CalculateACEYearInformation.");

            if (m_pObjSchedule == null || m_pObjCalendar == null)
                throw new Exception("Error in CalculateACEYearInformation.");

            if (m_bUseACEHandling && m_iAceTransitionYear > 0)
            {
                m_dACETransitionYearElapsed = calcEngine.CalculateYearElapsed(m_pObjSchedule, pObjAvgConvetion, m_pObjCalendar, m_dtACEStart);
            }
            else
                m_dACETransitionYearElapsed = 0;
            return true;
        }

        public bool ProcessACEForYear(short iFYNum, IBADeprMethod pObjDeprMethod, double dActualPriorAccum, CalcEngine calcEngine, IBAAvgConvention pObjAvgConvetion, double dYearElapsed, out IBADeprMethod outMethod)
        {
	        bool hr;
            outMethod = null;

	        if ( m_pObjSchedule == null || m_pObjCalendar == null )
                throw new Exception("Error in ProcessACEForYear.");

	        if ( outMethod != null )
	        {
		        outMethod = null;
	        }
	        if ( pObjDeprMethod == null || calcEngine == null || pObjAvgConvetion == null )
                throw new Exception("Error in ProcessACEForYear.");

	        outMethod = pObjDeprMethod;

	        if ( m_bUseACEHandling && iFYNum == m_iAceTransitionYear + 1 )
	        {
		        double ACEBasis;
		        double ACELife;
		        double basis;
		        double yearElapsed = 0;
		        double ADSLife;
		        string deprmethod;
		        decimal curTmp;
		        short yearNum = 0;

		        basis       = pObjDeprMethod.Basis;
		        ACELife     = pObjDeprMethod.Life;
		        deprmethod  = m_pObjSchedule.DeprMethod;
		        ADSLife     = m_pObjSchedule.ADSLife;

		        ACEBasis = basis - dActualPriorAccum;
		        if ( String.Compare(deprmethod, "MT")  == 0 ||
			         String.Compare(deprmethod, "MF")  == 0 ||
			         String.Compare(deprmethod, "MAT") == 0 ||
			         String.Compare(deprmethod, "MAF") == 0 ||
			         String.Compare(deprmethod, "MST") == 0 ||
			         String.Compare(deprmethod, "MSF") == 0 ||
			         String.Compare(deprmethod, "AT")  == 0 ||
			         String.Compare(deprmethod, "AST") == 0 ||
			         String.Compare(deprmethod, "ASF") == 0 )
		        {
			        if ( m_pObjACEInfo == null )
			        {
				        ACELife = m_curACELife;
				        ACEBasis = m_curACEBasis;
			        }
			        else
			        {
				        if ( m_PISDate >= m_dtACEStart )
				        {
					        ACEBasis = basis;

				        }
				        else
				        {
					        ACEBasis = basis - dActualPriorAccum;
					        ACELife = ADSLife - yearElapsed;
				        }
				        CurrencyHelper.DoubleToCurrency(ACEBasis, out curTmp);
				        m_pObjACEInfo.ACELife = ACELife;
				        m_pObjACEInfo.ACEBasis = curTmp;
				
			        }
			        //
			        // Now we need to create the RV method and initialize it.
			        //
			        outMethod = null;
			        dYearElapsed = yearElapsed;
			        if ( !(hr = calcEngine.CreateDeprMethod("RV", false, out outMethod)) ||
                         !(hr = calcEngine.InitializeDeprMethod(m_pObjSchedule, pObjAvgConvetion, outMethod)))
                        return hr;
			        outMethod.Life = ACELife + m_dACETransitionYearElapsed;
			        outMethod.SalvageDeduction = 0;
			        outMethod.YearElapsed = dYearElapsed;
			        outMethod.YearNum = yearNum;
			        outMethod.PriorAccum = dActualPriorAccum;
		        }
		        else
		        {
		        }
	        }

            return true;
        }

	    public bool get_ACEFlag(short iFYNum, out string pVal)
        {
            string tmp;

            tmp = "";
            if (m_bUseACEHandling && iFYNum > m_iAceTransitionYear)
                tmp = "v";
            pVal = tmp; 
            return true;
        }

    }
}
