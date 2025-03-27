using TestFlow.Domain.Entities;

namespace TestFlow.Application.Intefaces;
public interface IEndpointRepository : IRepository<Endpoint>
{
    Task<Endpoint?> GetByUrlAsync(string url);
}
