using System.Text.Json;
using System.Text.RegularExpressions;
using TestFlow.Application.Models.Tests;
using TestFlow.Domain.Entities;

namespace TestFlow.Application.Utils
{
    public static class AIResponseHelper
    {
        public static List<TestCase> ExtractTestCasesFromRaw(string rawText)
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

                        cases.Add(new TestCase
                        {
                            Type = "Validation",
                            Input = Regex.Unescape(input ?? string.Empty),
                            ExpectedStatusCode = expectedCodes
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
                         "Each test case must be an object with \"Input\" (as a JSON string representing query parameters or path variables, or empty if none) and \"ExpectedStatusCode\" (as an array of numbers, e.g. [200, 404]). " +
                         "Return ONLY a valid JSON array, with no explanations, labels, or extra text. Example:\n" +
                         "[\n" +
                         "  {{ \"Input\": \"{{}}\", \"ExpectedStatusCode\": [200, 201] }},\n" +
                         "  {{ \"Input\": \"{{\\\"id\\\":\\\"nonexistent\\\"}}\", \"ExpectedStatusCode\": [404, 400] }}\n" +
                         "]",
                "DELETE" => $"Generate 3 {testType} test cases for the DELETE method at URL: {endpoint.Url}. " +
                            "Each test case must be an object with \"Input\" (as a JSON string representing path variables or query parameters, or empty if none) and \"ExpectedStatusCode\" (as an array of numbers, e.g. [200, 404]). " +
                            "Return ONLY a valid JSON array, with no explanations, labels, or extra text. Example:\n" +
                            "[\n" +
                            "  {{ \"Input\": \"{{}}\", \"ExpectedStatusCode\": [204, 201, 200] }},\n" +
                            "  {{ \"Input\": \"{{\\\"id\\\":\\\"invalid\\\"}}\", \"ExpectedStatusCode\": [404, 400] }}\n" +
                            "]",
                "POST" or "PUT" or "PATCH" => $"Generate 3 {testType} test cases for the {method} method and for the following JSON model: {endpoint.RequestBodyModel}. " +
                            "Each test case must be an object with \"Input\" (as a JSON string) and \"ExpectedStatusCode\" (as an array of numbers, e.g. [200, 201]). " +
                            "Return ONLY a valid JSON array, with no explanations, labels, or extra text. Example:\n" +
                            "[\n" +
                            "  {{ \"Input\": \"{{\\\"name\\\":\\\"morpheus\\\",\\\"job\\\":\\\"captain\\\"}}\", \"ExpectedStatusCode\": [400, 404] }},\n" +
                            "  {{ \"Input\": \"{{\\\"name\\\":\\\"morfeus\\\",\\\"job\\\":\\\"leader\\\"}}\", \"ExpectedStatusCode\": [200, 201] }}\n" +
                            "]",
                _ => $"Generate 3 {testType} test cases for the {method} method. " +
                     "Each test case must be an object with \"Input\" (as a JSON string) and \"ExpectedStatusCode\" (as an array of numbers). " +
                     "Return ONLY a valid JSON array."
            };

        }
    }
}
