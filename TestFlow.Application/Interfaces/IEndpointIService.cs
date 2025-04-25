using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestFlow.Application.Models.Requests;
using TestFlow.Application.Models.Responses;

namespace TestFlow.Application.Interfaces;
public interface IEndpointIService
{
    Task<List<EndpointResponse>> GetUserEndpointsAsync(Guid userId);
    Task<EndpointResponse> GetUserEndpointByIdAsync(Guid id, Guid userId);
    Task<EndpointResponse> CreateAsync(CreateEndpointRequest request, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid userId);
}
