using System;
using System.Collections.Generic;
using System.Text;

namespace SFABusinessTypes
{
    public class bpPropertyTypeCode : bpPropertyType
    {
        #region Nested Struct
        
        struct PROPCODE
        {
            public bpPropertyTypeEnum type;
            public string code;
            public string name;
        }

        #endregion


        #region Static Variables
        
        static PROPCODE[] codes = new PROPCODE[] { 
							new PROPCODE(){ type = bpPropertyTypeEnum.PersonalGeneral, code = "P", name = "Personal, General"},
							new PROPCODE(){ type = bpPropertyTypeEnum.Automobile, code = "A", name = "Automobile"},
							new PROPCODE(){ type = bpPropertyTypeEnum.LtTrucksAndVans, code = "T", name = "Lt Trucks and Vans"},
							new PROPCODE(){ type = bpPropertyTypeEnum.PersonalListed, code = "Q", name = "Personal, listed"},
							new PROPCODE(){ type = bpPropertyTypeEnum.RealGeneral, code = "R", name = "Real, General"},
							new PROPCODE(){ type = bpPropertyTypeEnum.RealListed, code = "S", name = "Real, Listed"},
							new PROPCODE(){ type = bpPropertyTypeEnum.RealConservation, code = "C", name = "Real, Conservation"},
							new PROPCODE(){ type = bpPropertyTypeEnum.RealEnergy, code = "E", name = "Real, Energy"},
							new PROPCODE(){ type = bpPropertyTypeEnum.RealFarms, code = "F", name = "Real, Farms"},
							new PROPCODE(){ type = bpPropertyTypeEnum.RealLowIncomeHousing, code = "H", name = "Real, Low-income housing"},
							new PROPCODE(){ type = bpPropertyTypeEnum.Amortizable, code = "Z", name = "Amortizable"},
							new PROPCODE(){ type = bpPropertyTypeEnum.VintageAccount, code = "V", name = "Vintage"},
							new PROPCODE(){ type = bpPropertyTypeEnum.Depreciable, code = "D", name = "Depreciable"},
							new PROPCODE(){ type = bpPropertyTypeEnum.NonDepreciable, code = "N", name = "Non-Depreciable"},
							new PROPCODE(){ type = bpPropertyTypeEnum.UnknownPropertyType, code = "\0", name = null}
							};

        static PROPCODE[] gasbcodes = new PROPCODE[] { 
							new PROPCODE(){ type = bpPropertyTypeEnum.Depreciable, code = "D", name = "Depreciable"},
							new PROPCODE(){ type = bpPropertyTypeEnum.NonDepreciable, code = "N", name = "Non-Depreciable"},
							new PROPCODE(){ type = bpPropertyTypeEnum.UnknownPropertyType, code = "\0", name = null}
							};

        #endregion


        #region Public Static Methods

        public static bool isValidShortName(string name)
        {
            if (translateShortNameToType(name) == bpPropertyTypeEnum.UnknownPropertyType)
                return false;
            else
                return true;
        }

        public static bool isValidLongName(string name)
        {
            if (translateLongNameToType(name) == bpPropertyTypeEnum.UnknownPropertyType)
                return false;
            else
                return true;
        }

        public static bpPropertyTypeEnum translateShortNameToType(string shortName)
        {
            bool isGASB = false;

            if (isGASB)
            {
                foreach (PROPCODE code in gasbcodes)
                {
                    if (code.code == shortName)
                        return code.type;
                }
            }
            else
            {
                foreach (PROPCODE code in codes)
                {
                    if (code.code == shortName)
                        return code.type;
                }
            }

            return bpPropertyTypeEnum.UnknownPropertyType;
        }

        #endregion


        #region Protected Static Methods

        protected static bpPropertyTypeEnum translateLongNameToType(string longName)
        {
            foreach (PROPCODE code in codes)
            {
                if (code.name == longName)
                    return code.type;
            }

            return bpPropertyTypeEnum.UnknownPropertyType;
        }

        protected static string translateTypeToShortName(bpPropertyTypeEnum type)
        {
            foreach (PROPCODE code in codes)
            {
                if (code.type == type)
                    return code.code;
            }
            return "\0";
        }

        protected static string translateTypeToLongName(bpPropertyTypeEnum type)
        {
            foreach (PROPCODE code in codes)
            {
                if (code.type == type)
                    return code.name;
            }
            return null;
        }

        #endregion



        #region Private Variables

        private bool _stable;

        #endregion


        #region Constructors

        public bpPropertyTypeCode()
        {
            _stable = true;
            defaults();
        }

        public bpPropertyTypeCode(bpPropertyType obj)
            : base(obj)
        {
            _stable = true;
        }

        public bpPropertyTypeCode(bpPropertyTypeCode obj)
        {
            copyFrom(obj);
        }

        public bpPropertyTypeCode(string name)
        {
            _stable = true;
            if (isValidShortName(name))
                Type = (translateShortNameToType(name));
            else
                _stable = false;
        }

        //public bpPropertyTypeCode(string aLongName)
        //{
        //    _stable = true;
        //    if (isValidLongName(aLongName))
        //        Type = (translateLongNameToType(aLongName));
        //    else
        //        _stable = false;
        //}

        #endregion

        #region Operator Overload

        public override bool Equals(object o)
        {
            return this == (bpPropertyTypeCode)o;
        }

        public override int GetHashCode()
        {
            return (int)Type;
        }

        public static bool operator ==(bpPropertyTypeCode left, bpPropertyTypeCode right)
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

            return (bpPropertyType)left == (bpPropertyType)right;
        }

        public static bool operator !=(bpPropertyTypeCode left, bpPropertyTypeCode right)
        {
            return !((bpPropertyType)left == (bpPropertyType)right);
        }

        public static bool operator <(bpPropertyTypeCode left, bpPropertyTypeCode right)
        {
            return left.Type < right.Type;
        }

        public static bool operator >(bpPropertyTypeCode left, bpPropertyTypeCode right)
        {
            return !(left < right);
        }

        #endregion

        #region Public Methods

        public void copyFrom(bpPropertyTypeCode obj)
        {
            base.copyFrom(obj);
            _stable = obj._stable; ;
        }

        public string shortName()
        {
            return translateTypeToShortName(Type);
        }

        public string longName()
        {
            return translateTypeToLongName(Type);
        }

        public virtual bool isObjectOk()
        {
	        if( base.isObjectOk() == false)
                return false;

	        return _stable;
        }

        #endregion
    }
}
