using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestFlow.Application.Models.Responses;
using TestFlow.Domain.Entities;

namespace TestFlow.Application.Interfaces.Services;
public interface ITestRunService
{
    Task<List<TestRunDto>> GetByUserIdAsync(Guid userId);
    Task<TestRunDto> GetByIdAsync(Guid id, Guid userId);
    Task<List<TestRunDto>> GetByEndpointIdAsync(Guid endpointId);

}
