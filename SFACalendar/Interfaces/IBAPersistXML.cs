using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFACalendar
{
    public interface IBAPersistXML
    {
        string XML { get; set; }
        string AttributeString { get; set; }
        bool TranslateEnums { get; set; }
        int AttributeCount { get; }

        bool AttributeName(int index, out string pVal);
		bool  get_Attribute(string name, out string pVal);
		bool  set_Attribute(string name, string newVal);
        bool AttributeID(string name, out int pVal);
		bool  RemoveAttribute(string name);
		bool  RemoveAllAttributes();

    }
}
