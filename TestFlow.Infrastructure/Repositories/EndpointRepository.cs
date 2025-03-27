using Microsoft.EntityFrameworkCore;
using TestFlow.Application.Intefaces;
using TestFlow.Domain.Entities;

namespace TestFlow.Infrastructure.Repositories;
public class EndpointRepository : Repository<Endpoint>, IEndpointRepository
{
    private readonly ApplicationDbContext _context;

    public EndpointRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public Task<Endpoint?> GetByUrlAsync(string url)
    {
        return _context.Endpoints.FirstOrDefaultAsync(e => e.Url == url);
    }
}
