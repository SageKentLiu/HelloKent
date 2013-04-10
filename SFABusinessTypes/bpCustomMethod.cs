using System;
using System.Collections.Generic;
using System.Text;

namespace SFABusinessTypes
{
    public class bpCustomMethod
    {
        protected const int DEFCUSTMETHPCTS = 60;

        private int _numYears;
        private string _code;
        private string _desc;
        private bpDisposalConvention _convention;
        private double[] _pcts;
        private long _updateFlag;
        private long[] _pctUpdateFlag;

        public bpCustomMethod()
        {
            _pcts = null;
            _pctUpdateFlag = null;
            createPcts(DEFCUSTMETHPCTS);
            defaults();
            code(null);
        }

        public bpCustomMethod(bpCustomMethod obj)
        {
            _pcts = null;
            _pctUpdateFlag = null;
            createPcts(obj.countOfYears());
            defaults();
            copyFrom(obj);
        }


        public bpCustomMethod(string pCode, int numYears)
        {
            _pcts = null;
            _pctUpdateFlag = null;
            createPcts(numYears);
            defaults();
            if (pCode != null)
                code(pCode);
        }

        ~bpCustomMethod()
        {
            //if ( _pcts )
            //     delete []_pcts;
            //if ( _pctUpdateFlag )
            //     delete []_pctUpdateFlag;
        }

        //public LRbpCustomMethod    operator=(LRCbpCustomMethod obj);


        /// <summary>
        /// OverLoading == operator
        /// </summary>
        /// <param name="left">Left value</param>
        /// <param name="right">Right value</param>
        /// <returns>return true if values are the same</returns>
        public static bool operator ==(bpCustomMethod left, bpCustomMethod right)
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

            // Test codes.
            if (left.code() != right.code())
                return false;

            // Test descriptions.
            if (left.description() != right.description())
                return false;

            // Test the disposal conventions.
            if (left.convention() != right.convention())
                return false;

            // Test number of years in each.
            if (left.countOfYears() != right.countOfYears())
                return false;

            // Test each percentage.
            for (int i = 1; i <= left.countOfYears(); i++)
            {
                if (left.percentage(i) != right.percentage(i))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// OverLoading != operator
        /// </summary>
        /// <param name="left">Left value</param>
        /// <param name="right">Right value</param>
        /// <returns>return true if values are not the same</returns>
        public static bool operator !=(bpCustomMethod left, bpCustomMethod right)
        {
            return !(left == right);
        }

        public void copyFrom(bpCustomMethod obj)
        {
            code(obj.code());
            description(obj.description());
            convention(obj.convention());
            createPcts(obj.countOfYears());

            //assert( _pcts != null );
            //memcpy( _pcts,obj._pcts,sizeof(double)*countOfYears() );
            //assert( _pctUpdateFlag != null );
            //memcpy( _pctUpdateFlag,obj._pctUpdateFlag,sizeof(LONG)*countOfYears() );

            //blame Zhiyang if problem....
            for(int i=1;i<=countOfYears(); i++)
            {
                percentage(i, obj.percentage(i));
                pctUpdateFlag(i, obj.pctUpdateFlag(i));
            }

            updateFlag(obj.updateFlag());
        }


        public string code()
        { 
            return _code; 
        }

        public void code(string newCode)
        {
            _code = newCode;

            if ( _code != null && _code.Length > 2 )
                _code = _code.Substring(0, 2);
        }

        public string description()
        { 
            return _desc; 
        }

        public void description(string newDesc)
        {
            _desc = newDesc;
            _desc.TrimEnd(' ');
        }

        public bpDisposalConvention convention()
        { 
            return _convention; 
        }

        public void convention(bpDisposalConvention obj)
        { 
            _convention = obj; 
        }

        public double percentage(int year)
        {
            //assert( _pcts != null );

            if (year > countOfYears())
                return 0.0;
            return (_pcts[year - 1]);
        }

        public bool percentage(int year, double value)
        {
            //assert( _pcts != null );

            if (year <= 0 || year > countOfYears())
                return false;
            _pcts[year - 1] = value;
            return true;
        }

        public int countOfYears()
        { 
            return _numYears; 
        }

        public int lifeInYears()
        {
            int years = 0;
            for (int i = 1; i <= countOfYears(); ++i)
                if (percentage(i) != 0.0)
                    years = i;
            return years;
        }

        public bool isValidFormat()
        {
            string theCode = code();

            //// If the code does not consist of ONLY letters and/or numbers, return
            //// false.
            //if ( !isalnum(*theCode) || !isalnum(*(theCode+1)) )
            //     return false;

            //// If only lowercase letters, return a true.
            //if ( islower(*theCode) && islower(*(theCode+1)) )
            //     return true;

            //// If only digits, return a true.
            //if ( isdigit(*theCode) && isdigit(*(theCode+1)) )
            //     return true;

            //// If a digit and a letter or a letter and a digit, return a true.
            //if ( isdigit(*theCode) && islower(*(theCode+1)) )
            //     return true;

            //if ( islower(*theCode) && isdigit(*(theCode+1)) )
            //     return true;

            return false;
        }

        public void zeroPercentages()
        {
            _pcts = null;
            _pctUpdateFlag = null;

            _pcts = new double[countOfYears()];
            _pctUpdateFlag = new long[countOfYears()];

            //assert( _pcts != null );
            //memset( _pcts,0,sizeof(double)*countOfYears() );
            //assert( _pctUpdateFlag != null );
            //memset( _pctUpdateFlag,0,sizeof(LONG)*countOfYears() );
        }

        public long updateFlag()
        {
            return _updateFlag;
        }

        public void updateFlag(long setTo)
        {
            _updateFlag = setTo;
        }


        public long pctUpdateFlag(int year)
        {
            //assert( _pctUpdateFlag != null );

            if (year > countOfYears())
                return 0;
            return _pctUpdateFlag[year - 1];
        }

        public bool pctUpdateFlag(int year, long value)
        {
            //assert( _pctUpdateFlag != null );

            if (year <= 0 || year > countOfYears())
                return false;
            _pctUpdateFlag[year - 1] = value;
            return true;
        }


        public virtual void defaults()
        {
            code("aa");

            description("");

            bpDisposalConvention aConvention = new bpDisposalConvention(bpDisposalConvention.DispConvType.FullMonth);
            convention(aConvention);

            zeroPercentages();
            updateFlag((long)0);
        }

        public virtual bool isObjectOk()
        {
            if (!isValidFormat())
                return false;
            if (convention().isObjectOk() == false)
                return false;

            return true;
        }

        protected void createPcts(int numYears)
        {
            if (_pcts != null)
            {
                //delete []_pcts;
                _pcts = null;
            }

            if (_pctUpdateFlag != null)
            {
                //delete []_pctUpdateFlag;
                _pctUpdateFlag = null;
            }

            int years = (numYears == 0) ? DEFCUSTMETHPCTS : numYears;

            _pcts = new double[years];
            _pctUpdateFlag = new long[years];
            //assert( _pcts != null );
            //assert( _pctUpdateFlag != null );
            countOfYears(years);
        }

        protected double[] getPctList()
        { 
            return _pcts; 
        }
        
        protected long[] getPctUpdateFlagList()
        { 
            return _pctUpdateFlag; 
        }

        private void countOfYears(int numYears)
        { 
            _numYears = numYears; 
        }

    }
}
