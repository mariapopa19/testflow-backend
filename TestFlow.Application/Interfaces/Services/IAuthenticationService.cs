using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestFlow.API.Models.Requests;
using TestFlow.Application.Models.Requests;

namespace TestFlow.Application.Interfaces.Services;
public interface IAuthenticationService
{
    Task<(bool Success, string? Token, string? Error, string? Name, string? Email, string? Picture)> GoogleRegisterAsync(GoogleLoginRequest request);
    Task<(bool Success, string? Token, string? Error, string? Name, string? Email, string? Picture)> GoogleLoginAsync(GoogleLoginRequest request);
    Task<(bool Success, string? Token, string? Error, string? Name, string? Email, string? Picture)> RegisterAsync(RegisterRequest request);
    Task<(bool Success, string? Token, string? Error, string? Name, string? Email, string? Picture)> LoginAsync(LoginRequest request);
    Task<(bool Success, string? Token, string? Error, string? Name, string? Email)> TestLoginAsync(string email);
}
