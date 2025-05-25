using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestFlow.Application.Models.Tests;

namespace TestFlow.Application.Interfaces.Services
{
    public interface ITestCaseGeneratorService
    {
        Task<List<TestCase>> GenerateValidationTestsAsync(Guid endpointId, Guid userId);
        Task<List<TestResultDto>> RunValidationTestsAsync(Guid endpointId, Guid userId);
        Task<List<TestCase>> GenerateFuzzyTestsAsync(Guid endpointId, Guid userId);
        Task<List<TestResultDto>> RunFuzzyTestsAsync(Guid endpointId, Guid userId);
    }
}
