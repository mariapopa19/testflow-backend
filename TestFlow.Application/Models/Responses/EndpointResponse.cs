using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestFlow.Application.Models.Responses;
public class EndpointResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!; 
    public string Url { get; set; } = null!; 
    public string HttpMethod { get; set; } = null!;
}
