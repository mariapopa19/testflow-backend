using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestFlow.Application.Models.Tests
{
    public class TestResultDto
    {
        public string TestCaseType { get; set; } = null!;
        public string Input { get; set; } = null!;
        public List<int>? ExpectedStatusCode { get; set; }
        public int ActualStatusCode { get; set; }
        public bool Passed { get; set; }
        public string? ResponseBody { get; set; }
    }
}
