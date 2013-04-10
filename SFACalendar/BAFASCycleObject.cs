using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFACalendar
{
    public class BAFASCycleObject : IBACycleObject, IBAPersistXML
    {
        DateTime m_dtEffectiveDate;
        DateTime m_dtEndDate;
        ECALENDARCYCLE_CYCLETYPE m_eCycleType;
        ECALENDARCYCLE_DATEOFWEEK m_eDayWeekEnum;
        ECALENDARCYCLE_PDCOUNTING m_ePDCounting;
        ECALENDARCYCLE_YEARENDELECTION m_eYearEndElect;
        short m_iFYEndMonthEnum;
        short m_iYearNumberOffset;
        bool m_IsDirty;
        bool m_translate;

        IBAPersistXML m_persistance;

        public static cbrType ParserCallback(BAFASCycleObject value, xpNodeType nodeType, string Name, string Attrs, string Xml)
        {
            BAFASCycleObject This = (BAFASCycleObject)value;

            if (string.Compare("Cycle", Name) == 0)
            {
                This.AttributeString = (Attrs);

                return cbrType.cbr_OK;
            }
            else
                return cbrType.cbr_OK;

        }

        public BAFASCycleObject()
        {
            m_persistance = new XMLPersistance();
        }

        public ECALENDARCYCLE_CYCLETYPE CycleType
        {
            get
            {
                return m_eCycleType;
            }
            set
            {
                if (value != m_eCycleType)
                    m_IsDirty = true;

                m_eCycleType = value;
                m_persistance.set_Attribute("CycleType", m_eCycleType.ToString());
            }
        }

        public ECALENDARCYCLE_DATEOFWEEK DateOfWeek
        {
            get
            {
                return m_eDayWeekEnum;
            }
            set
            {
                if (value != m_eDayWeekEnum)
                    m_IsDirty = true;

                m_persistance.set_Attribute("DayOfWeek", value.ToString());
                m_eDayWeekEnum = value;
            }
        }

        public ECALENDARCYCLE_PDCOUNTING PDCounting
        {
            get
            {
                return m_ePDCounting;
            }
            set
            {
                if (value != m_ePDCounting)
                    m_IsDirty = true;

                m_persistance.set_Attribute("PDCounting", value.ToString());
                m_ePDCounting = value;
            }
        }

        public ECALENDARCYCLE_YEARENDELECTION YearEndElect
        {
            get
            {
                return m_eYearEndElect;
            }
            set
            {
                if (value != m_eYearEndElect)
                    m_IsDirty = true;

                m_persistance.set_Attribute("YearEndElection", value.ToString());
                m_eYearEndElect = value;
            }
        }

        public short FYEndMonth
        {
            get
            {
                return m_iFYEndMonthEnum;
            }
            set
            {

                if (value != m_iFYEndMonthEnum)
                    m_IsDirty = true;

                m_persistance.set_Attribute("FYEndMonth", value.ToString());
                m_iFYEndMonthEnum = value;
            }
        }

        public DateTime EffectiveDate
        {
            get
            {
                return m_dtEffectiveDate;
            }
            set
            {
                string tmp;
                if (value != m_dtEffectiveDate)
                    m_IsDirty = true;

                if (value == DateTime.MinValue)
                    tmp = "12/29/1899";
                else
                {
                    //sprintf (buff, "%d/%d/%d", Month(value), Day(value), Year(value));
                    tmp = value.ToShortDateString();
                }
                m_persistance.set_Attribute("Effective", tmp);
                m_dtEffectiveDate = value;
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
                string tmp;

                if (value != m_dtEndDate)
                    m_IsDirty = true;

                if (value == DateTime.MinValue)
                    tmp = "12/29/1899";
                else
                {
                    tmp = value.ToShortDateString();
                }
                m_persistance.set_Attribute("EndDate", tmp);
                m_dtEndDate = value;
            }
        }

        public short YearNumberOffset
        {
            get
            {
                return m_iYearNumberOffset;
            }
            set
            {

                if (value != m_iYearNumberOffset)
                    m_IsDirty = true;

                m_persistance.set_Attribute("YearNumberOffset", value.ToString());
                m_iYearNumberOffset = value;
            }
        }

        public short NumberOfYears
        {
            get
            {
                short iNumYears;

                if (m_eCycleType != ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_CUSTOM)
                {
                    if (m_dtEndDate <= DateTime.MinValue)
                        iNumYears = 0;
                    else
                    {
                        iNumYears = (short)((m_dtEndDate.Year) - (m_dtEffectiveDate.Year));
                        if (((m_dtEndDate.Month) == 1 && (m_dtEndDate.Day) < 7))
                            iNumYears--;

                        if (m_iFYEndMonthEnum > (m_dtEffectiveDate.Month) ||
                             (m_iFYEndMonthEnum > (m_dtEffectiveDate.Month) &&
                              m_dtEndDate < DateTimeHelper.GetEndOfMonth(m_dtEndDate.Year, m_dtEndDate.Month).AddDays(-7)) ||
                             ((m_dtEffectiveDate.Month) == m_iFYEndMonthEnum && (m_dtEffectiveDate.Day) <= 7))
                            iNumYears++;
                    }
                }
                else
                {
                    //
                    // Need code here when we know how to deal with Custom Cycles.
                    //
                    iNumYears = 0;
                }

                return iNumYears;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool IsDirty
        {
            get
            {
                return m_IsDirty;
            }
            set
            {
                m_IsDirty = value;
            }
        }

        public string XML
        {
            get
            {
                string xml;
                string attrs;

                xml = "<Cycle ";

                attrs = AttributeString;

                xml += attrs;
                xml += "/>";

                return xml;
            }
            set
            {
                XMLParser parser = new XMLParser();
                bool hr;

                if (m_persistance == null)
                    throw new Exception("Persistance module does not support IBAPersistXML.");

                if (!(hr = RemoveAllAttributes()))
                    throw new Exception("Persistance module does not support IBAPersistXML.");

                parser.SetCallback(ParserCallback, (this));
                hr = parser.ParseXML(value);

                m_IsDirty = true;
            }
        }

        public string AttributeString
        {
            get
            {
                if (m_persistance == null)
                    throw new Exception("Persistance module does not support IBAPersistXML.");

                string str = m_persistance.AttributeString;

                if (m_translate)
                {
                    str.Replace("CycleType=\"332\"", "CycleType=\"Custom\"");
                    str.Replace("CycleType=\"331\"", "CycleType=\"13\"");
                    str.Replace("CycleType=\"330\"", "CycleType=\"544\"");
                    str.Replace("CycleType=\"329\"", "CycleType=\"454\"");
                    str.Replace("CycleType=\"328\"", "CycleType=\"445\"");
                    str.Replace("CycleType=\"327\"", "CycleType=\"Monthly\"");
                      
                    str.Replace("DayOfWeek=\"1\"", "DayOfWeek=\"Sunday\"");
                    str.Replace("DayOfWeek=\"2\"", "DayOfWeek=\"Monday\"");
                    str.Replace("DayOfWeek=\"3\"", "DayOfWeek=\"Tuesday\"");
                    str.Replace("DayOfWeek=\"4\"", "DayOfWeek=\"Wednesday\"");
                    str.Replace("DayOfWeek=\"5\"", "DayOfWeek=\"Thursday\"");
                    str.Replace("DayOfWeek=\"6\"", "DayOfWeek=\"Friday\"");
                    str.Replace("DayOfWeek=\"7\"", "DayOfWeek=\"Saturday\"");

                    str.Replace("PDCounting=\"334\"", "PDCounting=\"Backward\"");
                    str.Replace("PDCounting=\"335\"", "PDCounting=\"BackwardOld\"");
                    str.Replace("PDCounting=\"333\"", "PDCounting=\"Forward\"");
                      
                    str.Replace("YearEndElection=\"337\"", "YearEndElection=\"ClosestWeekDay\"");
                    str.Replace("YearEndElection=\"336\"", "YearEndElection=\"LastWeekDay\"");
                }
                return str;
            }
            set
            {
                IBAPersistXML tmpPersist = new XMLPersistance();
                int count;
                int posi;
                string name;
                bool hr = false;

                if ( m_persistance == null )
                    throw new Exception ("Persistance module does not support IBAPersistXML.");

                tmpPersist.AttributeString = value;
                count = tmpPersist.AttributeCount;

                for (posi = 0; posi < count; posi++)
                {
                    name = "";
                    value = "";
                    if ( !(hr = tmpPersist.AttributeName(posi, out name)) ||
                         !(hr = tmpPersist.get_Attribute(name, out value)) ||
                         !(hr = set_Attribute(name, value)) )
                    {
                        throw new Exception ("Persistance module does not support IBAPersistXML.");
                    }
                }

                m_IsDirty = true;            
            }
        }

        public bool TranslateEnums
        {
            get
            {
                return m_translate;
            }
            set
            {
                m_translate = value;
            }
        }

        public int AttributeCount
        {
            get
            {
                if (m_persistance == null)
                    throw new Exception("Persistance module does not support IBAPersistXML.");

                return m_persistance.AttributeCount;
            }
        }

        public bool AttributeName(int index, out string pVal)
        {
            if ( m_persistance == null )
                throw new Exception("Persistance module does not support IBAPersistXML.");

	        return m_persistance.AttributeName(index, out pVal);
        }

        public bool get_Attribute(string name, out string pVal)
        {
            if ( m_persistance == null )
                throw new Exception("Persistance module does not support IBAPersistXML.");

	        return m_persistance.get_Attribute (name, out pVal);
        }

        public bool set_Attribute(string name, string newVal)
        {
            if ( m_persistance == null )
                throw new Exception("Persistance module does not support IBAPersistXML.");

            if (string.Compare (name, "Effective") == 0)
            {
                DateTime dt;

                dt = Convert.ToDateTime(newVal);
                EffectiveDate = dt;
            }
            else if (string.Compare(name, "EndDate") == 0)
            {
                DateTime dt;

                dt = Convert.ToDateTime(newVal);
                EndDate = dt;
            }
            else if (string.Compare(name, "CycleType") == 0)
            {
                if (string.Compare(newVal, "Monthly") == 0)
                    CycleType = (ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_MONTHLY);
                else if (string.Compare(newVal, "445") == 0)
                    CycleType = (ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FOURFOURFIVE);
                else if (string.Compare(newVal, "454") == 0)
                    CycleType = (ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FOURFIVEFOUR);
                else if (string.Compare(newVal, "544") == 0)
                    CycleType = (ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_FIVEFOURFOUR);
                else if (string.Compare(newVal, "13") == 0)
                    CycleType = (ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_THIRTEENPERIOD);
		        else
			        CycleType = (ECALENDARCYCLE_CYCLETYPE.CYCLETYPE_MONTHLY);
            }
            else if (string.Compare(name, "DayOfWeek") == 0)
            {
                if (string.Compare(newVal, "Sunday") == 0)
                    DateOfWeek = (ECALENDARCYCLE_DATEOFWEEK.DATEOFWEEK_SUNDAY);
                else if (string.Compare(newVal, "Monday") == 0)
                    DateOfWeek = (ECALENDARCYCLE_DATEOFWEEK.DATEOFWEEK_MONDAY);
                else if (string.Compare(newVal, "Tuesday") == 0)
                    DateOfWeek = (ECALENDARCYCLE_DATEOFWEEK.DATEOFWEEK_TUESDAY);
                else if (string.Compare(newVal, "Wednesday") == 0)
                    DateOfWeek = (ECALENDARCYCLE_DATEOFWEEK.DATEOFWEEK_WEDNESDAY);
                else if (string.Compare(newVal, "Thursday") == 0)
                    DateOfWeek = (ECALENDARCYCLE_DATEOFWEEK.DATEOFWEEK_THURSDAY);
                else if (string.Compare(newVal, "Friday") == 0)
                    DateOfWeek = (ECALENDARCYCLE_DATEOFWEEK.DATEOFWEEK_FRIDAY);
                else if (string.Compare(newVal, "Saturday") == 0)
                    DateOfWeek = (ECALENDARCYCLE_DATEOFWEEK.DATEOFWEEK_SATURDAY);
		        else 
                    DateOfWeek = (ECALENDARCYCLE_DATEOFWEEK.DATEOFWEEK_SUNDAY);
            }
            else if (string.Compare(name, "PDCounting") == 0)
            {
                if (string.Compare(newVal, "Forward") == 0)
                    PDCounting = (ECALENDARCYCLE_PDCOUNTING.PDCOUNT_FORWARD);
                else if (string.Compare(newVal, "Backward") == 0)
                    PDCounting = (ECALENDARCYCLE_PDCOUNTING.PDCOUNT_BACKWARD);
                else if (string.Compare(newVal, "BackwardOld") == 0)
                    PDCounting = (ECALENDARCYCLE_PDCOUNTING.PDCOUNT_BACKWARD_OLDMONTH);
		        else 
                    PDCounting = (ECALENDARCYCLE_PDCOUNTING.PDCOUNT_FORWARD);
            }
            else if (string.Compare(name, "YearEndElection") == 0)
            {
                if (string.Compare(newVal, "LastWeekDay") == 0)
                    YearEndElect = (ECALENDARCYCLE_YEARENDELECTION.YEARENDELECTION_LASTWEEKDAY);
                else if (string.Compare(newVal, "ClosestWeekDay") == 0)
                    YearEndElect = (ECALENDARCYCLE_YEARENDELECTION.YEARENDELECTION_CLOSESTWEEKDAY);
		        else 
                    YearEndElect = (ECALENDARCYCLE_YEARENDELECTION.YEARENDELECTION_LASTWEEKDAY);
            }
            else if (string.Compare(name, "FYEndMonth") == 0)
            {
                short value;

                value = Convert.ToInt16 (newVal);
                if (value < 1 || value > 12)
                    throw new Exception("Invalid Fiscal Year End Month");
                FYEndMonth = value;
            }
            else if (string.Compare(name, "YearNumberOffset") == 0)
            {
                short value;

                value = Convert.ToInt16 (newVal);
                if ( value < 0 || value > 1000)
                    throw new Exception("Invalid Year number offset");
                YearNumberOffset = value;
            }

            m_IsDirty = true;
	        return m_persistance.set_Attribute (name, newVal);
        }

        public bool AttributeID(string name, out int pVal)
        {
            if ( m_persistance == null )
                throw new Exception("Persistance module does not support IBAPersistXML.");

	        return m_persistance.AttributeID(name, out pVal);
        }

        public bool RemoveAttribute(string name)
        {
             if ( m_persistance == null )
                throw new Exception("Persistance module does not support IBAPersistXML.");

            m_IsDirty = true;
	        return m_persistance.RemoveAttribute (name);
        }

        public bool RemoveAllAttributes()
        {
             if ( m_persistance == null )
                throw new Exception("Persistance module does not support IBAPersistXML.");

            m_IsDirty = true;
            return m_persistance.RemoveAllAttributes();
        }
    }
}
