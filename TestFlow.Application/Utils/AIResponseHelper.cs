using System.Text.Json;
using System.Text.RegularExpressions;
using TestFlow.Application.Models.Tests;

namespace TestFlow.Application.Utils
{
    public static class AIResponseHelper
    {
        public static List<TestCase> ExtractTestCasesFromRaw(string rawText)
        {
            var normalized = System.Net.WebUtility.HtmlDecode(rawText);

            // Regex pentru a extrage doar obiectele JSON
            var regex = new Regex(@"\{[^}]+\}", RegexOptions.Multiline);
            var matches = regex.Matches(normalized);

            var cases = new List<TestCase>();

            foreach (Match match in matches)
            {
                try
                {
                    var obj = JsonSerializer.Deserialize<TestCase>(match.Value);
                    if (obj != null)
                        cases.Add(obj);
                }
                catch
                {
                    // ignori obiectele care nu se parsează
                }
            }

            return cases;
        }
    }
}
