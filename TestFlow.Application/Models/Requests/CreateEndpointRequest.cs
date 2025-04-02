using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestFlow.Domain.Enums;
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
}

