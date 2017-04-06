using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SerialKeyGenerator;

namespace SKGLTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void MachineCodeTest()
        {
            Generate gen = new Generate();
            string a= gen.MachineCode.ToString();
        }

        [TestMethod]
        public void CreateAndValidateSimple()
        {
            Generate gen = new Generate();
            string a  = gen.DoKey(30);

            Validate val = new Validate();

            val.Key = a;
            
            Assert.IsTrue(val.IsValid == true);
            Assert.IsTrue(val.IsExpired ==false);
            Assert.IsTrue(val.SetTime == 30);

        }
        [TestMethod]
        public void CreateAndValidateA()
        {

            Validate val = new Validate();

            val.Key = "MXNBF-ITLDZ-WPOBY-UCHQW";
            val.SecretPhase = "567";

            Assert.IsTrue(val.IsValid == true);
            Assert.IsTrue(val.IsExpired == true);
            Assert.IsTrue(val.SetTime == 30);

        }
        [TestMethod]
        public void CreateAndValidateC()
        {
            SerialKeyConfiguration skm = new SerialKeyConfiguration();

            Generate gen = new Generate(skm);
            skm.Features[0] = true;
            gen.SecretPhase = "567";
            string a = gen.DoKey(37);


            Validate val = new Validate();

            val.Key = a;
            val.SecretPhase = "567";

            Assert.IsTrue(val.IsValid == true);
            Assert.IsTrue(val.IsExpired == false);
            Assert.IsTrue(val.SetTime == 37);
            Assert.IsTrue(val.Features[0] == true);
            Assert.IsTrue(val.Features[1] == false);

        }


        [TestMethod]
        public void CreateAndValidateCJ()
        {


            Validate val = new Validate();

            val.Key = "LZWXQ-SMBAS-JDVDL-XTEHB";
            val.SecretPhase = "567";

            int timeLeft = val.DaysLeft;

            Assert.IsTrue(val.IsValid == true);
            Assert.IsTrue(val.IsExpired == true);
            Assert.IsTrue(val.SetTime == 30);
            Assert.IsTrue(val.Features[0] == true);
            //Assert.IsTrue(val.Features[1] == false);

        }

        [TestMethod]
        public void CreateAndValidateAM()
        {
            Generate gen = new Generate();
            string a = gen.DoKey(30);

            Validate ValidateAKey = new Validate();

            ValidateAKey.Key = a;

            Assert.IsTrue(ValidateAKey.IsValid == true);
            Assert.IsTrue(ValidateAKey.IsExpired == false);
            Assert.IsTrue(ValidateAKey.SetTime == 30);

            if (ValidateAKey.IsValid)
            {
                // displaying date
                // remember to use .ToShortDateString after each date!
                Console.WriteLine("This key is created {0}", ValidateAKey.CreationDate.ToShortDateString());
                Console.WriteLine("This key will expire {0}", ValidateAKey.ExpireDate.ToShortDateString());

                Console.WriteLine("This key is set to be valid in {0} day(s)", ValidateAKey.SetTime);
                Console.WriteLine("This key has {0} day(s) left", ValidateAKey.DaysLeft);

            }
            else
            {
                // if invalid
                Console.WriteLine("Invalid!");
            }

        }
    }
}
