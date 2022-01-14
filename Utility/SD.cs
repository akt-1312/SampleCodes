using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.Utility
{
    public class SD
    {
        public const string constDefinedMRNo = "AYT-";
        public const string constAutoMRNo = "AYT";
        public const int constMRNoCount9 = 9;
        public const int constMRNoCount12 = 12;
        public const string companyEmail = "softsoftwaresystem@gmail.com";
        public const string PatientRegImagesFolder = @"images\PatientRegistrationImages";
        public const string ConnectionStringFolder = @"TextFiles\ConnectionTextFiles";
        public const string RemarkTextFileFolder = @"TextFiles\RemarkFiles";
        public const string ExcelFolder = @"files\excelFiles\";
        public const string RegexPhoneNo = @"^(\d{9,12})$";

        //Initial User Account Creation Data
        public const string initialUserName = "softhmsadmin";
        public const string initialEmail = "softhmsadmin@gmail.com";
        public const string initialPhNo = "09911119";
        public const string initialPassword = "softhmsadmin2020";
    }
}
