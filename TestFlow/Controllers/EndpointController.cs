using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TestFlow.Application.Interfaces;
using TestFlow.Application.Models.Requests;


namespace TestFlow.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EndpointController : ControllerBase
{
    private readonly IEndpointIService _endpointService;

    public EndpointController(IEndpointIService endpointService)
    {
        _endpointService = endpointService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEndpointRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        var result = await _endpointService.CreateAsync(request, Guid.Parse(userId));
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        var endpoints = await _endpointService.GetUserEndpointsAsync(Guid.Parse(userId));
        return Ok(endpoints);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        var success = await _endpointService.DeleteAsync(id, Guid.Parse(userId));
        if (!success) return NotFound();

        return NoContent();
    }
}
