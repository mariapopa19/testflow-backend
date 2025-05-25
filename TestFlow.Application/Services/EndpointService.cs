using System.Text.Json;
using TestFlow.Application.Interfaces.Repository;
using TestFlow.Application.Interfaces.Services;
using TestFlow.Application.Models.Requests;
using TestFlow.Application.Models.Responses;
using TestFlow.Domain.Entities;

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
            HttpMethod = e.HttpMethod,
            RequestBodyModel = e.RequestBodyModel,
            ResponseBodyModel = e.ResponseBodyModel
        }).ToList();
    }

    public async Task<EndpointResponse> CreateAsync(CreateEndpointRequest request, Guid userId)
    {
        var endpoint = new Endpoint
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Url = request.Url,
            HttpMethod = request.HttpMethod.ToString(),
            RequestBodyModel = request.RequestBodyModel,
            ResponseBodyModel = request.ResponseBodyModel,
            HeadersJson = request.Headers != null ? JsonSerializer.Serialize(request.Headers) : null,
            UserId = userId
        };

        await _repo.AddAsync(endpoint);
        return new EndpointResponse
        {
            Id = endpoint.Id,
            Name = endpoint.Name,
            Url = endpoint.Url,
            HttpMethod = endpoint.HttpMethod,
            RequestBodyModel = endpoint.RequestBodyModel,
            ResponseBodyModel = endpoint.ResponseBodyModel,
            HeadersJson = request.Headers != null ? JsonSerializer.Serialize(request.Headers) : null
        };
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var endpoint = await _repo.GetByIdAsync(id, userId);
        if (endpoint == null || endpoint.UserId != userId)
            return false;

        await _repo.DeleteAsync(endpoint);
        return true;
    }

    public async Task<EndpointResponse> GetUserEndpointByIdAsync(Guid id, Guid userId)
    {
        var endpoint = await _repo.GetByIdAsync(id, userId);
        if (endpoint == null)
            throw new InvalidOperationException("Endpoint not found");

        return new EndpointResponse
        {
            Id = endpoint.Id,
            Name = endpoint.Name,
            Url = endpoint.Url,
            HttpMethod = endpoint.HttpMethod,
            RequestBodyModel = endpoint.RequestBodyModel,
            ResponseBodyModel = endpoint.ResponseBodyModel
        };
    }

    public async Task<bool> UpdateEndpointAsync(Guid id, Guid userId, UpdateEndpointRequest request)
    {
        return await _repo.UpdateEndpointAsync(id, request);
    }
}

