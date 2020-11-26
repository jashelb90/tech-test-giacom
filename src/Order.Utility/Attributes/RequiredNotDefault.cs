using System;
using System.ComponentModel.DataAnnotations;

namespace Order.Utility.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class RequiredNotDefault : ValidationAttribute
    {
        public RequiredNotDefault()
            : base("{0} can not be a default or empty value.")
        {
        }

        public override bool IsValid(object value)
        {
            if (value is null)
                return true;
            
            var type = value.GetType();
            
            return !Equals(value, Activator.CreateInstance(Nullable.GetUnderlyingType(type) ?? type));
        }
    }
    
}