using System;
using System.Collections.Generic;
using System.Text;

namespace SFABusinessTypes
{
    public class bpDisposalConventionCode : bpDisposalConvention
    {
        public bpDisposalConventionCode(bpDisposalConventionCode obj)
        { copyFrom(obj); }

        public bpDisposalConventionCode(bpDisposalConvention obj) :
            base(obj)
        { }

        public bpDisposalConventionCode(char shortName)
        {
            defaults();
            if (isValidName(shortName) == false)
                Type = (DispConvType.Unknown);
            else
                Type = (translateShortNameToType(shortName));
        }

        //public LRbpDisposalConventionCode operator=(LRCbpDisposalConventionCode obj);

        //public bool                operator==(LRCbpDisposalConventionCode obj)
        //                          { return (type()==obj.type()) ? true : false; }

        //public bool                operator!=(LRCbpDisposalConventionCode obj)
        //                          { return !operator==(obj); }
        #region Operator Overloading

        /// <summary>
        /// OverLoading == operator
        /// </summary>
        /// <param name="left">Left value</param>
        /// <param name="right">Right value</param>
        /// <returns>return true if values are the same</returns>
        public static bool operator ==(bpDisposalConventionCode left, bpDisposalConventionCode right)
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
        public static bool operator !=(bpDisposalConventionCode left, bpDisposalConventionCode right)
        {
            return !(left == right);
        }

        //Always override GetHashCode(),Equals when overloading ==
        public override bool Equals(object o)
        {
            return this == (bpDisposalConventionCode)o;
        }
        public override int GetHashCode()
        {
            return (int)Type;
        }

        #endregion

        public void copyFrom(bpDisposalConventionCode obj)
        { base.copyFrom(obj); }

        public char shortName()
        { return translateTypeToShortName(Type); }

        public string longName()
        { return translateTypeToLongName(Type); }

        public override bool isObjectOk()
        { return base.isObjectOk(); }

        public static bool isValidName(char name)
        {
            if (translateShortNameToType(name) == DispConvType.Unknown)
                return false;
            return true;
        }

        private static DispConvType translateShortNameToType(char shortName)
        {
            switch (shortName)
            {
                case 'F':
                    return DispConvType.FullMonth;
                case 'M':
                    return DispConvType.Midmonth;
                case 'H':
                    return DispConvType.HalfYearACRS;
                case 'P':
                    return DispConvType.HalfYearMACRSPreACRS;
                case 'O':
                    return DispConvType.ModifiedHalfYear;
                default:
                    return DispConvType.Unknown;
            }
        }

        private static char translateTypeToShortName(DispConvType type)
        {
            switch (type)
            {
                case DispConvType.FullMonth:
                    return 'F';
                case DispConvType.Midmonth:
                    return 'M';
                case DispConvType.HalfYearACRS:
                    return 'H';
                case DispConvType.HalfYearMACRSPreACRS:
                    return 'P';
                case DispConvType.ModifiedHalfYear:
                    return 'O';
                case DispConvType.Unknown:
                default:
                    return '\0';
            }
        }
        private static string translateTypeToLongName(DispConvType type)
        {
            switch (type)
            {
                case DispConvType.FullMonth:
                    return "Full Month";
                case DispConvType.Midmonth:
                    return "Midmonth";
                case DispConvType.HalfYearACRS:
                    return "Half-Year (ACRS)";
                case DispConvType.HalfYearMACRSPreACRS:
                    return "Half-Year (MACRS & Pre-ACRS)";
                case DispConvType.ModifiedHalfYear:
                    return "Modified Half-Year";
                default:
                    return (string)null;
            }
        }
    }
}
