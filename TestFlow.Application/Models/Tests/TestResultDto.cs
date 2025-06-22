using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestFlow.Application.Models.Tests
{
    public class TestResultDto
    {
        public Guid Id { get; set; }
        public string TestCaseType { get; set; } = null!;
        public string Input { get; set; } = null!;
        public string? CalledUrl { get; set; }
        public List<int>? ExpectedStatusCode { get; set; } = new List<int>();
        public int ActualStatusCode { get; set; }
        public bool Passed { get; set; }
        public string? ResponseBody { get; set; }
        public TimeSpan? Duration { get; set; }
    }
}
