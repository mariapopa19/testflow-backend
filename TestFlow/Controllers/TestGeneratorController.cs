using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestFlow.Application.Interfaces.Services;
using TestFlow.Application.Models.Requests;
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

        [HttpGet("validation/ai/{endpointId}")]
        public async Task<ActionResult<List<TestCase>>> GetValidationTestsWithAI(Guid endpointId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null) return Unauthorized();
                var tests = await _testCaseGeneratorService.GenerateValidationTestsWithAIAsync(endpointId, Guid.Parse(userId));
                return Ok(tests);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }


        [HttpPost("validation/run/{endpointId}")]
        public async Task<ActionResult<List<TestResultDto>>> RunValidationTests(RunTestsRequest runTestsRequest)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null) return Unauthorized();

                var userId = Guid.Parse(userIdClaim);
                var results = await _testCaseGeneratorService.RunValidationTestsAsync(runTestsRequest.EndpointId, Guid.Parse(userIdClaim), runTestsRequest.ArtificialIntelligence);
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

        [HttpGet("fuzzy/ai/{endpointId}")]
        public async Task<ActionResult<List<TestCase>>> GetFuzzyTestsWithAI(Guid endpointId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null) return Unauthorized();
                var tests = await _testCaseGeneratorService.GenerateAIFuzzyTestsAsync(endpointId, Guid.Parse(userId));
                return Ok(tests);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("fuzzy/run/{endpointId}")]
        public async Task<ActionResult<List<TestResultDto>>> RunFuzzyTests(RunTestsRequest runTestsRequest)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null) return Unauthorized();

                var userId = Guid.Parse(userIdClaim);
                var results = await _testCaseGeneratorService.RunFuzzyTestsAsync(runTestsRequest.EndpointId, userId, runTestsRequest.ArtificialIntelligence);
                return Ok(results);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("functional/{endpointId}")]
        public async Task<ActionResult<List<TestCase>>> GetFunctionalTests(Guid endpointId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null) return Unauthorized();
                var tests = await _testCaseGeneratorService.GenerateFunctionalTestsAsync(endpointId, Guid.Parse(userId));
                return Ok(tests);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("functional/ai/{endpointId}")]
        public async Task<ActionResult<List<TestCase>>> GetFunctionalTestsWithAI(Guid endpointId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null) return Unauthorized();
                var tests = await _testCaseGeneratorService.GenerateAIFunctionalTestsAsync(endpointId, Guid.Parse(userId));
                return Ok(tests);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("functional/run/{endpointId}")]
        public async Task<ActionResult<List<TestResultDto>>> RunFunctionalTests(RunTestsRequest runTestsRequest)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null) return Unauthorized();
                var userId = Guid.Parse(userIdClaim);
                var results = await _testCaseGeneratorService.RunFunctionalTestsAsync(runTestsRequest.EndpointId, userId, runTestsRequest.ArtificialIntelligence);
                return Ok(results);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
