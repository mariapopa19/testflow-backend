using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TestFlow.Application.Interfaces;
using TestFlow.Application.Models.Requests;
using TestFlow.Application.Models.Responses;
using TestFlow.Domain.Entities;
using static TestFlow.Domain.Enums.HttpMethods;

namespace TestFlow.Application.Services;
public class EndpointService : IEndpointIService
{
    private readonly IEndpointRepository _repo;

    public EndpointService(IEndpointRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<EndpointResponse>> GetUserEndpointsAsync(Guid userId)
    {
        var endpoints = await _repo.GetByUserIdAsync(userId);
        return endpoints.Select(e => new EndpointResponse
        {
            Id = e.Id,
            Name = e.Name,
            Url = e.Url,
            HttpMethod = e.HttpMethod
        }).ToList();
    }

    public async Task<EndpointResponse> CreateAsync(CreateEndpointRequest request, Guid userId)
    {
        var endpoint = new Endpoint
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Url = request.Url,
            HttpMethod = request.HttpMethod,
            UserId = userId
        };

        await _repo.AddAsync(endpoint);
        return new EndpointResponse
        {
            Id = endpoint.Id,
            Name = endpoint.Name,
            Url = endpoint.Url,
            HttpMethod = endpoint.HttpMethod
        };
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var endpoint = await _repo.GetByIdAsync(id);
        if (endpoint == null || endpoint.UserId != userId)
            return false;

        await _repo.DeleteAsync(endpoint);
        return true;
    }
}
