using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace SFACalendar
{
    public class XMLPersistance : IBAPersistXML
    {
        NameValueCollection m_attrList;
        bool m_translate;

        public XMLPersistance()
        {
            m_attrList = new NameValueCollection();
            m_translate = false;

        }

        public string XML
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string AttributeString
        {
            get
            {
                int idx;
                string name = string.Empty;
                string value = string.Empty;
                string attrs = string.Empty;
                bool hr;

                if (m_attrList == null)
                    throw new Exception("Attribute list not initialized.");

                for (idx = 0; idx < m_attrList.Count; idx++)
                {
                    name = m_attrList.GetKey(idx);
                    value = m_attrList.Get(idx);

                    attrs += name;
                    attrs += "=\"";
                    attrs += value;
                    attrs += "\" ";
                }

                return attrs;
            }
            set
            {
                string tmp;
                string name;
                string valueString;
                bool hr;
                string rest;
                string outString;

                if (m_attrList == null)
                    throw new Exception("Attribute list not initialized.");

                tmp = value;

                m_attrList.Clear();
                rest = tmp;
                do
                {
                    hr = GetName(rest, out name, out outString);
                    if (hr == true)
                    {
                        rest = outString;
                        hr = GetValue(rest, out valueString, out outString);
                        if (hr == true)
                        {
                            rest = outString;
                            m_attrList.Add(name, valueString);
                        }
                    }
                }
                while (hr == true);

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
            get { return m_attrList.Count; }
        }

        public bool AttributeName(int index, out string pVal)
        {
            if (m_attrList == null)
                throw new Exception("Attribute list not initialized.");
            pVal = m_attrList.GetKey(index);
            return true;
        }

        public bool get_Attribute(string name, out string pVal)
        {
            if (m_attrList == null)
                throw new Exception("Attribute list not initialized.");

            pVal = m_attrList.Get(name);
            return true;
        }

        public bool set_Attribute(string name, string newVal)
        {
            long idx;

            if (m_attrList == null)
                throw new Exception("Attribute list not initialized.");

            if (m_attrList[name] == null)
            {
                m_attrList.Add(name, newVal);
            }
            else
            {
                m_attrList.Set(name, newVal);
            }
            return true;
        }

        public bool AttributeID(string name, out int pVal)
        {
            pVal = -1;

            if (m_attrList == null)
                throw new Exception("Attribute list not initialized.");

            int idx = Enumerable.Range(0, m_attrList.Keys.Count - 1).First(i => m_attrList.Keys[i] == name);
            pVal = idx;
            return true;
        }

        public bool RemoveAttribute(string name)
        {
            bool hr;

            if (m_attrList == null)
                throw new Exception("Attribute list not initialized.");

            int idx = Enumerable.Range(0, m_attrList.Keys.Count - 1).First(i => m_attrList.Keys[i] == name);

            if (idx < 0)
                return true;

            m_attrList.Remove(name);
            return true;
        }

        public bool RemoveAllAttributes()
        {
            if (m_attrList == null)
                throw new Exception("Attribute list not initialized.");

            m_attrList.Clear();
            return true;
        }


        bool EatSpace(string inString, out string outString)
        {
            int i = 0;

            outString = null;
            if (inString == null)
                return false;

            while (inString[i] == 32 || inString[i] == 9 || inString[i] == 10 || inString[i] == 13)
                i++;
            outString = inString.Substring(i, inString.Length - i);
            if (inString[i] == 0)
                return false;
            return true;
        }

        bool GetName(string inString, out string name, out string outString)
        {
            int i = 0;
            bool hr;

            hr = EatSpace(inString, out inString);
            name = null;
            outString = null;
            if (hr != true)
                return hr;
            name = inString;
            while ((inString[i] >= 'a' && inString[i] <= 'z') || (inString[i] >= 'A' && inString[i] <= 'Z') ||
                   (inString[i] >= '0' && inString[i] <= '9') || inString[i] == '_' || inString[i] == '-')
                i++;
            outString = inString.Substring(i, inString.Length - i);
            if (inString[i] == 0)
                return false;
            else if (inString[i] == '=')
            {
                i++;
                outString = inString.Substring(i, inString.Length - i);
                return true;
            }
            else
                throw new Exception("Improperly formatted string");
        }

        bool GetValue(string inString, out string value, out string outString)
        {
            int i = 0;

            value = null;
            outString = null;

            if (inString[i] != '"')
                throw new Exception("Improperly formatted string");

            i++;
            value = inString.Substring(i, inString.Length - i);
            while (inString[i] != '"' && inString[i] != 0)
                i++;
            outString = inString.Substring(i, inString.Length - i);
            if (inString[i] == 0)
                return false;
            else if (inString[i] == '"')
            {
                i++;
                outString = inString.Substring(i, inString.Length - i);
                return true;
            }
            else
                throw new Exception("Improperly formatted string");
        }


    }
}
