using System;
using System.Collections.Generic;
using System.Text;

namespace SFABusinessTypes
{
    public class bpPropertyTypeList
    {
        protected List<bpPropertyTypeCode> _list;

        public bpPropertyTypeList()
        {
            _list = new List<bpPropertyTypeCode>();

            buildFullList();
        }

        ~bpPropertyTypeList()
        {
            _list.Clear();
        }

        public List<bpPropertyTypeCode> PropertyTypeCodeList
        {
            get
            {
                return _list;
            }
        }

        public void buildFullList()
        {
            bpPropertyTypeEnum[] types = {
                bpPropertyTypeEnum.PersonalGeneral,
                bpPropertyTypeEnum.Automobile,
                bpPropertyTypeEnum.PersonalListed,
                bpPropertyTypeEnum.RealGeneral,
                bpPropertyTypeEnum.RealListed,
                bpPropertyTypeEnum.RealConservation,
                bpPropertyTypeEnum.RealEnergy,
                bpPropertyTypeEnum.RealFarms,
                bpPropertyTypeEnum.RealLowIncomeHousing,
                bpPropertyTypeEnum.Amortizable,
                bpPropertyTypeEnum.VintageAccount,
                bpPropertyTypeEnum.Depreciable,
                bpPropertyTypeEnum.NonDepreciable,
                bpPropertyTypeEnum.LtTrucksAndVans
                                               };

            // Destroy old contents.
            _list.Clear();

            // Build new list.
            int i = -1;
            bpPropertyTypeCode aCode = new bpPropertyTypeCode();
            do
            {
                ++i;
                aCode.Type = (types[i]);
                _list.Add(new bpPropertyTypeCode(aCode));
            }
            while (types[i] != bpPropertyTypeEnum.LtTrucksAndVans);
        }

        public virtual bool isObjectOk()
        { 
            return true; 
        }

    }
}
