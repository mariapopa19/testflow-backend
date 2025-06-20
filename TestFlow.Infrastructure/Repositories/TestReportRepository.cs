using Microsoft.EntityFrameworkCore;
using TestFlow.Application.Interfaces.Repository;
using TestFlow.Domain.Entities;

namespace TestFlow.Infrastructure.Repositories;
public class TestReportRepository : ITestReportRepository
{
    private readonly ApplicationDbContext _context;

    public TestReportRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TestReport>> GetAllAsync(Guid userId)
    {
        return await _context.TestReports
            .Include(r => r.TestRun)
                .ThenInclude(tr => tr.Endpoint)
            .Include(r => r.Results)
                .ThenInclude(tr => tr.TestCase)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<TestReport?> GetByIdAsync(Guid id)
    {
        return await _context.TestReports
            .Include(r => r.TestRun)
                .ThenInclude(tr => tr.Endpoint) // <-- Ensure this is included
            .Include(r => r.Results)
                .ThenInclude(tr => tr.TestCase)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<TestReport> CreateAsync(TestReport report)
    {
        await _context.TestReports.AddAsync(report);
        await _context.SaveChangesAsync();
        return report;
    }

    public async Task DeleteAsync(Guid id)
    {
        var report = await _context.TestReports.FindAsync(id);
        if (report != null)
        {
            _context.TestReports.Remove(report);
            await _context.SaveChangesAsync();
        }
    }
}
