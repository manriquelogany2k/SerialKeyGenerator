﻿using System;
using System.Security;
//Copyright (C) 2011-2012 Artem Los, www.clizware.net.
//The author of this code shall get the credits

// This project uses two general algorithms:
//  - Artem's Information Storage Format (Artem's ISF-2)
//  - Artem's Serial Key Algorithm (Artem's SKA-2)

// A great thank to Iberna (https://www.codeplex.com/site/users/view/lberna)
// for getHardDiskSerial algorithm.


[assembly: AllowPartiallyTrustedCallers]
namespace SerialKeyGenerator
{

    public class Generate : BaseConfiguration
    {
        //this class have to be inherited because of the key which is shared with both encryption/decryption classes.

        private readonly SerialKeyConfiguration _configuration = new SerialKeyConfiguration();
        private readonly InternalCoreMethods _internalCoreMethods = new InternalCoreMethods();
        private readonly Random _random = new Random();
        private string _secretPhase;

        public Generate()
        {
        }

        public Generate(SerialKeyConfiguration configuration)
        {
            _configuration = configuration;
        }


        /// <summary>
        /// If the key is to be encrypted, enter a password here.
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
        /// This function will generate a key.
        /// </summary>
        /// <param name="timeLeft">For instance, 30 days.</param>
        public string DoKey(int timeLeft)
        {
            return DoKey(timeLeft, DateTime.Today); // removed extra argument false
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeLeft">For instance, 30 days</param>
        /// <param name="useMachineCode">Lock a serial key to a specific machine, given its "machine code". Should be 5 digits long.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public object DoKey(int timeLeft, int useMachineCode)
        {
            return DoKey(timeLeft, DateTime.Today, useMachineCode);
        }


        /// <summary>
        /// This function will generate a key. You may also change the creation date.
        /// </summary>
        /// <param name="timeLeft">For instance, 30 days.</param>
        /// <param name="creationDate">Change the creation date of a key.</param>
        /// <param name="useMachineCode">Lock a serial key to a specific machine, given its "machine code". Should be 5 digits long.</param>
        public string DoKey(int timeLeft, System.DateTime creationDate, int useMachineCode = 0)
        {
            if (timeLeft > 999)
            {
                //Checking if the timeleft is NOT larger than 999. It cannot be larger to match the key-length 20.
                throw new ArgumentException("The timeLeft is larger than 999. It can only consist of three digits.");
            }

            if (!string.IsNullOrEmpty(SecretPhase) | SecretPhase != null)
            {
                //if some kind of value is assigned to the variable "secretPhase", the code will execute it FIRST.
                //the secretPhase shall only consist of digits!
                System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("^\\d$");
                //cheking the string
                if (reg.IsMatch(SecretPhase))
                {
                    //throwing new exception if the string contains non-numrical letters.
                    throw new ArgumentException("The secretPhase consist of non-numerical letters.");
                }
            }

            //if no exception is thown, do following
            string _stageThree = null;
            if (useMachineCode > 0 & useMachineCode <= 99999)
            {
                _stageThree = _internalCoreMethods.encrypt(timeLeft, _configuration.Features, SecretPhase, useMachineCode, creationDate);
                // stage one
            }
            else
            {
                _stageThree = _internalCoreMethods.encrypt(timeLeft, _configuration.Features, SecretPhase, _random.Next(0, 99999), creationDate);
                // stage one
            }

            //if it is the same value as default, we do not need to mix chars. This step saves generation time.

            if (_configuration.AddSplitChar == true)
            {
                // by default, a split character will be addedr
                Key = _stageThree.Substring(0, 5) + "-" + _stageThree.Substring(5, 5) + "-" + _stageThree.Substring(10, 5) + "-" + _stageThree.Substring(15, 5);
            }
            else
            {
                Key = _stageThree;
            }

            //we also include the key in the Key variable to make it possible for user to get his key without generating a new one.
            return Key;

        }


    }


}