using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TestFlow.API.Models.Requests;
using TestFlow.Application.Models.GoogleLogin;
using TestFlow.Application.Models.Requests;
using TestFlow.Domain.Entities;
using TestFlow.Infrastructure;
using IAuthenticationService = TestFlow.Application.Interfaces.Services.IAuthenticationService;

namespace TestFlow.API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _config;
    private readonly IAuthenticationService _authService;

    public AuthController(ApplicationDbContext context, IConfiguration config, IAuthenticationService authenticationService)
    {
        _context = context;
        _config = config;
        _authService = authenticationService;
    }

    [HttpPost("google-register")]
    public async Task<IActionResult> GoogleRegister([FromBody] GoogleLoginRequest request)
    {
        var result = await _authService.GoogleRegisterAsync(request);
        if (!result.Success)
            return BadRequest(result.Error);
        return Ok(new { token = result.Token, email = result.Email, name = result.Name, picture = result.Picture });
    }

    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        var result = await _authService.GoogleLoginAsync(request);
        if (!result.Success)
            return Unauthorized(result.Error);
        return Ok(new { token = result.Token, name = result.Name, email = result.Email, picture = result.Picture });
    }

    [HttpPost("test-login")]
    [ProducesResponseType(typeof(OkObjectResult), 200)]
    public async Task<IActionResult> TestLogin([FromQuery] string email = "testuser@testflow.com")
    {
        var result = await _authService.TestLoginAsync(email);
        if (!result.Success)
            return BadRequest(result.Error);
        return Ok(new { token = result.Token, email = result.Email, name = result.Name });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        if (!result.Success)
            return BadRequest(result.Error);
        return Ok(new { token = result.Token, email = result.Email, name = result.Name, picture = result.Picture });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (!result.Success)
            return Unauthorized(result.Error);
        return Ok(new { token = result.Token, email = result.Email, name = result.Name, picture = result.Picture });
    }
}
