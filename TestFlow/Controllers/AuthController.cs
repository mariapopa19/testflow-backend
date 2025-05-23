﻿using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using TestFlow.API.Models.Requests;
using TestFlow.Application.Models.GoogleLogin;
using TestFlow.Application.Models.Requests;
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
        if (string.IsNullOrWhiteSpace(request.AccessToken))
            return BadRequest("Access token is required.");

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", request.AccessToken);

        var response = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v3/userinfo");
        if (!response.IsSuccessStatusCode)
            return Unauthorized("Invalid Google access token.");

        var userInfoJson = await response.Content.ReadAsStringAsync();
        var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(userInfoJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (userInfo == null || string.IsNullOrWhiteSpace(userInfo.Email))
            return Unauthorized("Could not retrieve user info from Google.");

        var exists = await _context.Users.AnyAsync(u => u.Email == userInfo.Email);
        if (exists)
            return BadRequest("User already exists");

        if (userInfo == null || string.IsNullOrWhiteSpace(userInfo.Name))
            return Unauthorized("Could not retrieve user info from Google.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = userInfo.Email,
            Name = userInfo.Name,
            Role = "User"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = GenerateJwt(user);
        return Ok(new { token, email = user.Email, name = user.Name, picture = userInfo.Picture });
    }

    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.AccessToken))
            return BadRequest("Access token is required.");

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", request.AccessToken);

        var response = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v3/userinfo");
        if (!response.IsSuccessStatusCode)
            return Unauthorized("Invalid Google access token.");

        var userInfoJson = await response.Content.ReadAsStringAsync();
        var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(userInfoJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (userInfo == null || string.IsNullOrWhiteSpace(userInfo.Email))
            return Unauthorized("Could not retrieve user info from Google.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userInfo.Email);
        if (user == null)
            return Unauthorized("User not found. Please register.");

        var token = GenerateJwt(user);
        return Ok(new { token, name = user.Name, email = user.Email, picture = userInfo.Picture });
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

    [HttpPost("test-login")]
    [ProducesResponseType(typeof(OkObjectResult), 200)]
    public async Task<IActionResult> TestLogin([FromQuery] string email = "testuser@testflow.com")
    {
        var exists = await _context.Users.AnyAsync(u => u.Email == email);
        if (exists)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return BadRequest("User not found");

            var token = GenerateJwt(user);

            return Ok(new { token, email = user.Email, name = user.Name });
        }
        return BadRequest();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Email and password are required.");
        var exists = await _context.Users.AnyAsync(u => u.Email == request.Email);
        if (exists)
            return BadRequest("User already exists");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            Name = request.Name,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "User"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        var token = GenerateJwt(user);
        return Ok(new { token, email = user.Email, name = user.Name, picture = user.PictureUrl });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Email and password are required.");
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized("Invalid email or password.");
        var token = GenerateJwt(user);
        return Ok(new { token, email = user.Email, name = user.Name, picture = user.PictureUrl });
    }
}
