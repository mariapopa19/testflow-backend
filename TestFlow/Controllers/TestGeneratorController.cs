using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestFlow.Application.Interfaces.Services;
using TestFlow.Application.Models.Tests;

namespace TestFlow.API.Controllers
{
    [ApiController]
    [Route("api/test-generator")]
    [Authorize]
    public class TestGeneratorController : ControllerBase
    {
        private readonly ITestCaseGeneratorService _testCaseGeneratorService;

        public TestGeneratorController(ITestCaseGeneratorService testCaseGeneratorService)
        {
            _testCaseGeneratorService = testCaseGeneratorService;
        }

        [HttpGet("validation/{endpointId}")]
        public async Task<ActionResult<List<TestCase>>> GetValidationTests(Guid endpointId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null) return Unauthorized();

                var tests = await _testCaseGeneratorService.GenerateValidationTestsAsync(endpointId, Guid.Parse(userId));
                return Ok(tests);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("validation/run/{endpointId}")]
        public async Task<ActionResult<List<TestResultDto>>> RunValidationTests(Guid endpointId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null) return Unauthorized();

                var userId = Guid.Parse(userIdClaim);
                var results = await _testCaseGeneratorService.RunValidationTestsAsync(endpointId, Guid.Parse(userIdClaim));
                return Ok(results);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("fuzzy/{endpointId}")]
        public async Task<ActionResult<List<TestCase>>> GetFuzzyTests(Guid endpointId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null) return Unauthorized();

                var tests = await _testCaseGeneratorService.GenerateFuzzyTestsAsync(endpointId, Guid.Parse(userId));
                return Ok(tests);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("fuzzy/run/{endpointId}")]
        public async Task<ActionResult<List<TestResultDto>>> RunFuzzyTests(Guid endpointId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null) return Unauthorized();

                var userId = Guid.Parse(userIdClaim);
                var results = await _testCaseGeneratorService.RunFuzzyTestsAsync(endpointId, userId);
                return Ok(results);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
