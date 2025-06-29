using System.Text.Json;
using System.Text.RegularExpressions;
using TestFlow.Application.Models.Tests;
using TestFlow.Domain.Entities;

namespace TestFlow.Application.Utils
{
    public static class AIResponseHelper
    {
        public static List<TestCase> ExtractTestCasesFromRaw(string rawText, string testType = "Validation")
        {
            var normalized = System.Net.WebUtility.HtmlDecode(rawText);
            var cases = new List<TestCase>();

            try
            {
                using var doc = JsonDocument.Parse(normalized);
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var element in doc.RootElement.EnumerateArray())
                    {
                        if (!element.TryGetProperty("Input", out var inputProp)) continue;
                        if (!element.TryGetProperty("ExpectedStatusCode", out var statusProp)) continue;

                        var input = inputProp.GetString();
                        var expectedCodes = new List<int>();

                        if (statusProp.ValueKind == JsonValueKind.Number)
                            expectedCodes.Add(statusProp.GetInt32());
                        else if (statusProp.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var code in statusProp.EnumerateArray())
                            {
                                if (code.ValueKind == JsonValueKind.Number)
                                    expectedCodes.Add(code.GetInt32());
                            }
                        }

                        // Extract CustomUrl if present (for GET methods with query params)
                        string? customUrl = null;
                        if (element.TryGetProperty("CustomUrl", out var customUrlProp))
                        {
                            customUrl = customUrlProp.GetString();
                        }

                        cases.Add(new TestCase
                        {
                            Type = testType,
                            Input = input == "null" ? string.Empty : Regex.Unescape(input ?? string.Empty),
                            ExpectedStatusCode = expectedCodes,
                            CustomUrl = customUrl
                        });
                    }
                }
            }
            catch
            {
                // Ignore unparsable cases
            }

            return cases;
        }

        public static string GenerateAIPrompt(string testType, Endpoint endpoint)
        {
            var method = endpoint.HttpMethod.ToUpperInvariant();

            return method switch
            {
                "GET" => $"Generate 3 {testType} test cases for the GET method at URL: {endpoint.Url}. " +
                         "For GET requests, DO NOT generate request body data. Instead, focus on URL variations and query parameters. " +
                         "Each test case must be an object with \"Input\" (always empty string for GET), \"ExpectedStatusCode\" (as an array of numbers), and optionally \"CustomUrl\" (for testing with different URL variations). " +
                         "Use CustomUrl to test different scenarios like adding query parameters, invalid paths, or malformed URLs. " +
                         "Return ONLY a valid JSON array, with no explanations, labels, or extra text. Example:\n" +
                         "[\n" +
                         "  {{ \"Input\": \"\", \"ExpectedStatusCode\": [200], \"CustomUrl\": null }},\n" +
                         "  {{ \"Input\": \"\", \"ExpectedStatusCode\": [400, 404], \"CustomUrl\": \"{endpoint.Url}?invalidParam=test\" }},\n" +
                         "  {{ \"Input\": \"\", \"ExpectedStatusCode\": [404], \"CustomUrl\": \"{endpoint.Url}/nonexistent\" }}\n" +
                         "]",

                "DELETE" => $"Generate 3 {testType} test cases for the DELETE method at URL: {endpoint.Url}. " +
                            "For DELETE requests, typically no request body is needed. Focus on URL variations and path parameters. " +
                            "Each test case must be an object with \"Input\" (empty string for most DELETE requests), \"ExpectedStatusCode\" (as an array of numbers), and optionally \"CustomUrl\" (for testing with different URL variations). " +
                            "Return ONLY a valid JSON array, with no explanations, labels, or extra text. Example:\n" +
                            "[\n" +
                            "  {{ \"Input\": \"\", \"ExpectedStatusCode\": [200, 204], \"CustomUrl\": null }},\n" +
                            "  {{ \"Input\": \"\", \"ExpectedStatusCode\": [404, 400], \"CustomUrl\": \"{endpoint.Url}/invalidid\" }}\n" +
                            "]",

                "POST" or "PUT" or "PATCH" => $"Generate 3 {testType} test cases for the {method} method using the following JSON model: {endpoint.RequestBodyModel}. " +
                            "Each test case must be an object with \"Input\" (as a JSON string representing the request body) and \"ExpectedStatusCode\" (as an array of numbers). " +
                            "Create variations like valid data, invalid data, missing fields, wrong data types, etc. " +
                            "Return ONLY a valid JSON array, with no explanations, labels, or extra text. Example:\n" +
                            "[\n" +
                            "  {{ \"Input\": \"{{\\\"name\\\":\\\"validName\\\",\\\"email\\\":\\\"test@example.com\\\"}}\", \"ExpectedStatusCode\": [200, 201] }},\n" +
                            "  {{ \"Input\": \"{{\\\"name\\\":\\\"\\\",\\\"email\\\":\\\"invalid-email\\\"}}\", \"ExpectedStatusCode\": [400, 422] }},\n" +
                            "  {{ \"Input\": \"{{\\\"name\\\":\\\"test\\\"}}\", \"ExpectedStatusCode\": [400] }}\n" +
                            "]",

                _ => $"Generate 3 {testType} test cases for the {method} method. " +
                     "Each test case must be an object with \"Input\" (as a JSON string) and \"ExpectedStatusCode\" (as an array of numbers). " +
                     "Return ONLY a valid JSON array."
            };
        }
    }
}
