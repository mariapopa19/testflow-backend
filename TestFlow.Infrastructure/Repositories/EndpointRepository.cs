using Microsoft.EntityFrameworkCore;
using TestFlow.Application.Interfaces;
using TestFlow.Application.Models.Requests;
using TestFlow.Domain.Entities;

namespace TestFlow.Infrastructure.Repositories;
public class EndpointRepository : IEndpointRepository
{
    private readonly ApplicationDbContext _context;

    public EndpointRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Endpoint>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Endpoints
            .Where(e => e.UserId == userId)
            .ToListAsync();
    }

    public async Task<Endpoint?> GetByIdAsync(Guid id, Guid userId)
    {
        return await _context.Endpoints
            .Where(e => e.UserId == userId && e.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(Endpoint endpoint)
    {
        await _context.Endpoints.AddAsync(endpoint);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Endpoint endpoint)
    {
        _context.Endpoints.Remove(endpoint);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateEndpointAsync(Guid id, UpdateEndpointRequest request)
    {
        var endpoint = await _context.Endpoints.FindAsync(id);

        if (endpoint == null)
            return false;

        if (!string.IsNullOrEmpty(request.Name))
            endpoint.Name = request.Name;

        if (!string.IsNullOrEmpty(request.Url))
            endpoint.Url = request.Url;

        if (!string.IsNullOrEmpty(request.Method))
            endpoint.HttpMethod = request.Method;

        if (!string.IsNullOrEmpty(request.RequestBodyModel))
            endpoint.RequestBodyModel = request.RequestBodyModel;

        if (!string.IsNullOrEmpty(request.ResponseBodyModel))
            endpoint.ResponseBodyModel = request.ResponseBodyModel;

        await _context.SaveChangesAsync();
        return true;
    }

}
