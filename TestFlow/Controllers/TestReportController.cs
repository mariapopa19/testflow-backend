using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TestFlow.Application.Interfaces.Services;
using TestFlow.Application.Models.Requests;
using TestFlow.Application.Models.Responses;
using TestFlow.Domain.Entities;

[Authorize]
[ApiController]
[Route("api/test-reports")]
public class TestReportController : ControllerBase
{
    private readonly ITestReportService _testReportService;

    public TestReportController(ITestReportService testReportService)
    {
        _testReportService = testReportService;
    }

    [HttpGet]
    public async Task<ActionResult<List<TestReportDto>>> GetAll()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        var reports = await _testReportService.GetAllReportsAsync(Guid.Parse(userId));
        return Ok(reports);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TestReportDto>> GetById(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        var report = await _testReportService.GetReportByIdAsync(id);
        if (report == null) return NotFound();

        return Ok(report);
    }

    [HttpPost]
    public async Task<ActionResult<TestReportDto>> Create([FromBody] CreateTestReportRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        try
        {
            var report = await _testReportService.GenerateReportFromTestRunAsync(request.TestRunId, Guid.Parse(userId));
            return CreatedAtAction(nameof(GetById), new { id = report.Id }, report);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        var success = await _testReportService.DeleteAsync(id, Guid.Parse(userId));
        if (!success) return NotFound();

        return NoContent();
    }

    [HttpGet("recent")]
    public async Task<ActionResult<List<TestReportDto>>> GetRecent([FromQuery] int limit = 5)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        var reports = await _testReportService.GetRecentReportsAsync(Guid.Parse(userId), limit);
        return Ok(reports);
    }
}
