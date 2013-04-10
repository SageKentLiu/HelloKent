using System;
using System.Collections.Generic;
using System.Text;

namespace SFABusinessTypes
{
    public enum bpPropertyTypeEnum
    {
        PropMin = 0,          //Internal Use Only
        PersonalGeneral = 1,
        Automobile,
        PersonalListed,
        RealGeneral,
        RealListed,
        RealConservation,
        RealEnergy,
        RealFarms,
        RealLowIncomeHousing,
        Amortizable,
        VintageAccount,
        Depreciable,
        NonDepreciable,
        LtTrucksAndVans,
        PropMax,
        UnknownPropertyType     //Internal Use Only
    } ;

    public class bpPropertyType
    {
        #region Private Variables

        private bpPropertyTypeEnum _type;

        #endregion


        #region Constructors

        public bpPropertyType()
        {
            Type = bpPropertyTypeEnum.PersonalGeneral;
        }

        public bpPropertyType(bpPropertyTypeEnum newType)
        {
            Type = newType;
        }

        public bpPropertyType(bpPropertyType obj)
        {
            copyFrom(obj);
        }

        #endregion


        #region Operator Overload

        public override bool Equals(object o)
        {
            return this == (bpPropertyType)o;
        }

        public override int GetHashCode()
        {
            return (int)Type;
        }

        public static bool operator ==(bpPropertyType left, bpPropertyType right)
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

        public static bool operator !=(bpPropertyType left, bpPropertyType right)
        {
            return !(left == right);
        }

        public static implicit operator bpPropertyTypeEnum(bpPropertyType bpPropType)
        {
            return bpPropType.Type;
        }

        public static implicit operator bpPropertyType(bpPropertyTypeEnum propType)
        {
            bpPropertyType bpPropType = new bpPropertyType(propType);
            return bpPropType;
        }

        #endregion

        #region Public Methods
        
        public void copyFrom(bpPropertyType pInfo)
        { 
            Type = (pInfo.Type); 
        }
                                  
        public virtual void defaults()
		{
            Type = bpPropertyTypeEnum.PersonalGeneral;
        }
				                    
        public virtual bool isObjectOk()
        {
            if (_type <= bpPropertyTypeEnum.PropMin || _type >= bpPropertyTypeEnum.PropMax)
                return false;
            else
                return true;
        }

        #endregion


        #region Public Properties

        public bpPropertyTypeEnum Type
        {
            get { return _type; }
            set { _type = value; }
        }

        #endregion
    }
}
