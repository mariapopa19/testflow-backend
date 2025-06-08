using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestFlow.Domain.Entities;

namespace TestFlow.Application.Interfaces.Repository;
public interface IAuthenticationRepository
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<bool> UserExistsAsync(string email);
    Task AddUserAsync(User user);
}
