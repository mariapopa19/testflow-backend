using Microsoft.EntityFrameworkCore;
using TestFlow.Application.Interfaces.Repository;
using TestFlow.Domain.Entities;

namespace TestFlow.Infrastructure.Repositories;
public class TestCaseRepository : ITestCaseRepository
{
    private readonly ApplicationDbContext _context;

    public TestCaseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(TestCase testCase)
    {
        _context.TestCases.Add(testCase);
        await _context.SaveChangesAsync();
    }

    public async Task<List<TestCase>> GetByEndpointIdAsync(Guid endpointId)
    {
        return await _context.TestCases
            .Where(tc => tc.EndpointId == endpointId)
            .ToListAsync();
    }

    public async Task<List<TestCase>> GetByEndpointIdAndTestTypeAsync(Guid endpointId, string testType)
    {
        return await _context.TestCases
            .Where(tc => tc.EndpointId == endpointId && tc.Type == testType)
            .ToListAsync();
    }
}
