using System;
using System.Collections.Generic;
using System.Text;

namespace SFABusinessTypes
{
    public class bpDeprMethodCode : bpDeprMethod
    {
        struct DMTHCODE
        {
            public bpDeprMethodTypeEnum type;
            public string code;
        };

        static DMTHCODE[] codes = new DMTHCODE[]{ 
							new DMTHCODE(){ type = bpDeprMethodTypeEnum.MacrsFormula, code = "MF"},
							new DMTHCODE(){ type = bpDeprMethodTypeEnum.MacrsTable, code = "MT"},
							new DMTHCODE(){ type = bpDeprMethodTypeEnum.AdsSlMacrs, code = "AD"},
							new DMTHCODE(){ type = bpDeprMethodTypeEnum.AcrsTable, code = "AT"},
							new DMTHCODE(){ type = bpDeprMethodTypeEnum.StraightLineAltAcrsFormula, code = "SA"},
							new DMTHCODE(){ type = bpDeprMethodTypeEnum.StraightLineAltAcrsTable, code = "ST"},
							new DMTHCODE(){ type = bpDeprMethodTypeEnum.StraightLineModHalfYear, code = "SD"},
							new DMTHCODE(){ type = bpDeprMethodTypeEnum.StraightLine, code = "SL"},
							new DMTHCODE(){ type = bpDeprMethodTypeEnum.StraightLineFullMonth, code = "SF"},
							new DMTHCODE(){ type = bpDeprMethodTypeEnum.StraightLineHalfYear, code = "SH"},
							new DMTHCODE(){ type = bpDeprMethodTypeEnum.DeclBalSwitch, code = "DB"},
							new DMTHCODE(){ type = bpDeprMethodTypeEnum.DeclBalModHalfYearSwitch, code = "DD"},
							new DMTHCODE(){ type = bpDeprMethodTypeEnum.DeclBalHalfYearSwitch, code = "DH"},
							new DMTHCODE(){ type = bpDeprMethodTypeEnum.DeclBal, code = "DC"},
							new DMTHCODE(){ type = bpDeprMethodTypeEnum.DeclBalModHalfYear, code = "DE"},
							new DMTHCODE(){ type = bpDeprMethodTypeEnum.DeclBalHalfYear, code = "DI"},
							new DMTHCODE(){ type = bpDeprMethodTypeEnum.SumOfTheYearsDigitsHalfYear, code = "YH"},
							new DMTHCODE(){ type = bpDeprMethodTypeEnum.SumOfTheYearsDigitsModHalfYear, code = "YD"},
							new DMTHCODE(){ type = bpDeprMethodTypeEnum.SumOfTheYearsDigits, code = "YS"},
							new DMTHCODE(){ type = bpDeprMethodTypeEnum.RemValueOverRemLife, code = "RV"},
							new DMTHCODE(){ type = bpDeprMethodTypeEnum.OwnDepreciationCalculation, code = "OC"},
							new DMTHCODE(){ type = bpDeprMethodTypeEnum.DoNotDepreciate, code = "NO"},
							new DMTHCODE(){ type = bpDeprMethodTypeEnum.CustomMethod, code = "cc"},
							new DMTHCODE(){ type = bpDeprMethodTypeEnum.MACRSIndianReservation, code = "MI"},
							new DMTHCODE(){ type = bpDeprMethodTypeEnum.MacrsFormula30, code = "MA"},
							new DMTHCODE(){ type = bpDeprMethodTypeEnum.AdsSlMacrs30, code = "AA"},
							new DMTHCODE(){ type = bpDeprMethodTypeEnum.MACRSIndianReservation30, code = "MR"},
							new DMTHCODE(){ type = bpDeprMethodTypeEnum.StraightLineFullMonth30, code = "SB"},
 // Canadian BEGIN !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
							new DMTHCODE(){ type = bpDeprMethodTypeEnum.CdnDeclBal, code = "DM"},
							new DMTHCODE(){ type = bpDeprMethodTypeEnum.CdnDeclBalFullMonth, code = "DL"},
							new DMTHCODE(){ type = bpDeprMethodTypeEnum.CdnDeclBalHalfYear, code = "DY"},
// Canadian END ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
							new DMTHCODE(){ type = bpDeprMethodTypeEnum.UnknownDeprMethod, code = null}
                          };



        public static bool isValidShortName(string name)
        {
            if (translateShortNameToType(name) == bpDeprMethodTypeEnum.UnknownDeprMethod)
                return false;
            else
                return true;
        }

        public static bool isValidPredefinedShortName(string name)
        {
            if (name == null || name.Length == 0)
                return false;

            bpDeprMethodTypeEnum c = translateShortNameToType(name);
            if (c == bpDeprMethodTypeEnum.UnknownDeprMethod || c == bpDeprMethodTypeEnum.CustomMethod)
                return false;

            return true;
        }


        public static bpDeprMethodTypeEnum translateShortNameToType(string shortName)
        {
            if (shortName == null || shortName.Length == 0)
                return bpDeprMethodTypeEnum.DoNotDepreciate;

            foreach (DMTHCODE code in codes)
            {
                if (code.type != bpDeprMethodTypeEnum.CustomMethod && (code.code) == shortName)
                    return code.type;
            }

            return bpDeprMethodTypeEnum.CustomMethod;
        }
        public static string translateTypeToShortName(bpDeprMethodTypeEnum newType)
        {
            foreach (DMTHCODE code in codes)
            {
                if (code.type == newType)
                    return code.code;
            }

            return null;
        }


        private bool _stable;


        public bpDeprMethodCode()
        {
            _stable = true;
            defaults();
        }

        public bpDeprMethodCode(string shortName)
        {
            _stable = true;
            if (isValidShortName(shortName))
            {
                Type = (translateShortNameToType(shortName));
                if (Type == bpDeprMethodTypeEnum.CustomMethod)
                {
                    bpCustomMethod cust = new bpCustomMethod();
                    cust.code(shortName);
                    base.CustomInfo = (cust);
                }
            }
            else
                _stable = false;
        }

        public bpDeprMethodCode(bpDeprMethod obj)
            : base(obj)
        {
            _stable = true;
        }

        public bpDeprMethodCode(bpDeprMethodCode obj)
        {
            copyFrom(obj);
        }

        //public LRbpDeprMethodCode  operator=(LRCbpDeprMethodCode object);

        //public bool                operator<(LRCbpDeprMethodCode object)
        //                          { return ( type() < object.type() ); }

        //public bool                operator==(LRCbpDeprMethodCode object)
        //                          { return inherited::operator==(object); }

        //public bool                operator!=(LRCbpDeprMethodCode object)
        //                          { return !operator==(object); }
        #region Operator Overloading

        /// <summary>
        /// OverLoading < operator
        /// </summary>
        /// <param name="left">Left value</param>
        /// <param name="right">Right value</param>
        /// <returns>return true if left value is less than the right value</returns>
        public static bool operator <(bpDeprMethodCode left, bpDeprMethodCode right)
        {
            return left.Type < right.Type;
        }

        /// <summary>
        /// OverLoading > operator
        /// </summary>
        /// <param name="left">Left value</param>
        /// <param name="right">Right value</param>
        /// <returns>return true if left value is less than the right value</returns>
        public static bool operator >(bpDeprMethodCode left, bpDeprMethodCode right)
        {
            return !(left < right);
        }

        /// <summary>
        /// OverLoading == operator
        /// </summary>
        /// <param name="left">Left value</param>
        /// <param name="right">Right value</param>
        /// <returns>return true if values are the same</returns>
        public static bool operator ==(bpDeprMethodCode left, bpDeprMethodCode right)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(left, right))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)left == null) || ((object)right == null))
            {
                return false;
            }

            return left.Type == right.Type;
        }

        /// <summary>
        /// OverLoading != operator
        /// </summary>
        /// <param name="left">Left value</param>
        /// <param name="right">Right value</param>
        /// <returns>return true if values are not the same</returns>
        public static bool operator !=(bpDeprMethodCode left, bpDeprMethodCode right)
        {
            return !(left == right);
        }

        //Always override GetHashCode(),Equals when overloading ==
        public override bool Equals(object o)
        {
            return this == (bpDeprMethodCode)o;
        }
        public override int GetHashCode()
        {
            return (int)Type;
        }

        #endregion

        public void copyFrom(bpDeprMethodCode obj)
        {
            //inherited::copyFrom( obj ); 
            _stable = obj._stable;
        }
        public string shortName()
        {
            return translateTypeToShortName(Type);
        }

        public string longName()
        {
            return "";
        }

        public bool isObjectOk()
        {
            if (base.isObjectOk() == false)
                return false;
            return _stable;
        }


    }
}
