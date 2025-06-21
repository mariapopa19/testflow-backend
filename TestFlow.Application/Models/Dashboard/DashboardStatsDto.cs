using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestFlow.Application.Models.Dashboard;
public class DashboardStatsDto
{
    public int TotalEndpoints { get; set; }
    public int TotalTestRuns { get; set; }
    public double PassedTestsPercentage { get; set; }
    public double FailedTestsPercentage { get; set; }
}
