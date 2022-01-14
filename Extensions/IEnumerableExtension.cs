using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.Extensions
{
    public static class IEnumerableExtension
    {
        public static IEnumerable<SelectListItem> ToSelectListItem<T>(this IEnumerable<T> items, int selectedValue, string value, string text)
        {
            return from item in items
                   select new SelectListItem
                   {                    
                       Text = item.GetPropertyValue(text),
                       Value = item.GetPropertyValue(value),
                       Selected = item.GetPropertyValue(value).Equals(selectedValue.ToString())
                   };
        }

        public static IEnumerable<SelectListItem> ToSelectListItemString<T>(this IEnumerable<T> items, string selectedValue,string value,string text)
        {
            if (selectedValue == null)
                selectedValue = "";
            return from item in items
                   select new SelectListItem
                   {
                       Text = item.GetPropertyValue(text),
                       Value = item.GetPropertyValue(value),
                       Selected = item.GetPropertyValue(value).Equals(selectedValue.ToString())
                   };
        }
    }
}
