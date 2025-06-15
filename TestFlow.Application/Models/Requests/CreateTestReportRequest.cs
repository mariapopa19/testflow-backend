using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestFlow.Application.Models.Tests;

namespace TestFlow.Application.Models.Requests;
public class CreateTestReportRequest
{
    public Guid TestRunId { get; set; }
    public string TestType { get; set; } = null!;
}
