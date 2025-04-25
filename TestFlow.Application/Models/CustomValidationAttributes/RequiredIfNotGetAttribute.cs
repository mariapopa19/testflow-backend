using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestFlow.Application.Models.CustomValidationAttributes
{
    public class RequiredIfNotGetAttribute : ValidationAttribute
    {
        private readonly string _methodProperty;
        public RequiredIfNotGetAttribute(string methodProperty)
        {
            _methodProperty = methodProperty;
            ErrorMessage = "Request body is required for non-GET methods.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var methodProp = validationContext.ObjectType.GetProperty(_methodProperty);
            if (methodProp == null)
            {
                return new ValidationResult($"Unknown property: {_methodProperty}");
            }

            var methodValue = methodProp.GetValue(validationContext.ObjectInstance)?.ToString()?.ToUpperInvariant();

            if (methodValue != "GET" && string.IsNullOrWhiteSpace(value?.ToString()))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }

}
