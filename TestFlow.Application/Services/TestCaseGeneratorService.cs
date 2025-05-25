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
        private readonly HttpClient _httpClient;
        private readonly string _openAiApiKey;

        public TestCaseGeneratorService(
            IEndpointRepository endpointRepository,
            ITestRunRepository testRunRepository,
            ITestResultRepository testResultRepository,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _endpointRepository = endpointRepository;
            _testRunRepository = testRunRepository;
            _testResultRepository = testResultRepository;
            _httpClient = httpClientFactory.CreateClient(); 
            _openAiApiKey = configuration["HuggingFace:ApiKey"]!;
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

            var prompt = $"Generează 3 cazuri de test de validare pentru următorul model JSON: { endpoint.RequestBodyModel}. " +
                         $"Fiecare test trebuie să aibă `Input` (ca string JSON) și `ExpectedStatusCode`. Răspunde cu un array JSON.";

            var routerBody = new
            {
                model = "mistralai/Mistral-7B-Instruct-v0.3",
                stream = false,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post,
                "https://router.huggingface.co/hf-inference/models/mistralai/Mistral-7B-Instruct-v0.3/v1/chat/completions")
            {
                Content = new StringContent(JsonSerializer.Serialize(routerBody), Encoding.UTF8, "application/json")
            };


            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _openAiApiKey);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();


            var content = await response.Content.ReadAsStringAsync();
            var root = JsonDocument.Parse(content).RootElement;
            var rawJson = root.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

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
                // linia asta este ca sa merga endpointul de test de la reqres.in
                //request.Headers.TryAddWithoutValidation("x-api-key", "reqres-free-v1");

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

        public async Task<List<TestCase>> GenerateAIFuzzyTestsAsync(Guid endpointId, Guid userId)
        {
            var endpoint = await _endpointRepository.GetByIdAsync(endpointId, userId);
            if (endpoint == null) throw new ArgumentException("Endpoint not found");

            var prompt = $"Generează 3 cazuri de test fuzzy (cu date invalide sau neașteptate) pentru următorul model JSON:\n{endpoint.RequestBodyModel}";

            var request = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "system", content = "Ești un generator de cazuri de test pentru API-uri REST. Răspunzi cu un array JSON de teste." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.4
            };

            var requestContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _openAiApiKey);

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", requestContent);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var completion = JsonDocument.Parse(responseContent)
                .RootElement.GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            var testCases = JsonSerializer.Deserialize<List<TestCase>>(completion!);
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

            var prompt = $"Generează 3 cazuri de test funcționale pentru următorul model JSON:\n{endpoint.RequestBodyModel}";

            var request = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "system", content = "Ești un generator de cazuri de test pentru API-uri REST. Răspunzi cu un array JSON de teste." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.2
            };

            var requestContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _openAiApiKey);

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", requestContent);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var completion = JsonDocument.Parse(responseContent)
                .RootElement.GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            var testCases = JsonSerializer.Deserialize<List<TestCase>>(completion!);
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

    }
}
