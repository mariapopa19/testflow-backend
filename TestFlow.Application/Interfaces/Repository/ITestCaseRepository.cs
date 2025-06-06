using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestFlow.Domain.Entities;

namespace TestFlow.Application.Interfaces.Repository;

public interface ITestCaseRepository
{
    Task AddAsync(TestCase testCase);
    Task UpdateAsync(TestCase testCase);
    Task<List<TestCase>> GetByEndpointIdAsync(Guid endpointId);
    Task<List<TestCase>> GetByEndpointIdAndTestTypeAsync(Guid endpointId, string testType);
}

