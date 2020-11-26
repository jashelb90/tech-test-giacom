using System;
using System.ComponentModel;

namespace Order.Utility
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum enumValue)
        {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            if (fieldInfo == null) return null;
            
            var attributes =
                (DescriptionAttribute[]) fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                
            return attributes.Length > 0 ? attributes[0].Description : enumValue.ToString();

        }
    }
}