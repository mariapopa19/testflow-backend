using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TestFlow.Application.Interfaces.Repository;
using TestFlow.Application.Interfaces.Services;
using TestFlow.Application.Models.Responses;
using TestFlow.Application.Models.Tests;
using TestFlow.Domain.Entities;

namespace TestFlow.Application.Services;
public class TestReportService : ITestReportService
{
    private readonly ITestReportRepository _reportRepository;
    private readonly ITestRunRepository _testRunRepository;
    private readonly ITestResultRepository _testResultRepository;
    private readonly ILogger<TestReportService> _logger;

    public TestReportService(
        ITestReportRepository reportRepository,
        ITestRunRepository testRunRepository,
        ITestResultRepository testResultRepository,
        ILogger<TestReportService> logger)
    {
        _reportRepository = reportRepository;
        _testRunRepository = testRunRepository;
        _testResultRepository = testResultRepository;
        _logger = logger;
    }

    public async Task<TestReport> GenerateReportFromTestRunAsync(Guid testRunId, Guid userId)
    {
        var testRun = await _testRunRepository.GetByIdAsync(testRunId, userId);
        if (testRun == null)
            throw new ArgumentException("Test run not found");

        var results = await _testResultRepository.GetByTestRunIdAsync(testRunId);

        var report = new TestReport
        {
            Id = Guid.NewGuid(),
            TestRunId = testRunId,
            UserId = testRun.UserId,
            TestType = testRun.TestType,
            CreatedAt = DateTime.UtcNow,
            TotalTests = results.Count,
            PassedTests = results.Count(r => r.Outcome == "Pass"),
            FailedTests = results.Count(r => r.Outcome == "Fail"),
            Results = results
        };

        return await _reportRepository.CreateAsync(report);
    }

    public async Task<List<TestReportDto>> GetAllReportsAsync(Guid userId)
    {
        var reports = await _reportRepository.GetAllAsync(userId);
        return reports.Select(r => new TestReportDto
        {
            Id = r.Id,
            TestRunId = r.TestRunId,
            TestType = r.TestType,
            CreatedAt = r.CreatedAt,
            TotalTests = r.TotalTests,
            PassedTests = r.PassedTests,
            FailedTests = r.FailedTests,
            Results = r.Results.Select(result => new TestResultDto
            {
                TestCaseType = result.TestCase?.Type ?? "Unknown",
                Input = result.TestCase?.Input ?? string.Empty,
                ExpectedStatusCode = result.TestCase?.ExpectedStatusCode,
                ActualStatusCode = int.Parse(JsonDocument.Parse(result.Details).RootElement.GetProperty("ActualStatusCode").GetString() ?? "0"),
                Passed = result.Outcome == "Pass",
                ResponseBody = JsonDocument.Parse(result.Details).RootElement.GetProperty("ResponseBody").GetString()
            }).ToList()
        }).ToList();
    }

    public async Task<TestReportDto?> GetReportByIdAsync(Guid id)
    {
        var report = await _reportRepository.GetByIdAsync(id);
        if (report == null) return null;

        return new TestReportDto
        {
            Id = report.Id,
            TestRunId = report.TestRunId,
            TestType = report.TestType,
            CreatedAt = report.CreatedAt,
            TotalTests = report.TotalTests,
            PassedTests = report.PassedTests,
            FailedTests = report.FailedTests,
            Results = report.Results.Select(result => new TestResultDto
            {
                TestCaseType = result.TestCase?.Type ?? "Unknown",
                Input = result.TestCase?.Input ?? string.Empty,
                ExpectedStatusCode = result.TestCase?.ExpectedStatusCode,
                ActualStatusCode = int.Parse(JsonDocument.Parse(result.Details).RootElement.GetProperty("ActualStatusCode").GetString() ?? "0"),
                Passed = result.Outcome == "Pass",
                ResponseBody = JsonDocument.Parse(result.Details).RootElement.GetProperty("ResponseBody").GetString()
            }).ToList()
        };
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var report = await _reportRepository.GetByIdAsync(id);
        if (report == null)
            return false;

        await _reportRepository.DeleteAsync(id);
        return true;
    }
}
