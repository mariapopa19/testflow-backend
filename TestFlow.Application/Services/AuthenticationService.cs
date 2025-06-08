using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using TestFlow.API.Models.Requests;
using TestFlow.Application.Interfaces.Repository;
using TestFlow.Application.Interfaces.Services;
using TestFlow.Application.Models.GoogleLogin;
using TestFlow.Application.Models.Requests;
using TestFlow.Domain.Entities;
using TestFlow.Infrastructure;

namespace TestFlow.Application.Services;
public class AuthenticationService : IAuthenticationService
{
    private readonly IAuthenticationRepository _authRepo;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(IAuthenticationRepository authenticationRepository, IConfiguration config, ILogger<AuthenticationService> logger)
    {
        _authRepo = authenticationRepository;
        _config = config;
        _logger = logger;
    }

    public async Task<(bool Success, string? Token, string? Error, string? Name, string? Email, string? Picture)> GoogleRegisterAsync(GoogleLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.AccessToken))
            return (false, null, "Access token is required.", null, null, null);

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", request.AccessToken);

        var response = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v3/userinfo");
        if (!response.IsSuccessStatusCode)
            return (false, null, "Invalid Google access token.", null, null, null);

        var userInfoJson = await response.Content.ReadAsStringAsync();
        var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(userInfoJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (userInfo == null || string.IsNullOrWhiteSpace(userInfo.Email))
            return (false, null, "Could not retrieve user info from Google.", null, null, null);

        if (await _authRepo.UserExistsAsync(userInfo.Email))
            return (false, null, "User already exists", null, null, null);

        if (string.IsNullOrWhiteSpace(userInfo.Name))
            return (false, null, "Could not retrieve user info from Google.", null, null, null);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = userInfo.Email,
            Name = userInfo.Name,
            Role = "User"
        };

        try
        {
            await _authRepo.AddUserAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user with Google");
            return (false, null, $"Error registering user: {ex.Message}", null, null, null);
        }

        var token = GenerateJwt(user);
        return (true, token, null, user.Name, user.Email, userInfo.Picture);
    }

    public async Task<(bool Success, string? Token, string? Error, string? Name, string? Email, string? Picture)> GoogleLoginAsync(GoogleLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.AccessToken))
            return (false, null, "Access token is required.", null, null, null);

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", request.AccessToken);

        var response = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v3/userinfo");
        if (!response.IsSuccessStatusCode)
            return (false, null, "Invalid Google access token.", null, null, null);

        var userInfoJson = await response.Content.ReadAsStringAsync();
        var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(userInfoJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (userInfo == null || string.IsNullOrWhiteSpace(userInfo.Email))
            return (false, null, "Could not retrieve user info from Google.", null, null, null);

        var user = await _authRepo.GetUserByEmailAsync(userInfo.Email);
        if (user == null)
            return (false, null, "User not found. Please register.", null, null, null);

        var token = GenerateJwt(user);
        return (true, token, null, user.Name, user.Email, userInfo.Picture);
    }

    public async Task<(bool Success, string? Token, string? Error, string? Name, string? Email, string? Picture)> RegisterAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return (false, null, "Email and password are required.", null, null, null);

        if (await _authRepo.UserExistsAsync(request.Email))
            return (false, null, "User already exists", null, null, null);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            Name = request.Name,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "User"
        };
        try
        {
            await _authRepo.AddUserAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user");
            return (false, null, $"Error registering user: {ex.Message}", null, null, null);
        }

        var token = GenerateJwt(user);
        return (true, token, null, user.Name, user.Email, user.PictureUrl);
    }

    public async Task<(bool Success, string? Token, string? Error, string? Name, string? Email, string? Picture)> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return (false, null, "Email and password are required.", null, null, null);

        var user = await _authRepo.GetUserByEmailAsync(request.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return (false, null, "Invalid email or password.", null, null, null);

        var token = GenerateJwt(user);
        return (true, token, null, user.Name, user.Email, user.PictureUrl);
    }

    public async Task<(bool Success, string? Token, string? Error, string? Name, string? Email)> TestLoginAsync(string email)
    {
        var user = await _authRepo.GetUserByEmailAsync(email);
        if (user == null)
            return (false, null, "User not found", null, null);

        var token = GenerateJwt(user);
        return (true, token, null, user.Name, user.Email);
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
}
