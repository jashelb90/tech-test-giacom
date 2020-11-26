using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Order.Utility.Attributes
{
    public class RequiredCollection: ValidationAttribute {

        public RequiredCollection() 
            : base("{0} can not be an empty collection") 
        {
        }

        public override bool IsValid(object value) {

            if (value is ICollection == false)
            {
                return false;
            }
            
            return ((ICollection) value).Count > 0;
        }
    }
}