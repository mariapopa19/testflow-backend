using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestFlow.Application.Models.Dashboard;
using TestFlow.Application.Models.Responses;

namespace TestFlow.Application.Interfaces.Services;
public interface IDashboardService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync(Guid userId);
    Task<List<TestRunsOverTimeDto>> GetTestRunsOverTimeAsync(Guid userId, int days);
    Task<PassFailDistributionDto> GetPassFailDistributionAsync(Guid userId);
    Task<List<TestReportDto>> GetRecentTestRunsAsync(Guid userId, int limit);
}
