using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TestFlow.Application.Interfaces.Repository;
using TestFlow.Application.Interfaces.Services;
using TestFlow.Application.Models.Responses;
using TestFlow.Application.Models.Tests;
using TestFlow.Application.Utils;
using TestFlow.Domain.Entities;

namespace TestFlow.Application.Services
{
    public class TestCaseGeneratorService : ITestCaseGeneratorService
    {
        private readonly ILogger<TestCaseGeneratorService> _logger;
        private readonly IEndpointRepository _endpointRepository;
        private readonly ITestRunRepository _testRunRepository;
        private readonly ITestResultRepository _testResultRepository;
        private readonly IAIClientService _aiClientService;
        private readonly ITestCaseRepository _testCaseRepository;

        public TestCaseGeneratorService(
            IEndpointRepository endpointRepository,
            ITestRunRepository testRunRepository,
            ITestResultRepository testResultRepository,
            IAIClientService aiClientService,
            ITestCaseRepository testCaseRepository,
            ILogger<TestCaseGeneratorService> logger)
        {
            _endpointRepository = endpointRepository;
            _testRunRepository = testRunRepository;
            _testResultRepository = testResultRepository;
            _aiClientService = aiClientService;
            _testCaseRepository = testCaseRepository;
            _logger = logger;
        }

        public async Task<List<TestCaseDto>> GenerateValidationTestsAsync(Guid endpointId, Guid userId)
        {
            var endpoint = await _endpointRepository.GetByIdAsync(endpointId, userId);
            if (endpoint == null)
            {
                _logger.LogWarning("Endpoint {EndpointId} not found for user {UserId}", endpointId, userId);
                throw new ArgumentException("Endpoint not found");
            }

            var testCases = new List<TestCase>();

            if (endpoint.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                // Test 1: Basic valid GET request
                testCases.Add(new TestCase
                {
                    Id = Guid.NewGuid(),
                    EndpointId = endpointId,
                    Type = "Validation",
                    Input = string.Empty,
                    ExpectedResponse = endpoint.ResponseBodyModel,
                    ExpectedStatusCode = ExpectedStatusCodeProvider.GetExpectedStatusCodes("Validation", endpoint.HttpMethod, "Success"),
                    CreatedAt = DateTime.UtcNow
                });

                // Test 2: GET with invalid query parameter
                testCases.Add(new TestCase
                {
                    Id = Guid.NewGuid(),
                    EndpointId = endpointId,
                    Type = "Validation",
                    Input = string.Empty,
                    CustomUrl = endpoint.Url + "?invalidParam=test",
                    ExpectedStatusCode = ExpectedStatusCodeProvider.GetExpectedStatusCodes("Validation", endpoint.HttpMethod, "ClientError"),
                    CreatedAt = DateTime.UtcNow
                });

                // Test 3: GET with malformed query
                testCases.Add(new TestCase
                {
                    Id = Guid.NewGuid(),
                    EndpointId = endpointId,
                    Type = "Validation",
                    Input = string.Empty,
                    CustomUrl = endpoint.Url + "?id=",
                    ExpectedStatusCode = ExpectedStatusCodeProvider.GetExpectedStatusCodes("Validation", endpoint.HttpMethod, "ClientError"),
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                // Keep your existing logic for non-GET methods
                var model = JsonDocument.Parse(endpoint.RequestBodyModel).RootElement;
                var validInput = TestDataFaker.GenerateValidInput(model);

                testCases.Add(new TestCase
                {
                    Id = Guid.NewGuid(),
                    EndpointId = endpointId,
                    Type = "Validation",
                    Input = JsonSerializer.Serialize(validInput),
                    ExpectedResponse = endpoint.ResponseBodyModel,
                    ExpectedStatusCode = ExpectedStatusCodeProvider.GetExpectedStatusCodes("Validation", endpoint.HttpMethod, "Success"),
                    CreatedAt = DateTime.UtcNow
                });

                foreach (var prop in model.EnumerateObject())
                {
                    var withoutProp = new Dictionary<string, object?>(validInput);
                    withoutProp.Remove(prop.Name);

                    testCases.Add(new TestCase
                    {
                        Id = Guid.NewGuid(),
                        EndpointId = endpointId,
                        Type = "Validation",
                        Input = JsonSerializer.Serialize(withoutProp),
                        ExpectedResponse = endpoint.ResponseBodyModel,
                        ExpectedStatusCode = ExpectedStatusCodeProvider.GetExpectedStatusCodes("Validation", endpoint.HttpMethod, "ClientError"),
                        CreatedAt = DateTime.UtcNow
                    });

                    var corrupted = new Dictionary<string, object?>(validInput);
                    corrupted[prop.Name] = "###INVALID###";

                    testCases.Add(new TestCase
                    {
                        Id = Guid.NewGuid(),
                        EndpointId = endpointId,
                        Type = "Validation",
                        Input = JsonSerializer.Serialize(corrupted),
                        ExpectedResponse = endpoint.ResponseBodyModel,
                        ExpectedStatusCode = ExpectedStatusCodeProvider.GetExpectedStatusCodes("Validation", endpoint.HttpMethod, "ClientError"),
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            // Save all test cases to DB
            foreach (var testCase in testCases)
            {
                await _testCaseRepository.AddAsync(testCase);
            }

            // Map to DTOs - INCLUDE CustomUrl here!
            var dtos = testCases.Select(tc => new TestCaseDto
            {
                Type = tc.Type,
                Input = tc.Input,
                ExpectedStatusCode = tc.ExpectedStatusCode,
                ExpectedResponse = tc.ExpectedResponse,
                CustomUrl = tc.CustomUrl // ← ADD THIS LINE
            }).ToList();

            return dtos;
        }

        public async Task<List<TestCaseDto>> GenerateValidationTestsWithAIAsync(Guid endpointId, Guid userId)
        {
            var endpoint = await _endpointRepository.GetByIdAsync(endpointId, userId);
            if (endpoint == null)
            {
                _logger.LogWarning("Endpoint {EndpointId} not found for user {UserId}", endpointId, userId);
                throw new ArgumentException("Endpoint not found");
            }

            var prompt = AIResponseHelper.GenerateAIPrompt("validation", endpoint);
            var rawJson = await _aiClientService.GetPromptResponseAsync(prompt);
            var testCases = AIResponseHelper.ExtractTestCasesFromRaw(rawJson!, "Validation");

            // Save to DB
            foreach (var testCase in testCases)
            {
                testCase.Id = Guid.NewGuid();
                testCase.EndpointId = endpointId;
                testCase.CreatedAt = DateTime.UtcNow;
                await _testCaseRepository.AddAsync(testCase);
            }

            // Map to DTOs
            return testCases.Select(tc => new TestCaseDto
            {
                Type = tc.Type,
                Input = tc.Input,
                ExpectedStatusCode = tc.ExpectedStatusCode,
                ExpectedResponse = tc.ExpectedResponse,
                CustomUrl = tc.CustomUrl // Include CustomUrl in DTO
            }).ToList();
        }

        public async Task<RunTestResponse> RunValidationTestsAsync(Guid endpointId, Guid userId, bool artificialInteligence)
        {
            var endpoint = await _endpointRepository.GetByIdAsync(endpointId, userId);
            if (endpoint == null)
            {
                _logger.LogWarning("Endpoint {EndpointId} not found for user {UserId}", endpointId, userId);
                throw new ArgumentException("Endpoint not found");
            }

            var testCases = await _testCaseRepository.GetByEndpointIdAndTestTypeAsync(endpointId, "Validation");

            var startTime = DateTime.UtcNow;
            var testRun = new TestRun
            {
                Id = Guid.NewGuid(),
                EndpointId = endpointId,
                UserId = userId,
                StartedAt = startTime,
                TestType = "Validation"
            };
            await _testRunRepository.AddAsync(testRun);

            if (testCases == null || testCases.Count == 0)
            {
                List<TestCase> generated;
                if (artificialInteligence)
                {
                    // Generate and save new test cases (as entities)
                    generated = (await GenerateValidationTestsWithAIAsync(endpointId, userId))
                        .Select(dto => new TestCase
                        {
                            Id = Guid.NewGuid(),
                            EndpointId = endpointId,
                            Type = dto.Type,
                            Input = dto.Input,
                            ExpectedStatusCode = dto.ExpectedStatusCode,
                            ExpectedResponse = dto.ExpectedResponse,
                            CustomUrl = dto.CustomUrl,
                            CreatedAt = DateTime.UtcNow,
                            TestRunId = testRun.Id
                        }).ToList();
                }
                else
                {
                    generated = (await GenerateValidationTestsAsync(endpointId, userId))
                        .Select(dto => new TestCase
                        {
                            Id = Guid.NewGuid(),
                            EndpointId = endpointId,
                            Type = dto.Type,
                            Input = dto.Input,
                            ExpectedStatusCode = dto.ExpectedStatusCode,
                            ExpectedResponse = dto.ExpectedResponse,
                            CustomUrl = dto.CustomUrl,
                            CreatedAt = DateTime.UtcNow,
                            TestRunId = testRun.Id
                        }).ToList();
                }
                foreach (var tc in generated)
                {
                    await _testCaseRepository.AddAsync(tc);
                }
                testCases = generated;
            }
            else
            {
                // If test cases already exist, update their TestRunId
                foreach (var tc in testCases)
                {
                    tc.TestRunId = testRun.Id;
                    await _testCaseRepository.UpdateAsync(tc);
                }
            }

            var resultDtos = new List<TestResultDto>();

            using var httpClient = new HttpClient();

            foreach (var test in testCases)
            {
                var method = new HttpMethod(endpoint.HttpMethod.ToUpper());
                var urlToCall = !string.IsNullOrEmpty(test.CustomUrl) ? test.CustomUrl : endpoint.Url;
                var request = new HttpRequestMessage(method, urlToCall);

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
                HttpResponseMessage response = null; 
                try
                {
                    response = await httpClient.SendAsync(request);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send request to {Url}", endpoint.Url);
                    continue; 
                }
                var endTime = DateTime.UtcNow;

                var responseBody = await response.Content.ReadAsStringAsync();

                var passed = test.ExpectedStatusCode?.Contains((int)response.StatusCode) == true;

                var testResult = new TestResult
                {
                    Id = Guid.NewGuid(),
                    TestRunId = testRun.Id,
                    TestCaseId = test.Id,
                    StartedAt = startTime,
                    FinishedAt = endTime,
                    Duration = endTime - startTime,
                    CalledUrl = urlToCall,
                    Outcome = passed ? "Pass" : "Fail",
                    Details = JsonSerializer.Serialize(new
                    {
                        TestCaseType = test.Type,
                        Input = test.Input,
                        ExpectedStatusCode = test.ExpectedStatusCode,
                        ActualStatusCode = (int)response.StatusCode,
                        ResponseBody = responseBody
                    })

                };

                await _testResultRepository.AddAsync(testResult);

                resultDtos.Add(new TestResultDto
                {
                    Id = testResult.Id,
                    TestCaseType = test.Type,
                    Input = test.Input,
                    ExpectedStatusCode = test.ExpectedStatusCode,
                    ActualStatusCode = (int)response.StatusCode,
                    Passed = passed,
                    ResponseBody = responseBody,
                    Duration = testResult.Duration,
                    CalledUrl = urlToCall
                });
            }

            testRun.FinishedAt = DateTime.UtcNow;
            testRun.Duration = testRun.FinishedAt - testRun.StartedAt;
            await _testRunRepository.UpdateAsync(testRun);

            try
            {
                if (resultDtos.Count <= 0) throw new Exception("Result DTOs is empty.");

                return new RunTestResponse
                {
                    TestRunId = testRun.Id,
                    Results = resultDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to run functional tests for endpoint {EndpointId}", endpointId);
                throw new InvalidOperationException("Failed to run functional tests.", ex);
            }
        }

        public async Task<List<TestCaseDto>> GenerateAIFuzzyTestsAsync(Guid endpointId, Guid userId)
        {
            var endpoint = await _endpointRepository.GetByIdAsync(endpointId, userId);
            if (endpoint == null)
            {
                _logger.LogWarning("Endpoint {EndpointId} not found for user {UserId}", endpointId, userId);
                throw new ArgumentException("Endpoint not found");
            }

            var prompt = AIResponseHelper.GenerateAIPrompt("fuzzy", endpoint);

            var rawJson = await _aiClientService.GetPromptResponseAsync(prompt);

            var testCases = AIResponseHelper.ExtractTestCasesFromRaw(rawJson!, "Fuzzy");

            // Save to DB
            foreach (var testCase in testCases)
            {
                testCase.Id = Guid.NewGuid();
                testCase.EndpointId = endpointId;
                testCase.CreatedAt = DateTime.UtcNow;
                await _testCaseRepository.AddAsync(testCase);
            }

            // Map to DTOs
            return testCases.Select(tc => new TestCaseDto
            {
                Type = tc.Type,
                Input = tc.Input,
                ExpectedStatusCode = tc.ExpectedStatusCode,
                ExpectedResponse = tc.ExpectedResponse,
                CustomUrl = tc.CustomUrl
            }).ToList();
        }

        public async Task<List<TestCaseDto>> GenerateFuzzyTestsAsync(Guid endpointId, Guid userId)
        {
            var endpoint = await _endpointRepository.GetByIdAsync(endpointId, userId);
            if (endpoint == null)
            {
                _logger.LogWarning("Endpoint {EndpointId} not found for user {UserId}", endpointId, userId);
                throw new ArgumentException("Endpoint not found");
            }

            var testCases = new List<TestCase>();

            if (endpoint.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                // Test 1: URL valid (fără parametri)
                testCases.Add(new TestCase
                {
                    Id = Guid.NewGuid(),
                    EndpointId = endpointId,
                    Type = "Fuzzy",
                    Input = string.Empty,
                    ExpectedStatusCode = new List<int> { 200 },
                    CustomUrl = endpoint.Url, // URL original
                    CreatedAt = DateTime.UtcNow
                });

                // Test 2: URL cu parametru random
                testCases.Add(new TestCase
                {
                    Id = Guid.NewGuid(),
                    EndpointId = endpointId,
                    Type = "Fuzzy",
                    Input = string.Empty,
                    ExpectedStatusCode = new List<int> { 404, 500 },
                    CustomUrl = endpoint.Url + "?randomparam=xyz",
                    CreatedAt = DateTime.UtcNow
                });

                // Test 3: URL distorsionat
                testCases.Add(new TestCase
                {
                    Id = Guid.NewGuid(),
                    EndpointId = endpointId,
                    Type = "Fuzzy",
                    Input = string.Empty,
                    ExpectedStatusCode = new List<int> { 404, 500 },
                    CustomUrl = endpoint.Url.TrimEnd('/') + "/invalidpath",
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                var model = JsonDocument.Parse(endpoint.RequestBodyModel).RootElement;

                var baseInput = TestDataFaker.GenerateValidInput(model);

                // Test 1: toate valorile null
                var allNulls = baseInput.ToDictionary(kvp => kvp.Key, kvp => (object?)null);
                testCases.Add(new TestCase
                {
                    Id = Guid.NewGuid(),
                    EndpointId = endpointId,
                    Type = "Fuzzy",
                    Input = JsonSerializer.Serialize(allNulls),
                    ExpectedStatusCode = ExpectedStatusCodeProvider.GetExpectedStatusCodes("Validation", endpoint.HttpMethod, "ClientError"),
                    CreatedAt = DateTime.UtcNow
                });

                // Test 2: tipuri greșite
                var wrongTypes = baseInput.ToDictionary(kvp => kvp.Key, kvp => (object?)new[] { 1, 2, 3 });
                testCases.Add(new TestCase
                {
                    Id = Guid.NewGuid(),
                    EndpointId = endpointId,
                    Type = "Fuzzy",
                    Input = JsonSerializer.Serialize(wrongTypes),
                    ExpectedStatusCode = ExpectedStatusCodeProvider.GetExpectedStatusCodes("Validation", endpoint.HttpMethod, "ClientError"),
                    CreatedAt = DateTime.UtcNow
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
                    Id = Guid.NewGuid(),
                    EndpointId = endpointId,
                    Type = "Fuzzy",
                    Input = JsonSerializer.Serialize(partialInput),
                    ExpectedStatusCode = ExpectedStatusCodeProvider.GetExpectedStatusCodes("Validation", endpoint.HttpMethod, "ClientError"),
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Save all test cases to DB
            foreach (var testCase in testCases)
            {
                await _testCaseRepository.AddAsync(testCase);
            }

            // Map to DTOs
            List<TestCaseDto> dtos = testCases.Select(tc => new TestCaseDto
            {
                Type = tc.Type,
                Input = tc.Input,
                ExpectedStatusCode = tc.ExpectedStatusCode,
                ExpectedResponse = tc.ExpectedResponse,
                CustomUrl = tc.CustomUrl
            }).ToList();

            return dtos.Count > 0 ? dtos : throw new InvalidOperationException("No test cases generated for Fuzzy tests.");
        }

        public async Task<RunTestResponse> RunFuzzyTestsAsync(Guid endpointId, Guid userId, bool artificialInteligence)
        {
            var endpoint = await _endpointRepository.GetByIdAsync(endpointId, userId);
            if (endpoint == null)
            {
                _logger.LogWarning("Endpoint {EndpointId} not found for user {UserId}", endpointId, userId);
                throw new ArgumentException("Endpoint not found");
            }

            var testCases = await _testCaseRepository.GetByEndpointIdAndTestTypeAsync(endpointId, "Fuzzy");

            var startTime = DateTime.UtcNow;
            var testRun = new TestRun
            {
                Id = Guid.NewGuid(),
                EndpointId = endpointId,
                UserId = userId,
                StartedAt = startTime,
                TestType = "Fuzzy"
            };
            await _testRunRepository.AddAsync(testRun);

            if (testCases == null || testCases.Count == 0)
            {
                List<TestCase> generated;
                if (artificialInteligence)
                {
                    // Generate and save new test cases (as entities)
                    generated = (await GenerateAIFuzzyTestsAsync(endpointId, userId))
                        .Select(dto => new TestCase
                        {
                            Id = Guid.NewGuid(),
                            EndpointId = endpointId,
                            Type = dto.Type,
                            Input = dto.Input,
                            ExpectedStatusCode = dto.ExpectedStatusCode,
                            ExpectedResponse = dto.ExpectedResponse,
                            CustomUrl = dto.CustomUrl,
                            CreatedAt = DateTime.UtcNow,
                            TestRunId = testRun.Id
                        }).ToList();
                }
                else
                {
                    generated = (await GenerateFuzzyTestsAsync(endpointId, userId))
                        .Select(dto => new TestCase
                        {
                            Id = Guid.NewGuid(),
                            EndpointId = endpointId,
                            Type = dto.Type,
                            Input = dto.Input,
                            ExpectedStatusCode = dto.ExpectedStatusCode,
                            ExpectedResponse = dto.ExpectedResponse,
                            CustomUrl = dto.CustomUrl,
                            CreatedAt = DateTime.UtcNow,
                            TestRunId = testRun.Id
                        }).ToList();
                }
                foreach (var tc in generated)
                {
                    await _testCaseRepository.AddAsync(tc);
                }
                testCases = generated;
            }
            else
            {
                // If test cases already exist, update their TestRunId
                foreach (var tc in testCases)
                {
                    tc.TestRunId = testRun.Id;
                    await _testCaseRepository.UpdateAsync(tc);
                }
            }

            var resultDtos = new List<TestResultDto>();

            Dictionary<string, string> headers = new();
            if (!string.IsNullOrWhiteSpace(endpoint.HeadersJson))
            {
                headers = JsonSerializer.Deserialize<Dictionary<string, string>>(endpoint.HeadersJson!) ?? new();
            }

            using var httpClient = new HttpClient();

            foreach (var test in testCases ?? [])
            {
                var method = new HttpMethod(endpoint.HttpMethod.ToUpper());
                var urlToCall = !string.IsNullOrEmpty(test.CustomUrl) ? test.CustomUrl : endpoint.Url;
                var request = new HttpRequestMessage(method, urlToCall);

                if (!method.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
                {
                    request.Content = new StringContent(test.Input, Encoding.UTF8, "application/json");
                }

                foreach (var header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                HttpResponseMessage response = null; // Initialize the variable to avoid CS0165
                try
                {
                    response = await httpClient.SendAsync(request);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send request to {Url}", endpoint.Url);
                    continue; // Skip to the next test case if an exception occurs
                }

                var endTime = DateTime.UtcNow;

                var responseBody = await response.Content.ReadAsStringAsync();

                var passed = test.ExpectedStatusCode?.Contains((int)response.StatusCode) == true;

                var testResult = new TestResult
                {
                    Id = Guid.NewGuid(),
                    TestRunId = testRun.Id,
                    Outcome = passed ? "Pass" : "Fail",
                    StartedAt = startTime,
                    FinishedAt = endTime,
                    Duration = endTime - startTime,
                    CalledUrl = urlToCall,
                    Details = JsonSerializer.Serialize(new
                    {
                        TestCaseType = test.Type,
                        Input = test.Input,
                        ExpectedStatusCode = test.ExpectedStatusCode,
                        ActualStatusCode = (int)response.StatusCode,
                        ResponseBody = responseBody
                    })

                };

                await _testResultRepository.AddAsync(testResult);

                resultDtos.Add(new TestResultDto
                {
                    Id = testResult.Id,
                    TestCaseType = test.Type,
                    Input = test.Input,
                    ExpectedStatusCode = test.ExpectedStatusCode,
                    ActualStatusCode = (int)response.StatusCode,
                    Passed = passed,
                    ResponseBody = responseBody,
                    Duration = testResult.Duration,
                    CalledUrl = urlToCall
                });
            }

            testRun.FinishedAt = DateTime.UtcNow;
            testRun.Duration = testRun.FinishedAt - testRun.StartedAt;
            await _testRunRepository.UpdateAsync(testRun);

            try
            {
                if (resultDtos.Count <= 0) throw new Exception("Result DTOs is empty.");

                return new RunTestResponse
                {
                    TestRunId = testRun.Id,
                    Results = resultDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to run functional tests for endpoint {EndpointId}", endpointId);
                throw new InvalidOperationException("Failed to run functional tests.", ex);
            }
        }

        public async Task<List<TestCaseDto>> GenerateFunctionalTestsAsync(Guid endpointId, Guid userId)
        {
            var endpoint = await _endpointRepository.GetByIdAsync(endpointId, userId);
            if (endpoint == null)
            {
                _logger.LogWarning("Endpoint {EndpointId} not found for user {UserId}", endpointId, userId);
                throw new ArgumentException("Endpoint not found");
            }

            var testCases = new List<TestCase>();

            if (endpoint.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                testCases.Add(new TestCase
                {
                    Id = Guid.NewGuid(),
                    EndpointId = endpointId,
                    Type = "Functional",
                    Input = string.Empty,
                    ExpectedResponse = endpoint.ResponseBodyModel,
                    ExpectedStatusCode = ExpectedStatusCodeProvider.GetExpectedStatusCodes("Functional", endpoint.HttpMethod, "Success"),
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                var model = JsonDocument.Parse(endpoint.RequestBodyModel).RootElement;
                var validInput = TestDataFaker.GenerateValidInput(model);

                testCases.Add(new TestCase
                {
                    Id = Guid.NewGuid(),
                    EndpointId = endpointId,
                    Type = "Functional",
                    Input = JsonSerializer.Serialize(validInput),
                    ExpectedResponse = endpoint.ResponseBodyModel,
                    ExpectedStatusCode = ExpectedStatusCodeProvider.GetExpectedStatusCodes("Functional", endpoint.HttpMethod, "Success"),
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Save all test cases to DB
            foreach (var testCase in testCases)
            {
                await _testCaseRepository.AddAsync(testCase);
            }

            // Map to DTOs - INCLUDE CustomUrl here!
            var dtos = testCases.Select(tc => new TestCaseDto
            {
                Type = tc.Type,
                Input = tc.Input,
                ExpectedStatusCode = tc.ExpectedStatusCode,
                ExpectedResponse = tc.ExpectedResponse,
                CustomUrl = tc.CustomUrl // ← ADD THIS LINE
            }).ToList();

            return dtos;
        }

        public async Task<List<TestCaseDto>> GenerateAIFunctionalTestsAsync(Guid endpointId, Guid userId)
        {
            var endpoint = await _endpointRepository.GetByIdAsync(endpointId, userId);
            if (endpoint == null)
            {
                _logger.LogWarning("Endpoint {EndpointId} not found for user {UserId}", endpointId, userId);
                throw new ArgumentException("Endpoint not found");
            }

            var prompt = AIResponseHelper.GenerateAIPrompt("functional", endpoint);
            var rawJson = await _aiClientService.GetPromptResponseAsync(prompt);
            var testCases = AIResponseHelper.ExtractTestCasesFromRaw(rawJson!, "Functional");

            // Save all test cases to DB
            foreach (var testCase in testCases)
            {
                testCase.Id = Guid.NewGuid();
                testCase.EndpointId = endpointId;
                testCase.CreatedAt = DateTime.UtcNow;
                await _testCaseRepository.AddAsync(testCase);
            }

            // Map to DTOs
            var dtos = testCases.Select(tc => new TestCaseDto
            {
                Type = tc.Type,
                Input = tc.Input,
                ExpectedStatusCode = tc.ExpectedStatusCode,
                ExpectedResponse = tc.ExpectedResponse,
                CustomUrl = tc.CustomUrl // Include CustomUrl in DTO
            }).ToList();

            return dtos;
        }

        public async Task<RunTestResponse> RunFunctionalTestsAsync(Guid endpointId, Guid userId, bool artificialInteligence)
        {
            var endpoint = await _endpointRepository.GetByIdAsync(endpointId, userId);
            if (endpoint == null)
            {
                _logger.LogWarning("Endpoint {EndpointId} not found for user {UserId}", endpointId, userId);
                throw new ArgumentException("Endpoint not found");
            }

            var testCases = await _testCaseRepository.GetByEndpointIdAndTestTypeAsync(endpointId, "Functional");

            var startTime = DateTime.UtcNow;
            var testRun = new TestRun
            {
                Id = Guid.NewGuid(),
                EndpointId = endpointId,
                UserId = userId,
                StartedAt = startTime,
                TestType = "Functional"
            };
            await _testRunRepository.AddAsync(testRun);

            if (testCases == null || testCases.Count == 0)
            {
                List<TestCase> generated;
                if (artificialInteligence)
                {
                    // Generate and save new test cases (as entities)
                    generated = (await GenerateAIFunctionalTestsAsync(endpointId, userId))
                        .Select(dto => new TestCase
                        {
                            Id = Guid.NewGuid(),
                            EndpointId = endpointId,
                            Type = dto.Type,
                            Input = dto.Input,
                            ExpectedStatusCode = dto.ExpectedStatusCode,
                            ExpectedResponse = dto.ExpectedResponse,
                            CustomUrl = dto.CustomUrl,
                            CreatedAt = DateTime.UtcNow,
                            TestRunId = testRun.Id
                        }).ToList();
                }
                else
                {
                    generated = (await GenerateFunctionalTestsAsync(endpointId, userId))
                        .Select(dto => new TestCase
                        {
                            Id = Guid.NewGuid(),
                            EndpointId = endpointId,
                            Type = dto.Type,
                            Input = dto.Input,
                            ExpectedStatusCode = dto.ExpectedStatusCode,
                            ExpectedResponse = dto.ExpectedResponse,
                            CustomUrl = dto.CustomUrl,
                            CreatedAt = DateTime.UtcNow,
                            TestRunId = testRun.Id
                        }).ToList();
                }
                foreach (var tc in generated)
                {
                    await _testCaseRepository.AddAsync(tc);
                }
                testCases = generated;
            }
            else
            {
                // If test cases already exist, update their TestRunId
                foreach (var tc in testCases)
                {
                    tc.TestRunId = testRun.Id;
                    await _testCaseRepository.UpdateAsync(tc);
                }
            }

            var resultDtos = new List<TestResultDto>();

            var headers = !string.IsNullOrWhiteSpace(endpoint.HeadersJson)
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(endpoint.HeadersJson!) ?? new()
                : new Dictionary<string, string>();

            using var httpClient = new HttpClient();

            foreach (var test in testCases ?? [])
            {
                var method = new HttpMethod(endpoint.HttpMethod.ToUpper());
                var urlToCall = !string.IsNullOrEmpty(test.CustomUrl) ? test.CustomUrl : endpoint.Url;
                var request = new HttpRequestMessage(method, urlToCall);

                if (!method.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
                {
                    request.Content = new StringContent(test.Input, Encoding.UTF8, "application/json");
                }

                foreach (var header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                HttpResponseMessage response = null; // Initialize the variable to avoid CS0165
                try
                {
                    response = await httpClient.SendAsync(request);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send request to {Url}", endpoint.Url);
                    continue; // Skip to the next test case if an exception occurs
                }
                var endTime = DateTime.UtcNow;

                var responseBody = await response.Content.ReadAsStringAsync();

                var passed = test.ExpectedStatusCode?.Contains((int)response.StatusCode) == true;

                var testResult = new TestResult
                {
                    Id = Guid.NewGuid(),
                    TestRunId = testRun.Id,
                    Outcome = passed ? "Pass" : "Fail",
                    StartedAt = startTime,
                    FinishedAt = endTime,
                    Duration = endTime - startTime,
                    CalledUrl = urlToCall,
                    Details = JsonSerializer.Serialize(new
                    {
                        TestCaseType = test.Type,
                        Input = test.Input,
                        ExpectedStatusCode = test.ExpectedStatusCode,
                        ActualStatusCode = (int)response.StatusCode,
                        ResponseBody = responseBody
                    })

                };

                try
                {
                    await _testResultRepository.AddAsync(testResult);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save test result for {TestCaseId}", test.Id);
                    continue; // Skip to the next test case if an exception occurs
                }

                resultDtos.Add(new TestResultDto
                {
                    Id = testResult.Id,
                    TestCaseType = test.Type,
                    Input = test.Input,
                    ExpectedStatusCode = test.ExpectedStatusCode,
                    ActualStatusCode = (int)response.StatusCode,
                    Passed = passed,
                    ResponseBody = responseBody,
                    Duration = testResult.Duration,
                    CalledUrl = urlToCall
                });
            }

            testRun.FinishedAt = DateTime.UtcNow;
            testRun.Duration = testRun.FinishedAt - testRun.StartedAt;
            await _testRunRepository.UpdateAsync(testRun);

            try
            {
                if (resultDtos.Count <= 0) throw new Exception("Result DTOs is empty.");

                return new RunTestResponse
                {
                    TestRunId = testRun.Id,
                    Results = resultDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to run functional tests for endpoint {EndpointId}", endpointId);
                throw new InvalidOperationException("Failed to run functional tests.", ex);
            }
        }

    }
}
