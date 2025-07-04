﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestFlow.Application.Models.Responses;
using TestFlow.Application.Models.Tests;

namespace TestFlow.Application.Interfaces.Services
{
    public interface ITestCaseGeneratorService
    {
        Task<List<TestCaseDto>> GenerateValidationTestsAsync(Guid endpointId, Guid userId);
        Task<List<TestCaseDto>> GenerateValidationTestsWithAIAsync(Guid endpointId, Guid userId);
        Task<RunTestResponse> RunValidationTestsAsync(Guid endpointId, Guid userId, bool artificialInteligence);
        Task<List<TestCaseDto>> GenerateFuzzyTestsAsync(Guid endpointId, Guid userId);
        Task<List<TestCaseDto>> GenerateAIFuzzyTestsAsync(Guid endpointId, Guid userId);
        Task<RunTestResponse> RunFuzzyTestsAsync(Guid endpointId, Guid userId, bool artificialInteligence);
        Task<List<TestCaseDto>> GenerateFunctionalTestsAsync(Guid endpointId, Guid userId);
        Task<List<TestCaseDto>> GenerateAIFunctionalTestsAsync(Guid endpointId, Guid userId);
        Task<RunTestResponse> RunFunctionalTestsAsync(Guid endpointId, Guid userId, bool artificialInteligence);
    }
}
