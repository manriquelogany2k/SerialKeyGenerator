using System;

namespace SerialKeyGenerator
{
    public class Validate : BaseConfiguration
    {
        //this class have to be inherited because of the key which is shared with both encryption/decryption classes.

        private readonly SerialKeyConfiguration _configuration = new SerialKeyConfiguration();
        private readonly InternalCoreMethods _internalCoreMethods = new InternalCoreMethods();
        private string _res = "";
        private string _secretPhase = "";


        public Validate()
        {
        }

        public Validate(SerialKeyConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        
        /// <summary>
        /// Enter a key here before validating.
        /// </summary>
        public override string Key
        {
            //re-defining the Key
            get => _key;
            set
            {
                _res = "";
                base.Key = value;
            }
        }

        
        /// <summary>
        /// If the key has been encrypted, when it was generated, please set the same SecretPhase here.
        /// </summary>
        public string SecretPhase
        {
            get { return _secretPhase; }
            set
            {
                if (value != _secretPhase)
                {
                    _secretPhase = _internalCoreMethods.twentyfiveByteHash(value);
                }
            }
        }
        
        
        /// <summary>
        /// Checks whether the key has been modified or not. If the key has been modified - returns false; if the key has not been modified - returns true.
        /// </summary>
        public bool IsValid => isValid();

        
        /// <summary>
        /// If the key has expired - returns true; if the key has not expired - returns false.
        /// </summary>
        public bool IsExpired => isExpired();

        


        /// <summary>
        /// Returns the creation date of the key.
        /// </summary>
        public DateTime CreationDate => creationDate();

        private int _DaysLeft()
        {
            decodeKeyToString();
            var _setDays = SetTime;
            return Convert.ToInt32(((TimeSpan)(ExpireDate - DateTime.Today)).TotalDays); //or viseversa
        }
        /// <summary>
        /// Returns the amount of days the key will be valid.
        /// </summary>
        public int DaysLeft => _DaysLeft();

        
        /// <summary>
        /// Returns the actual amount of days that were set when the key was generated.
        /// </summary>
        public int SetTime => getAmountOfDayThatWereSetWhenKeyWasGenerated();


        /// <summary>
        /// Returns the date when the key is to be expired.
        /// </summary>
        public DateTime ExpireDate => getExpireDate();
        
        
        /// <summary>
        /// Returns all 8 features in a boolean array
        /// </summary>
        public bool[] Features => getFeatures();


        /// <summary>
        /// If the current machine's machine code is equal to the one that this key is designed for, return true.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsOnRightMachine
        {
            get
            {
                var decodedMachineCode = Convert.ToInt32(_res.Substring(23, 5));
                return decodedMachineCode == MachineCode;
            }
        }






        private void decodeKeyToString()
        {
            // checking if the key already have been decoded.
            if (string.IsNullOrEmpty(_res) | _res == null)
            {

                var _stageOne = "";

                Key = Key.Replace("-", "");

                //if the admBlock has been changed, the getMixChars will be executed.


                _stageOne = Key;

                // _stageTwo = _a._decode(_stageOne)

                if (!string.IsNullOrEmpty(SecretPhase) | SecretPhase != null)
                {
                    //if no value "secretPhase" given, the code will directly decrypt without using somekind of encryption
                    //if some kind of value is assigned to the variable "secretPhase", the code will execute it FIRST.
                    //the secretPhase shall only consist of digits!
                    var reg = new System.Text.RegularExpressions.Regex("^\\d$");
                    //cheking the string
                    if (reg.IsMatch(SecretPhase))
                    {
                        //throwing new exception if the string contains non-numrical letters.
                        throw new ArgumentException("The secretPhase consist of non-numerical letters.");
                    }
                }
                _res = _internalCoreMethods.decrypt(_stageOne, SecretPhase);


            }
        }

        private bool isValid()
        {
            //Dim _a As New methods ' is only here to provide the geteighthashcode method
            try
            {
                if (Key.Contains("-"))
                {
                    if (Key.Length != 23)
                    {
                        return false;
                    }
                }
                else
                {
                    if (Key.Length != 20)
                    {
                        return false;
                    }
                }
                decodeKeyToString();

                var _decodedHash = _res.Substring(0, 9);
                var _calculatedHash = _internalCoreMethods.getEightByteHash(_res.Substring(9, 19)).ToString().Substring(0, 9);
                // changed Math.Abs(_res.Substring(0, 17).GetHashCode).ToString.Substring(0, 8)

                //When the hashcode is calculated, it cannot be taken for sure, 
                //that the same hash value will be generated.
                //learn more about this issue: http://msdn.microsoft.com/en-us/library/system.object.gethashcode.aspx
                if (_decodedHash == _calculatedHash)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                //if something goes wrong, for example, when decrypting, 
                //this function will return false, so that user knows that it is unvalid.
                //if the key is valid, there won't be any errors.
                return false;
            }
        }

        private bool isExpired()
        {
            if (DaysLeft > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private DateTime creationDate()
        {
            decodeKeyToString();
            var _date = new DateTime();
            _date = new DateTime(Convert.ToInt32(_res.Substring(9, 4)), Convert.ToInt32(_res.Substring(13, 2)), Convert.ToInt32(_res.Substring(15, 2)));

            return _date;
        }

        private int getAmountOfDayThatWereSetWhenKeyWasGenerated()
        {
            decodeKeyToString();
            return Convert.ToInt32(_res.Substring(17, 3));
        }

        private bool[] getFeatures()
        {
            decodeKeyToString();
            return _internalCoreMethods.intToBoolean(Convert.ToInt32(_res.Substring(20, 3)));
        }

        private DateTime getExpireDate()
        {
            decodeKeyToString();
            var _date = new DateTime();
            _date = CreationDate;
            return _date.AddDays(SetTime);
        }
    }
}