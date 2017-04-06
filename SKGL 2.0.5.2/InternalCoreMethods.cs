using System;
using System.Numerics;

namespace SerialKeyGenerator
{
    internal class InternalCoreMethods : SerialKeyConfiguration
    {

        //The construction of the key
        protected internal string encrypt(int days, bool[] features, string secretPhase, int randomId, DateTime creationDate)
        {
            // This function will store information in Artem's ISF-2
            var todayAsInteger = Convert.ToInt32(creationDate.ToString("yyyyMMdd"));
            decimal result = 0;
            result += todayAsInteger;
            result *= 1000;
            result += days;
            result *= 1000;
            result += booleanToInt(features);
            result *= 100000;
            result += randomId;


            // This part of the function uses Artem's SKA-2
            if (string.IsNullOrEmpty(secretPhase))
            {
                // if not password is set, return an unencrypted key
                return base10ToBase26((getEightByteHash(result.ToString()) + result.ToString()));
            }
            else
            {
                // if password is set, return an encrypted 
                return base10ToBase26((getEightByteHash(result.ToString()) + encryptText(result.ToString(), secretPhase)));
            }


        }

        protected internal string decrypt(string key, string secretPhase)
        {
            if (string.IsNullOrEmpty(secretPhase))
            {
                // if not password is set, return an unencrypted key
                return base26ToBase10(key);
            }
            else
            {
                // if password is set, return an encrypted 
                var usefulInformation = base26ToBase10(key);
                return usefulInformation.Substring(0, 9) + decryptText(usefulInformation.Substring(9), secretPhase);
            }

        }
        
        protected internal int booleanToInt(bool[] booleanArray)
        {
            var _aVector = 0;
            //
            //In this function we are converting a binary value array to a int
            //A binary array can max contain 4 values.
            //Ex: new boolean(){1,1,1,1}

            for (var _i = 0; _i < booleanArray.Length; _i++)
            {
                switch (booleanArray[_i])
                {
                    case true:
                        _aVector += Convert.ToInt32((Math.Pow(2, (booleanArray.Length - _i - 1))));
                        // times 1 has been removed
                        break;
                }
            }
            return _aVector;
        }

        protected internal bool[] intToBoolean(int num)
        {
            //Converting an integer to a binary array

            var b = Convert.ToInt32(Convert.ToString(num, 2));
            var a = padNumberIfNecessary(b.ToString(), 8);
            var result = new bool[8];


            for (var i = 0; i <= 7; i++)
            {
                result[i] = a.Substring(i, 1) == "1" ? true : false;
            }
            return result;
        }
        

        protected internal string twentyfiveByteHash(string stringValue)
        {
            var amountOfBlocks = stringValue.Length / 5;
            var preHash = new string[amountOfBlocks + 1];

            if (stringValue.Length <= 5)
            {
                //if the input string is shorter than 5, no need of blocks! 
                preHash[0] = getEightByteHash(stringValue).ToString();
            }
            else if (stringValue.Length > 5)
            {
                //if the input is more than 5, there is a need of dividing it into blocks.
                for (var i = 0; i <= amountOfBlocks - 2; i++)
                {
                    preHash[i] = getEightByteHash(stringValue.Substring(i * 5, 5)).ToString();
                }

                preHash[preHash.Length - 2] = getEightByteHash(stringValue.Substring((preHash.Length - 2) * 5, stringValue.Length - (preHash.Length - 2) * 5)).ToString();
            }
            return string.Join("", preHash);
        }

        protected internal int getEightByteHash(string stringValue, int mustBeLessThan = 1000000000)
        {
            //This function generates a eight byte hash

            //The length of the result might be changed to any length
            //just set the amount of zeroes in MUST_BE_LESS_THAN
            //to any length you want
            uint hash = 0;

            foreach (var b in System.Text.Encoding.Unicode.GetBytes(stringValue))
            {
                hash += b;
                hash += (hash << 10);
                hash ^= (hash >> 6);
            }

            hash += (hash << 3);
            hash ^= (hash >> 11);
            hash += (hash << 15);

            var result = (int)(hash % mustBeLessThan);
            var check = mustBeLessThan / result;

            if (check > 1)
            {
                result *= check;
            }

            return result;
        }





        private int modulo(int num, int baseNumber)
        {
            //this function simply calculates the "right modulo".
            //by using this function, there won't, hopefully be a negative
            //number in the result!
            return num - baseNumber * Convert.ToInt32(Math.Floor((decimal)num / (decimal)baseNumber));
        }

        private string base10ToBase26(string stringValue)
        {
            // This method is converting a base 10 number to base 26 number.
            // Remember that s is a decimal, and the size is limited. 
            // In order to get size, type Decimal.MaxValue.
            //
            // Note that this method will still work, even though you only 
            // can add, subtract numbers in range of 15 digits.
            var allowedLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

            var num = Convert.ToDecimal(stringValue);
            var reminder = 0;

            var result = new char[stringValue.Length + 1];
            var j = 0;


            while ((num >= 26))
            {
                reminder = Convert.ToInt32(num % 26);
                result[j] = allowedLetters[reminder];
                num = (num - reminder) / 26;
                j += 1;
            }

            result[j] = allowedLetters[Convert.ToInt32(num)];
            // final calculation

            var returnNum = "";

            for (var k = j; k >= 0; k -= 1)  // not sure
            {
                returnNum += result[k];
            }
            return returnNum;

        }

        private string base26ToBase10(string stringValue)
        {
            // This function will convert a number that has been generated
            // with functin above, and get the actual number in decimal
            //
            // This function requieres Mega Math to work correctly.

            var allowedLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var result = new System.Numerics.BigInteger();


            for (var i = 0; i <= stringValue.Length - 1; i += 1)
            {
                var pow = powof(26, (stringValue.Length - i - 1));

                result = result + allowedLetters.IndexOf(stringValue.Substring(i, 1)) * pow;

            }

            return result.ToString();
        }

        private BigInteger powof(int x, int y)
        {
            // Because of the uncertain answer using Math.Pow and ^, 
            // this function is here to solve that issue.
            // It is currently using the MegaMath library to calculate.
            BigInteger newNum = 1;

            if (y == 0)
            {
                return 1;
                // if 0, return 1, e.g. x^0 = 1 (mathematicaly proven!) 
            }
            else if (y == 1)
            {
                return x;
                // if 1, return x, which is the base, e.g. x^1 = x
            }
            else
            {
                for (var i = 0; i <= y - 1; i++)
                {
                    newNum = newNum * x;
                }
                return newNum;
                // if both conditions are not satisfied, this loop
                // will continue to y, which is the exponent.
            }
        }

        private string encryptText(string inputPhase, string secretPhase)
        {
            //in this class we are encrypting the integer array.
            var result = string.Empty;

            for (var i = 0; i <= inputPhase.Length - 1; i++)
            {
                result += modulo(Convert.ToInt32(inputPhase.Substring(i, 1)) + Convert.ToInt32(secretPhase.Substring(modulo(i, secretPhase.Length), 1)), 10);
            }

            return result;
        }

        private string decryptText(string encryptedPhase, string secretPhase)
        {
            //in this class we are decrypting the text encrypted with the function above.
            var result = string.Empty;

            for (var i = 0; i <= encryptedPhase.Length - 1; i++)
            {
                result += modulo(Convert.ToInt32(encryptedPhase.Substring(i, 1)) - Convert.ToInt32(secretPhase.Substring(modulo(i, secretPhase.Length), 1)), 10);
            }

            return result;
        }

        private string padNumberIfNecessary(string number, int length)
        {
            // This function create 3 length char ex: 39 to 039
            if ((number.Length == length)) return number;

            while (number.Length != length)
            {
                number = "0" + number;
            }
            return number;

        }
    }
}