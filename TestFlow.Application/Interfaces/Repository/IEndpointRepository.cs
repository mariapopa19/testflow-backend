using TestFlow.Application.Models.Requests;
using TestFlow.Domain.Entities;

namespace TestFlow.Application.Interfaces.Repository;
public interface IEndpointRepository 
{
    Task<List<Endpoint>> GetByUserIdAsync(Guid userId);
    Task<Endpoint?> GetByIdAsync(Guid id, Guid userId);
    Task<int> CountByUserAsync(Guid userId);
    Task AddAsync(Endpoint endpoint);
    Task DeleteAsync(Endpoint endpoint);
    Task<bool> UpdateEndpointAsync(Guid id, UpdateEndpointRequest request);
}