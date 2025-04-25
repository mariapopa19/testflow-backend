using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TestFlow.Application.Models.CustomValidationAttributes
{
    public class JsonFormatAttribute : ValidationAttribute
    {
        public JsonFormatAttribute()
        {
            ErrorMessage = "Invalid JSON format.";
        }

        public override bool IsValid(object? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return true; // optional field

            try
            {
                JsonConvert.DeserializeObject(value.ToString()!); // Fixed: Use JsonConvert instead of JsonConverter
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
