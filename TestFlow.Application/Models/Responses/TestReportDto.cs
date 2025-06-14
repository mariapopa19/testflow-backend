using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestFlow.Application.Models.Tests;

namespace TestFlow.Application.Models.Responses;
public class TestReportDto
{
    public Guid Id { get; set; }
    public Guid TestRunId { get; set; }
    public string TestType { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public int TotalTests { get; set; }
    public int PassedTests { get; set; }
    public int FailedTests { get; set; }
    public List<TestResultDto> Results { get; set; } = new();
}
