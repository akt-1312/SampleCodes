using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Math.EC.Rfc7748;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HMS.Extensions
{
    public static class StringCompareFormatExtension
    {
        public static string StringCompareFormat(this string inputStr)
        {
            if(inputStr == null)
            {
                return inputStr;
            }
            else
            {
                inputStr = inputStr.Trim().ToLower();
                inputStr = inputStr.Replace(" ", string.Empty);
                return inputStr;
            }
        }
    }
}
