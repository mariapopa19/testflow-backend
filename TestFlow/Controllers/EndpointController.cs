using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestFlow.Application.Interfaces.Services;
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

    [HttpGet("all-endpoints")]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        var endpoints = await _endpointService.GetUserEndpointsAsync(Guid.Parse(userId));
        return Ok(endpoints);
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        var endpoint = await _endpointService.GetUserEndpointByIdAsync(id, Guid.Parse(userId));
        if (endpoint == null) return NotFound();

        return Ok(endpoint);
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

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateEndpoint(Guid id, [FromBody] UpdateEndpointRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        var updated = await _endpointService.UpdateEndpointAsync(id, Guid.Parse(userId), request);

        if (!updated)
        {
            return NotFound("Endpoint not found.");
        }

        return Ok("Endpoint updated successfully.");
    }
}
