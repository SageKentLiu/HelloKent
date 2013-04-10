using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFACalendar
{
    public enum xpNodeType { xpTopNode, xpStandalone, xpParent, xpSubNode, xpSibbling, xpEndNode } ;
    public enum cbrType { cbr_OK, cbr_NoChildren, cbr_Abort } ;


    public class XMLParser
    {
        public delegate cbrType ParserCallbackFn(BAFASCycleObject value, xpNodeType nodeType, string Name, string Attrs, string Xml);

        public XMLParser()
        {

        }

        public void SetCallback(ParserCallbackFn setTo, BAFASCycleObject value)
        {

        }

        public bool ParseXML(string xml)
        {
            return true;
        }
    }
}
