using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HMS.Extensions
{
    public static class EnumDisplayNameExtension
    {
        public static string GetEnumDisplayName(this Enum enumValue)
        {
            var displayAttribute = enumValue.GetType()
                                    .GetMember(enumValue.ToString())
                                    .First()
                                    .GetCustomAttribute<DisplayAttribute>();

            bool isHasDisplayAttribute = displayAttribute == null ? false : true;

            if (isHasDisplayAttribute)
            {
                return displayAttribute.GetName();
            }
            else
            {
                return enumValue.ToString();
            }
        }
    }
}
