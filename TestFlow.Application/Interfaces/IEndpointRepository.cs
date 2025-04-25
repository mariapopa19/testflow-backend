using TestFlow.Domain.Entities;

namespace TestFlow.Application.Interfaces;
public interface IEndpointRepository 
{
    Task<List<Endpoint>> GetByUserIdAsync(Guid userId);
    Task<Endpoint?> GetByIdAsync(Guid id, Guid userId);
    Task AddAsync(Endpoint endpoint);
    Task DeleteAsync(Endpoint endpoint);
}