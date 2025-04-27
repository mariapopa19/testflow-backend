using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestFlow.Application.Models.CustomValidationAttributes;
using static TestFlow.Domain.Enums.HttpMethods;

namespace TestFlow.Application.Models.Requests
{
    public class UpdateEndpointRequest
    {
        public string? Name { get; set; } = null!;
        [Url]
        public string? Url { get; set; } = null!;
        [EnumDataType(typeof(HttpMethodEnum), ErrorMessage = "The Http Methods should be one of GET, POST, PUT, DELETE, PATCH.")]
        public string? Method { get; set; } = null!;
        [JsonFormat]
        public string? RequestBodyModel { get; set; } = null!;
        [JsonFormat]
        public string? ResponseBodyModel { get; set; } = null!;
    }
}
