using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestFlow.Application.Models.Responses;
using TestFlow.Domain.Entities;

namespace TestFlow.Application.Interfaces.Services;
public interface ITestReportService
{
    Task<TestReportDto> GenerateReportFromTestRunAsync(Guid testRunId, Guid userId);
    Task<List<TestReportDto>> GetAllReportsAsync(Guid userId);
    Task<TestReportDto?> GetReportByIdAsync(Guid id);
    Task<bool> DeleteAsync(Guid id, Guid userId);
    Task<List<TestReportDto>> GetRecentReportsAsync(Guid userId, int limit);

}
