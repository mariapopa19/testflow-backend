using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TestFlow.Application.Interfaces.Services;
using TestFlow.Application.Models.Dashboard;
using TestFlow.Application.Models.Responses;

namespace TestFlow.API.Controllers;


[Authorize]
[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsDto>> GetStats()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();
        var stats = await _dashboardService.GetDashboardStatsAsync(Guid.Parse(userId));
        return Ok(stats);
    }

    [HttpGet("test-runs-over-time")]
    public async Task<ActionResult<List<TestRunsOverTimeDto>>> GetTestRunsOverTime([FromQuery] int days = 7)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();
        var data = await _dashboardService.GetTestRunsOverTimeAsync(Guid.Parse(userId), days);
        return Ok(data);
    }

    [HttpGet("pass-fail-distribution")]
    public async Task<ActionResult<PassFailDistributionDto>> GetPassFailDistribution()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();
        var data = await _dashboardService.GetPassFailDistributionAsync(Guid.Parse(userId));
        return Ok(data);
    }

    [HttpGet("recent-test-runs")]
    public async Task<ActionResult<List<TestReportDto>>> GetRecentTestRuns([FromQuery] int limit = 5)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();
        var data = await _dashboardService.GetRecentTestRunsAsync(Guid.Parse(userId), limit);
        return Ok(data);
    }
}
