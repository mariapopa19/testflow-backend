using System.ComponentModel.DataAnnotations;
using TestFlow.Application.Models.CustomValidationAttributes;
using static TestFlow.Domain.Enums.HttpMethods;

namespace TestFlow.Application.Models.Requests;
public class CreateEndpointRequest
{
    [Required]
    public string Name { get; set; } = null!;
    [Required]
    [Url]
    public string Url { get; set; } = null!;
    [Required]
    [EnumDataType(typeof(HttpMethodEnum), ErrorMessage = "The Http Methods should be one of GET, POST, PUT, DELETE, PATCH.")]
    public string HttpMethod { get; set; } = null!;
    [RequiredIfNotGet("HttpMethod")]
    [JsonFormat]
    public string RequestBodyModel { get; set; } = null!;
    [JsonFormat]
    public string ResponseBodyModel { get; set; } = null!;
    public Dictionary<string, string>? Headers { get; set; } = null!;
}

