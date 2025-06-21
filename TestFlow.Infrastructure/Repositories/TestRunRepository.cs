using Microsoft.EntityFrameworkCore;
using TestFlow.Application.Interfaces.Repository;
using TestFlow.Domain.Entities;

namespace TestFlow.Infrastructure.Repositories
{
    public class TestRunRepository : ITestRunRepository
    {
        private readonly ApplicationDbContext _context;

        public TestRunRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(TestRun run)
        {
            await _context.TestRuns.AddAsync(run);
            await _context.SaveChangesAsync();  
        }

        public async Task<List<TestRun>> GetByUserIdAsync(Guid userId)
        {
            return await _context.TestRuns
                .Include(tr => tr.Endpoint)
                .Include(tr => tr.Results)
                .ThenInclude(r => r.TestCase)
                .Where(tr => tr.UserId == userId)
                .ToListAsync();
        }
        public async Task<TestRun?> GetByIdAsync(Guid id, Guid userId)
        {
            return await _context.TestRuns
                .Include(tr => tr.Endpoint)
                .Include(tr => tr.Results)
                .ThenInclude(r => r.TestCase)
                .Where(tr => tr.UserId == userId && tr.Id == id)
                .FirstOrDefaultAsync();
        }
        public async Task<List<TestRun>> GetByEndpointIdAsync(Guid endpointId)
        {
            return await _context.TestRuns
                .Include(tr => tr.Endpoint)
                .Include(tr => tr.Results)
                .ThenInclude(r => r.TestCase)
                .Where(tr => tr.EndpointId == endpointId)
                .ToListAsync();
        }
        public async Task<List<TestRun>> GetAllAsync()
        {
            return await _context.TestRuns
                .ToListAsync();
        }
        public async Task<int> CountByUserAsync(Guid userId)
        {
            return await _context.TestRuns.CountAsync(tr => tr.UserId == userId);
        }

        public async Task<List<TestRun>> GetByUserIdSinceAsync(Guid userId, DateTime since)
        {
            return await _context.TestRuns
                .Where(tr => tr.UserId == userId && tr.StartedAt >= since)
                .ToListAsync();
        }
        public async Task UpdateAsync(TestRun run)
        {
            _context.TestRuns.Update(run);
            await _context.SaveChangesAsync();
        }
    }
}
