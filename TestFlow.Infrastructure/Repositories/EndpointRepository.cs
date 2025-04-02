using Microsoft.EntityFrameworkCore;
using TestFlow.Application.Interfaces;
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

    public async Task<Endpoint?> GetByIdAsync(Guid id)
    {
        return await _context.Endpoints.FindAsync(id);
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

}
