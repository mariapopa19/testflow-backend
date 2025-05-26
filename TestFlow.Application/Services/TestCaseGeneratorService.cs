using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TestFlow.Application.Interfaces.Repository;
using TestFlow.Application.Interfaces.Services;
using TestFlow.Application.Models.Tests;
using TestFlow.Application.Utils;
using TestFlow.Domain.Entities;

namespace TestFlow.Application.Services
{
    public class TestCaseGeneratorService : ITestCaseGeneratorService
    {
        private readonly IEndpointRepository _endpointRepository;
        private readonly ITestRunRepository _testRunRepository;
        private readonly ITestResultRepository _testResultRepository;
        private readonly IAIClientService _aiClientService;

        public TestCaseGeneratorService(
            IEndpointRepository endpointRepository,
            ITestRunRepository testRunRepository,
            ITestResultRepository testResultRepository,
            IAIClientService aiClientService)
        {
            _endpointRepository = endpointRepository;
            _testRunRepository = testRunRepository;
            _testResultRepository = testResultRepository;
            _aiClientService = aiClientService;
        }

        public async Task<List<TestCase>> GenerateValidationTestsAsync(Guid endpointId, Guid userId)
        {
            var endpoint = await _endpointRepository.GetByIdAsync(endpointId, userId);
            if (endpoint == null) throw new ArgumentException("Endpoint not found");

            var testCases = new List<TestCase>();

            if (endpoint.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                testCases.Add(new TestCase
                {
                    Type = "Validation",
                    Input = string.Empty,
                    ExpectedStatusCode = ExpectedStatusCodeProvider.GetExpectedStatusCodes("Validation", endpoint.HttpMethod, "Success")
                });

                return testCases;
            }

            var model = JsonDocument.Parse(endpoint.RequestBodyModel).RootElement;

            // Caz valid complet (toate câmpurile corecte)
            var validInput = TestDataFaker.GenerateValidInput(model);
            testCases.Add(new TestCase
            {
                Type = "Validation",
                Input = JsonSerializer.Serialize(validInput),
                ExpectedStatusCode = ExpectedStatusCodeProvider.GetExpectedStatusCodes("Validation", endpoint.HttpMethod, "Success")
            });

            foreach (var prop in model.EnumerateObject())
            {
                // 1. Caz: lipsa unei proprietăți
                var withoutProp = new Dictionary<string, object?>(validInput);
                withoutProp.Remove(prop.Name);

                testCases.Add(new TestCase
                {
                    Type = "Validation",
                    Input = JsonSerializer.Serialize(withoutProp),
                    ExpectedStatusCode = ExpectedStatusCodeProvider.GetExpectedStatusCodes("Validation", endpoint.HttpMethod, "ClientError")
                });

                // 2. Caz: valoare invalidă
                var corrupted = new Dictionary<string, object?>(validInput);
                corrupted[prop.Name] = "###INVALID###";

                testCases.Add(new TestCase
                {
                    Type = "Validation",
                    Input = JsonSerializer.Serialize(corrupted),
                    ExpectedStatusCode = ExpectedStatusCodeProvider.GetExpectedStatusCodes("Validation", endpoint.HttpMethod, "ClientError")
                });
            }
            return testCases;
        }

        public async Task<List<TestCase>> GenerateValidationTestsWithAIAsync(Guid endpointId, Guid userId)
        {
            var endpoint = await _endpointRepository.GetByIdAsync(endpointId, userId);
            if (endpoint == null) throw new ArgumentException("Endpoint not found");

            var prompt = AIResponseHelper.GenerateAIPrompt("validation", endpoint);

            var rawJson = await _aiClientService.GetPromptResponseAsync(prompt);

            var testCases = AIResponseHelper.ExtractTestCasesFromRaw(rawJson!);
            return testCases ?? new List<TestCase>();
        }

        public async Task<List<TestResultDto>> RunValidationTestsAsync(Guid endpointId, Guid userId, bool artificialInteligence)
        {
            var endpoint = await _endpointRepository.GetByIdAsync(endpointId, userId);
            if (endpoint == null) throw new ArgumentException("Endpoint not found");

            List<TestCase> testCases;
            if (artificialInteligence)
            {
                testCases = await GenerateValidationTestsWithAIAsync(endpointId, userId);
            }
            else
            {
                testCases = await GenerateValidationTestsAsync(endpointId, userId);
            }

            var resultDtos = new List<TestResultDto>();

            var testRun = new TestRun
            {
                Id = Guid.NewGuid(),
                EndpointId = endpointId,
                UserId = userId,
                StartedAt = DateTime.UtcNow,
                TestType = "Validation"
            };
            await _testRunRepository.AddAsync(testRun);

            using var httpClient = new HttpClient();

            foreach (var test in testCases)
            {
                var method = new HttpMethod(endpoint.HttpMethod.ToUpper());
                var request = new HttpRequestMessage(method, endpoint.Url)
                {
                    Content = new StringContent(test.Input, Encoding.UTF8, "application/json")
                };

                if (!method.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
                {
                    request.Content = new StringContent(test.Input, Encoding.UTF8, "application/json");
                }

                var headers = !string.IsNullOrWhiteSpace(endpoint.HeadersJson)
                    ? JsonSerializer.Deserialize<Dictionary<string, string>>(endpoint.HeadersJson)
                    : new Dictionary<string, string>();

                foreach (var kvp in headers!)
                {
                    request.Headers.Add(kvp.Key, kvp.Value);
                }

                var response = await httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                var passed = test.ExpectedStatusCode?.Contains((int)response.StatusCode) == true;

                var testResult = new TestResult
                {
                    Id = Guid.NewGuid(),
                    TestRunId = testRun.Id,
                    StartedAt = DateTime.UtcNow,
                    Outcome = passed ? "Pass" : "Fail",
                    Details = JsonSerializer.Serialize(new
                    {
                        test.Type,
                        test.Input,
                        test.ExpectedStatusCode,
                        ActualStatusCode = (int)response.StatusCode,
                        ResponseBody = responseBody
                    })
                };

                await _testResultRepository.AddAsync(testResult);

                resultDtos.Add(new TestResultDto
                {
                    TestCaseType = test.Type,
                    Input = test.Input,
                    ExpectedStatusCode = test.ExpectedStatusCode,
                    ActualStatusCode = (int)response.StatusCode,
                    Passed = passed,
                    ResponseBody = responseBody
                });
            }

            return resultDtos;
        }

        public async Task<List<TestCase>> GenerateAIFuzzyTestsAsync(Guid endpointId, Guid userId)
        {
            var endpoint = await _endpointRepository.GetByIdAsync(endpointId, userId);
            if (endpoint == null) throw new ArgumentException("Endpoint not found");

            var prompt = AIResponseHelper.GenerateAIPrompt("fuzzy", endpoint);

            var rawJson = await _aiClientService.GetPromptResponseAsync(prompt);

            var testCases = AIResponseHelper.ExtractTestCasesFromRaw(rawJson!);
            return testCases ?? new List<TestCase>();
        }

        public async Task<List<TestCase>> GenerateFuzzyTestsAsync(Guid endpointId, Guid userId)
        {
            var endpoint = await _endpointRepository.GetByIdAsync(endpointId, userId);
            if (endpoint == null) throw new ArgumentException("Endpoint not found");

            var testCases = new List<TestCase>();

            if (endpoint.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                testCases.Add(new TestCase
                {
                    Type = "Fuzzy",
                    Input = string.Empty,
                    ExpectedStatusCode = ExpectedStatusCodeProvider.GetExpectedStatusCodes("Validation", endpoint.HttpMethod, "Success")
                });
                return testCases;
            }

            var model = JsonDocument.Parse(endpoint.RequestBodyModel).RootElement;

            var baseInput = TestDataFaker.GenerateValidInput(model);

            // Test 1: toate valorile null
            var allNulls = baseInput.ToDictionary(kvp => kvp.Key, kvp => (object?)null);
            testCases.Add(new TestCase
            {
                Type = "Fuzzy",
                Input = JsonSerializer.Serialize(allNulls),
                ExpectedStatusCode = ExpectedStatusCodeProvider.GetExpectedStatusCodes("Validation", endpoint.HttpMethod, "ClientError")
            });

            // Test 2: tipuri greșite
            var wrongTypes = baseInput.ToDictionary(kvp => kvp.Key, kvp => (object?)new[] { 1, 2, 3 });
            testCases.Add(new TestCase
            {
                Type = "Fuzzy",
                Input = JsonSerializer.Serialize(wrongTypes),
                ExpectedStatusCode = ExpectedStatusCodeProvider.GetExpectedStatusCodes("Validation", endpoint.HttpMethod, "ClientError")
            });

            // Test 3: câmpuri lipsă și extra
            var partialInput = new Dictionary<string, object?>();
            foreach (var key in baseInput.Keys.Take(baseInput.Count / 2))
            {
                partialInput[key] = baseInput[key];
            }
            partialInput["__extra_field__"] = 12345;

            testCases.Add(new TestCase
            {
                Type = "Fuzzy",
                Input = JsonSerializer.Serialize(partialInput),
                ExpectedStatusCode = ExpectedStatusCodeProvider.GetExpectedStatusCodes("Validation", endpoint.HttpMethod, "ClientError")
            });

            return testCases;
        }

        public async Task<List<TestResultDto>> RunFuzzyTestsAsync(Guid endpointId, Guid userId, bool artificialInteligence)
        {
            var endpoint = await _endpointRepository.GetByIdAsync(endpointId, userId);
            if (endpoint == null) throw new ArgumentException("Endpoint not found");

            List<TestCase> testCases;
            if (artificialInteligence)
            {
                testCases = await GenerateAIFuzzyTestsAsync(endpointId, userId);
            }
            else
            {
                testCases = await GenerateFuzzyTestsAsync(endpointId, userId);
            }

            var resultDtos = new List<TestResultDto>();

            var testRun = new TestRun
            {
                Id = Guid.NewGuid(),
                EndpointId = endpointId,
                UserId = userId,
                StartedAt = DateTime.UtcNow,
                TestType = "Fuzzy"
            };
            await _testRunRepository.AddAsync(testRun);

            Dictionary<string, string> headers = new();
            if (!string.IsNullOrWhiteSpace(endpoint.HeadersJson))
            {
                headers = JsonSerializer.Deserialize<Dictionary<string, string>>(endpoint.HeadersJson!) ?? new();
            }

            using var httpClient = new HttpClient();

            foreach (var test in testCases)
            {
                var method = new HttpMethod(endpoint.HttpMethod.ToUpper());
                var request = new HttpRequestMessage(method, endpoint.Url);

                if (!method.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
                {
                    request.Content = new StringContent(test.Input, Encoding.UTF8, "application/json");
                }

                foreach (var header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                var response = await httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                var passed = test.ExpectedStatusCode?.Contains((int)response.StatusCode) == true;

                var testResult = new TestResult
                {
                    Id = Guid.NewGuid(),
                    TestRunId = testRun.Id,
                    Outcome = passed ? "Pass" : "Fail",
                    Details = JsonSerializer.Serialize(new
                    {
                        test.Type,
                        test.Input,
                        ExpectedStatusCode = test.ExpectedStatusCode,
                        ActualStatusCode = (int)response.StatusCode,
                        ResponseBody = responseBody
                    })
                };

                await _testResultRepository.AddAsync(testResult);

                resultDtos.Add(new TestResultDto
                {
                    TestCaseType = test.Type,
                    Input = test.Input,
                    ActualStatusCode = (int)response.StatusCode,
                    Passed = passed,
                    ResponseBody = responseBody
                });
            }

            return resultDtos;
        }

        public async Task<List<TestCase>> GenerateFunctionalTestsAsync(Guid endpointId, Guid userId)
        {
            var endpoint = await _endpointRepository.GetByIdAsync(endpointId, userId);
            if (endpoint == null) throw new ArgumentException("Endpoint not found");

            var model = JsonDocument.Parse(endpoint.RequestBodyModel).RootElement;
            var validInput = TestDataFaker.GenerateValidInput(model);

            var testCases = new List<TestCase>
            {
                new TestCase
                {
                    Type = "Functional",
                    Input = JsonSerializer.Serialize(validInput),
                    ExpectedStatusCode = ExpectedStatusCodeProvider.GetExpectedStatusCodes("Functional", endpoint.HttpMethod, "Success")
                }
            };

            return testCases;
        }

        public async Task<List<TestCase>> GenerateAIFunctionalTestsAsync(Guid endpointId, Guid userId)
        {
            var endpoint = await _endpointRepository.GetByIdAsync(endpointId, userId);
            if (endpoint == null) throw new ArgumentException("Endpoint not found");

            var prompt = AIResponseHelper.GenerateAIPrompt("functional", endpoint);

            var rawJson = await _aiClientService.GetPromptResponseAsync(prompt);

            var testCases = AIResponseHelper.ExtractTestCasesFromRaw(rawJson!);
            return testCases ?? new List<TestCase>();
        }

        public async Task<List<TestResultDto>> RunFunctionalTestsAsync(Guid endpointId, Guid userId, bool artificialInteligence)
        {
            var endpoint = await _endpointRepository.GetByIdAsync(endpointId, userId);
            if (endpoint == null) throw new ArgumentException("Endpoint not found");

            List<TestCase> testCases;

            if (artificialInteligence)
            {
                testCases = await GenerateAIFunctionalTestsAsync(endpointId, userId);
            }
            else
            {
                testCases = await GenerateFunctionalTestsAsync(endpointId, userId);
            }

            var resultDtos = new List<TestResultDto>();

            var testRun = new TestRun
            {
                Id = Guid.NewGuid(),
                EndpointId = endpointId,
                UserId = userId,
                StartedAt = DateTime.UtcNow,
                TestType = "Functional"
            };
            await _testRunRepository.AddAsync(testRun);

            var headers = !string.IsNullOrWhiteSpace(endpoint.HeadersJson)
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(endpoint.HeadersJson!) ?? new()
                : new Dictionary<string, string>();

            using var httpClient = new HttpClient();

            foreach (var test in testCases)
            {
                var method = new HttpMethod(endpoint.HttpMethod.ToUpper());
                var request = new HttpRequestMessage(method, endpoint.Url);

                if (!method.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
                {
                    request.Content = new StringContent(test.Input, Encoding.UTF8, "application/json");
                }

                foreach (var header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                var response = await httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                var passed = test.ExpectedStatusCode?.Contains((int)response.StatusCode) == true;

                var testResult = new TestResult
                {
                    Id = Guid.NewGuid(),
                    TestRunId = testRun.Id,
                    Outcome = passed ? "Pass" : "Fail",
                    Details = JsonSerializer.Serialize(new
                    {
                        test.Type,
                        test.Input,
                        test.ExpectedStatusCode,
                        ActualStatusCode = (int)response.StatusCode,
                        ResponseBody = responseBody
                    })
                };

                await _testResultRepository.AddAsync(testResult);

                resultDtos.Add(new TestResultDto
                {
                    TestCaseType = test.Type,
                    Input = test.Input,
                    ActualStatusCode = (int)response.StatusCode,
                    Passed = passed,
                    ResponseBody = responseBody
                });
            }

            return resultDtos;
        }

    }
}
