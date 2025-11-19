using System.ComponentModel.DataAnnotations;

namespace Work360.Infrastructure.Attributes
{
    public class NotEmptyGuidAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is Guid guidValue)
                return guidValue != Guid.Empty;

            return false;
        }
    }
}