using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestFlow.Domain.Entities;

namespace TestFlow.Application.Interfaces.Repository
{
    public interface ITestResultRepository
    {
        Task AddAsync(TestResult testResult);
        Task<List<TestResult>> GetByTestRunIdAsync(Guid testRunId);
        Task UpdateAsync(TestResult testResult);
    }
}
