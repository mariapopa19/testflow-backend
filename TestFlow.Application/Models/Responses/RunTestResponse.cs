using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestFlow.Application.Models.Tests;

namespace TestFlow.Application.Models.Responses;
public class RunTestResponse
{
    public Guid TestRunId { get; set; }
    public List<TestResultDto> Results { get; set; } = new();
}
