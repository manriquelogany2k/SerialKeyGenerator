using System;
using System.Management;
using System.Security;

namespace SerialKeyGenerator
{
    public abstract class BaseConfiguration
    {


        protected internal string _key = "";


        /// <summary>
        /// The key will be stored here
        /// </summary>
        public virtual string Key
        {
            //will be changed in both generating and validating classes.
            get { return _key; }
            set { _key = value; }
        }


        public virtual int MachineCode => getMachineCode();


        [SecuritySafeCritical]
        private static int getMachineCode()
        {
            //      * Copyright (C) 2012 Artem Los, All rights reserved.
            //      * 
            //      * This code will generate a 5 digits long key, finger print, of the system
            //      * where this method is being executed. However, that might be changed in the
            //      * hash function "GetStableHash", by changing the amount of zeroes in
            //      * MUST_BE_LESS_OR_EQUAL_TO to the one you want to have. Ex 1000 will return 
            //      * 3 digits long hash.
            //      * 
            //      * Please note, that you might also adjust the order of these, but remember to
            //      * keep them there because as it is stated at 
            //      * (http://www.codeproject.com/Articles/17973/How-To-Get-Hardware-Information-CPU-ID-MainBoard-I)
            //      * the processorID might be the same at some machines, which will generate same
            //      * hashes for several machines.
            //      * 
            //      * The function will probably be implemented into SKGL Project at http://skgl.codeplex.com/
            //      * and Software Protector at http://softwareprotector.codeplex.com/, so I 
            //      * release this code under the same terms and conditions as stated here:
            //      * http://skgl.codeplex.com/license
            //      * 
            //      * Any questions, please contact me at
            //      *  * artem@artemlos.net
            //      
            var internalCoreMethods = new InternalCoreMethods();

            var searcher = new ManagementObjectSearcher("select * from Win32_Processor");
            var collectedInfo = "";
            // here we will put the informa
            foreach (ManagementObject share in searcher.Get())
            {
                // first of all, the processorid
                collectedInfo += share.GetPropertyValue("ProcessorId");
            }

            searcher.Query = new ObjectQuery("select * from Win32_BIOS");
            foreach (ManagementObject share in searcher.Get())
            {
                //then, the serial number of BIOS
                collectedInfo += share.GetPropertyValue("SerialNumber");
            }

            searcher.Query = new ObjectQuery("select * from Win32_BaseBoard");
            foreach (ManagementObject share in searcher.Get())
            {
                //finally, the serial number of motherboard
                collectedInfo += share.GetPropertyValue("SerialNumber");
            }

            // patch luca bernardini
            if (string.IsNullOrEmpty(collectedInfo) | collectedInfo == "00" | collectedInfo.Length <= 3)
            {
                collectedInfo += getHddSerialNumber();
            }

            return internalCoreMethods.getEightByteHash(collectedInfo, 100000);
        }


        // <summary>
        // Read the serial number from the hard disk that keep the bootable partition (boot disk)
        // </summary>
        // <returns>
        // If succeeds, returns the string representing the Serial Number.
        // String.Empty if it fails.
        // </returns>
        [SecuritySafeCritical]
        private static string getHddSerialNumber()
        {


            // --- Win32 Disk 
            var searcher = new ManagementObjectSearcher("\\root\\cimv2", "select * from Win32_DiskPartition WHERE BootPartition=True");

            uint diskIndex = 999;
            foreach (ManagementObject partition in searcher.Get())
            {
                diskIndex = Convert.ToUInt32(partition.GetPropertyValue("Index"));
                break; // TODO: might not be correct. Was : Exit For
            }

            // I haven't found the bootable partition. Fail.
            if (diskIndex == 999)
                return string.Empty;



            // --- Win32 Disk Drive
            searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive where Index = " + diskIndex.ToString());

            var deviceName = "";
            foreach (ManagementObject wmi_HD in searcher.Get())
            {
                deviceName = wmi_HD.GetPropertyValue("Name").ToString();
                break; // TODO: might not be correct. Was : Exit For
            }


            // I haven't found the disk drive. Fail
            if (string.IsNullOrEmpty(deviceName.Trim()))
                return string.Empty;

            // -- Some problems in query parsing with backslash. Using like operator
            if (deviceName.StartsWith("\\\\.\\"))
            {
                deviceName = deviceName.Replace("\\\\.\\", "%");
            }


            // --- Physical Media
            searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia WHERE Tag like '" + deviceName + "'");
            var serial = string.Empty;
            foreach (ManagementObject wmi_HD in searcher.Get())
            {
                serial = wmi_HD.GetPropertyValue("SerialNumber").ToString();
                break; // TODO: might not be correct. Was : Exit For
            }

            return serial;

        }

    }
}