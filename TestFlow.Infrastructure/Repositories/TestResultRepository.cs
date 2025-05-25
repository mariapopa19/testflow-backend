using Microsoft.EntityFrameworkCore;
using TestFlow.Application.Interfaces.Repository;
using TestFlow.Domain.Entities;

namespace TestFlow.Infrastructure.Repositories
{
    public class TestResultRepository : ITestResultRepository
    {
        private readonly ApplicationDbContext _context;
        public TestResultRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(TestResult testResult)
        {
            await _context.TestResults.AddAsync(testResult);
            await _context.SaveChangesAsync();
        }
        public async Task<List<TestResult>> GetByTestRunIdAsync(Guid testRunId)
        {
            return await _context.TestResults
                .Where(tr => tr.TestRunId == testRunId)
                .ToListAsync();
        }
    }
}
