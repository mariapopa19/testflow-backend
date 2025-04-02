using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TestFlow.API.Models.Requests;
using TestFlow.Domain.Entities;
using TestFlow.Infrastructure;

namespace TestFlow.API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(ApplicationDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    [HttpPost("google-register")]
    public async Task<IActionResult> GoogleRegister([FromBody] GoogleLoginRequest request)
    {
        var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken);
        var email = payload.Email;

        var exists = await _context.Users.AnyAsync(u => u.Email == email);
        if (exists)
            return BadRequest("User already exists");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Name = payload.Name,
            Role = "User"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = GenerateJwt(user);
        return Ok(new { token, email = user.Email, name = user.Name });
    }

    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);

        if (user == null)
            return Unauthorized("User not found. Please register.");

        var token = GenerateJwt(user);
        return Ok(new { token, name = user.Name, email = user.Email });
    }

    private string GenerateJwt(User user)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role ?? "User")
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryMinutes"])),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult GetMe()
    {
        var name = User.Identity?.Name;
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        return Ok(new { name, email, userId });
    }

    [HttpPost("test-login")]
    [ProducesResponseType(typeof(OkObjectResult), 200)]
    public IActionResult TestLogin([FromQuery] string email = "testuser@testflow.com", [FromQuery] string name = "Test User")
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Name = name,
            Role = "User"
        };

        var token = GenerateJwt(user);
        return Ok(new { token, email = user.Email, name = user.Name });
    }
}
