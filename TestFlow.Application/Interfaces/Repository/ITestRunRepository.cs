using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestFlow.Domain.Entities;

namespace TestFlow.Application.Interfaces.Repository
{
    public interface ITestRunRepository
    {
        Task AddAsync(TestRun testRun);
        Task<TestRun> GetByIdAsync(Guid id, Guid userId);
        Task<List<TestRun>> GetByEndpointIdAsync(Guid endpointId);
        Task<List<TestRun>> GetByUserIdAsync(Guid userId);
        Task<List<TestRun>> GetAllAsync();
    }
}
