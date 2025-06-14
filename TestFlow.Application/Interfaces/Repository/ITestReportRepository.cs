using TestFlow.Domain.Entities;

namespace TestFlow.Application.Interfaces.Repository;
public interface ITestReportRepository
{
    Task<List<TestReport>> GetAllAsync(Guid userId);
    Task<TestReport?> GetByIdAsync(Guid id);
    Task<TestReport> CreateAsync(TestReport report);
    Task DeleteAsync(Guid id);
}
