using System;
using System.Collections.Generic;
using System.Text;

namespace SFABusinessTypes
{
    public enum bpDeprMethodTypeEnum
    {
        InvalidDeprMethod = 0,
        MacrsFormula = 1,
        MacrsTable,
        AdsSlMacrs,
        AcrsTable,
        StraightLineAltAcrsFormula,
        StraightLineAltAcrsTable,
        StraightLine,
        StraightLineFullMonth,
        StraightLineHalfYear,
        StraightLineModHalfYear,
        DeclBal,
        DeclBalHalfYear,
        DeclBalModHalfYear,
        DeclBalSwitch,
        DeclBalHalfYearSwitch,
        DeclBalModHalfYearSwitch,
        SumOfTheYearsDigits,
        SumOfTheYearsDigitsHalfYear,
        SumOfTheYearsDigitsModHalfYear,
        RemValueOverRemLife,
        OwnDepreciationCalculation,
        DoNotDepreciate,
        RepeatTheTaxBookMethod,
        CustomMethod,
        MACRSIndianReservation,
        MacrsFormula30,
        AdsSlMacrs30,
        MACRSIndianReservation30,
        StraightLineFullMonth30,

        // Canadian BEGIN !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        CdnDeclBal,
        CdnDeclBalFullMonth,
        CdnDeclBalHalfYear,
        UnknownDeprMethod = 33  // internal use only.
        //        UnknownDeprMethod = 30  // internal use only.
        // Canadian END ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

    } ;

    public class bpDeprMethod
    {
        public static int countOfMethods()
        {
            return (((int)bpDeprMethodTypeEnum.UnknownDeprMethod) - 1);
        }

        public static bool getNextMethod(out bpDeprMethodTypeEnum type_, ref int key_)
        {
            if (key_ < 0)
            {
                // a new enumeration
                key_ = 0;
            }

            // make sure there are still books left to enumerate
            if (key_ >= countOfMethods())
            {
                type_ = bpDeprMethodTypeEnum.UnknownDeprMethod;
                return false;
            }


            type_ = (bpDeprMethodTypeEnum)Enum.GetValues(typeof(bpDeprMethodTypeEnum)).GetValue(key_);

            key_++;

            return true;
        }


        private bpDeprMethodTypeEnum _type;
        private int _pct;
        private bpCustomMethod _custom;

        public bpDeprMethod()
        {
            _custom = null;
            _pct = 0;
            Type = bpDeprMethodTypeEnum.StraightLine;
        }

        public bpDeprMethod(bpDeprMethodTypeEnum newType)
        {
            _custom = null;
            _pct = 0;
            Type = newType;
        }

        public bpDeprMethod(bpDeprMethod obj)
        {
            _custom = null;
            _pct = 0;
            copyFrom(obj);
        }

        public bpDeprMethodTypeEnum Type
        {
            get { return _type; }
            set
            {
                _type = value;
                if (_type == bpDeprMethodTypeEnum.CustomMethod)
                    _initCustomMethod();
                else
                    _termCustomMethod();

                if (_type != bpDeprMethodTypeEnum.DeclBal &&
                     _type != bpDeprMethodTypeEnum.DeclBalHalfYear &&
                     _type != bpDeprMethodTypeEnum.DeclBalModHalfYear &&
                     _type != bpDeprMethodTypeEnum.DeclBalSwitch &&
                     _type != bpDeprMethodTypeEnum.DeclBalHalfYearSwitch &&
                     _type != bpDeprMethodTypeEnum.DeclBalModHalfYearSwitch &&
                     _type != bpDeprMethodTypeEnum.MacrsFormula &&
                     _type != bpDeprMethodTypeEnum.MacrsFormula30 &&
                     _type != bpDeprMethodTypeEnum.MacrsTable &&
                     _type != bpDeprMethodTypeEnum.MACRSIndianReservation &&
                     _type != bpDeprMethodTypeEnum.MACRSIndianReservation30)
                    _pct = 0;

            }
        }

        public int Percentage
        {
            get { return _pct; }
            set
            {
                if (_type == bpDeprMethodTypeEnum.DeclBal ||
                     _type == bpDeprMethodTypeEnum.DeclBalHalfYear ||
                     _type == bpDeprMethodTypeEnum.DeclBalModHalfYear ||
                     _type == bpDeprMethodTypeEnum.DeclBalSwitch ||
                     _type == bpDeprMethodTypeEnum.DeclBalHalfYearSwitch ||
                     _type == bpDeprMethodTypeEnum.DeclBalModHalfYearSwitch ||
                     _type == bpDeprMethodTypeEnum.MacrsFormula ||
                     _type == bpDeprMethodTypeEnum.MacrsFormula30 ||
                     _type == bpDeprMethodTypeEnum.MacrsTable ||
                     _type == bpDeprMethodTypeEnum.MACRSIndianReservation ||
                     _type == bpDeprMethodTypeEnum.MACRSIndianReservation30)
                    _pct = value;

            }
        }

        public bpCustomMethod CustomInfo
        {
            get
            {
                if (_type == bpDeprMethodTypeEnum.CustomMethod)
                {
                    return _custom;
                }
                return null;
            }

            set
            {
                if (_type == bpDeprMethodTypeEnum.CustomMethod)
                {
                    if ( _custom != null )
                        _custom.copyFrom(value);
                }
            }
        }

        /// <summary>
        /// Creates a bpDeprMethodTypeEnum value from bpDeprMethod object,
        /// </summary>
        /// <param name="bpDeprMethod">The bpDeprMethod object.</param>
        /// <returns>Returns bpDeprMethodTypeEnum</returns>
        public static implicit operator bpDeprMethodTypeEnum(bpDeprMethod bpDeprMethod)
        {
            return bpDeprMethod.Type;
        }

        /// <summary>
        /// Creates bpDeprMethod object from bpDeprMethodTypeEnum.
        /// </summary>
        /// <param name="deprMethType"> bpDeprMethodTypeEnum </param>
        /// <returns>Returns new bpDeprMethod object.</returns>
        public static implicit operator bpDeprMethod(bpDeprMethodTypeEnum deprMethType)
        {
            bpDeprMethod deprMethod = new bpDeprMethod(deprMethType);
            return deprMethod;
        }

        public override bool Equals(object obj)
        {
            bpDeprMethod that = obj as bpDeprMethod;
            return this == that;
        }

        public override int GetHashCode()
        {
            return (int)0;
        }

        public static bool operator ==(bpDeprMethod left, bpDeprMethod right)
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

            if (left.Type != right.Type)
                return false;

            if (left.Type == bpDeprMethodTypeEnum.CustomMethod && left.CustomInfo.code() != right.CustomInfo.code())
                return false;

            if (left.Percentage != right.Percentage)
                return false;

            return true;

        }

        public static bool operator !=(bpDeprMethod left, bpDeprMethod right)
        {
            return !(left == right);
        }


        public void copyFrom(bpDeprMethod  obj)
        {
            Type = obj.Type;
            Percentage = obj.Percentage;

            if (Type == bpDeprMethodTypeEnum.CustomMethod)
            {
                _custom.copyFrom( obj._custom );                
            }
        }

        public virtual void defaults()
        {
            _type = bpDeprMethodTypeEnum.StraightLine;
        }

        public virtual bool isObjectOk()
        {
            return true;
        }

        private void _initCustomMethod()
        {
            if (_custom == null)
                _custom = new bpCustomMethod();
        }

        private void _termCustomMethod()
        {
            if (_custom != null)
            {
                _custom = null;
            }
        }

    }
}
