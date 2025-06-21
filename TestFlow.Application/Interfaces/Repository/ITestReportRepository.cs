using TestFlow.Domain.Entities;

namespace TestFlow.Application.Interfaces.Repository;
public interface ITestReportRepository
{
    Task<List<TestReport>> GetAllAsync(Guid userId);
    Task<TestReport?> GetByIdAsync(Guid id);
    Task<List<TestReport>> GetRecentByUserAsync(Guid userId, int limit);
    Task<TestReport> CreateAsync(TestReport report);
    Task DeleteAsync(Guid id);
    Task<IDisposable> BeginTransactionAsync();
}
