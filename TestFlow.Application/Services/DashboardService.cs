using AutoMapper;
using TestFlow.Application.Interfaces.Repository;
using TestFlow.Application.Interfaces.Services;
using TestFlow.Application.Models.Dashboard;
using TestFlow.Application.Models.Responses;

namespace TestFlow.Application.Services;
public class DashboardService : IDashboardService
{
    private readonly IEndpointRepository _endpointRepo;
    private readonly ITestRunRepository _testRunRepo;
    private readonly ITestResultRepository _testResultRepo;
    private readonly ITestReportRepository _testReportRepo;
    private readonly IMapper _mapper;

    public DashboardService(
        IEndpointRepository endpointRepo,
        ITestRunRepository testRunRepo,
        ITestResultRepository testResultRepo,
        ITestReportRepository testReportRepo,
        IMapper mapper)
    {
        _endpointRepo = endpointRepo;
        _testRunRepo = testRunRepo;
        _testResultRepo = testResultRepo;
        _testReportRepo = testReportRepo;
        _mapper = mapper;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(Guid userId)
    {
        var totalEndpoints = await _endpointRepo.CountByUserAsync(userId);
        var totalTestRuns = await _testRunRepo.CountByUserAsync(userId);
        var totalResults = await _testResultRepo.GetByUserIdAsync(userId);

        int passed = totalResults.Count(r => r.Outcome == "Pass");
        int failed = totalResults.Count(r => r.Outcome == "Fail");
        int total = passed + failed;

        double passedPct = total > 0 ? (double)passed / total * 100 : 0;
        double failedPct = total > 0 ? (double)failed / total * 100 : 0;

        return new DashboardStatsDto
        {
            TotalEndpoints = totalEndpoints,
            TotalTestRuns = totalTestRuns,
            PassedTestsPercentage = Math.Round(passedPct, 1),
            FailedTestsPercentage = Math.Round(failedPct, 1)
        };
    }

    public async Task<List<TestRunsOverTimeDto>> GetTestRunsOverTimeAsync(Guid userId, int days)
    {
        var since = DateTime.UtcNow.Date.AddDays(-days + 1);
        var runs = await _testRunRepo.GetByUserIdSinceAsync(userId, since);

        return runs
            .GroupBy(r => r.StartedAt.Date)
            .Select(g => new TestRunsOverTimeDto
            {
                Date = DateOnly.FromDateTime(g.Key),
                Count = g.Count()
            })
            .OrderBy(x => x.Date)
            .ToList();
    }

    public async Task<PassFailDistributionDto> GetPassFailDistributionAsync(Guid userId)
    {
        var results = await _testResultRepo.GetByUserIdAsync(userId);
        return new PassFailDistributionDto
        {
            Passed = results.Count(r => r.Outcome == "Pass"),
            Failed = results.Count(r => r.Outcome == "Fail")
        };
    }

    public async Task<List<TestReportDto>> GetRecentTestRunsAsync(Guid userId, int limit)
    {
        var reports = await _testReportRepo.GetRecentByUserAsync(userId, limit);
        return _mapper.Map<List<TestReportDto>>(reports);
    }
}
