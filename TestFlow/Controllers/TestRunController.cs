using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestFlow.Application.Interfaces.Repository;
using TestFlow.Application.Interfaces.Services;
using TestFlow.Application.Models.Responses;
using TestFlow.Domain.Entities;

namespace TestFlow.API.Controllers;
[ApiController]
[Route("api/test-run")]
[Authorize]
public class TestRunController : ControllerBase
{
    private readonly ITestRunService _testRunService;

    public TestRunController(ITestRunService testRunService)
    {
        _testRunService = testRunService;
    }

    [HttpGet("user")]
    public async Task<ActionResult<List<TestRun>>> GetByUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        var testRuns = await _testRunService.GetByUserIdAsync(Guid.Parse(userId));
        if (testRuns is null || !testRuns.Any())
        {
            return NotFound(new { message = "No test runs found for the user." });
        }
        return Ok(testRuns);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TestRun>> GetById(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        var testRun = await _testRunService.GetByIdAsync(id, Guid.Parse(userId));
        if (testRun is null)
        {
            return NotFound(new { message = "Test run not found." });
        }
        return Ok(testRun);
    }

    [HttpGet("endpoint/{endpointId}")]
    public async Task<ActionResult<List<TestRun>>> GetByEndpointId(Guid endpointId)
    {
        var testRuns = await _testRunService.GetByEndpointIdAsync(endpointId);
        if (testRuns is null || !testRuns.Any())
        {
            return NotFound(new { message = "No test runs found for the specified endpoint." });
        }
        return Ok(testRuns);
    }
}
