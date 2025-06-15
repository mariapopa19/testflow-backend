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

    public async Task<TestReportDto> GenerateReportFromTestRunAsync(Guid testRunId, Guid userId)
    {
        _logger.LogInformation("Generating report for test run {TestRunId} for user {UserId}", testRunId, userId);

        var testRun = await _testRunRepository.GetByIdAsync(testRunId, userId);
        if (testRun == null)
        {
            _logger.LogWarning("Test run {TestRunId} not found for user {UserId}", testRunId, userId);
            throw new ArgumentException("Test run not found");
        }

        var results = await _testResultRepository.GetByTestRunIdAsync(testRunId);
        if (results == null || results.Count == 0)
        {
            _logger.LogWarning("No test results found for test run {TestRunId}", testRunId);
            throw new ArgumentException("No test results found for this test run");
        }

        _logger.LogInformation("Creating test report for {ResultCount} results from test run {TestRunId}", results.Count, testRunId);


        var report = new TestReport
        {
            Id = Guid.NewGuid(),
            TestRunId = testRunId,
            UserId = testRun.UserId,
            TestType = testRun.TestType,
            CreatedAt = DateTime.UtcNow,
            TotalTests = results.Count,
            PassedTests = results.Count(r => r.Outcome == "Pass"),
            FailedTests = results.Count(r => r.Outcome == "Fail")
        };

        try
        {
            // 1. Save the report first so its Id exists in the DB
            await _reportRepository.CreateAsync(report);

            // 2. Now update the test results to reference the new report
            foreach (var result in results)
            {
                result.ReportId = report.Id;
                await _testResultRepository.UpdateAsync(result);
            }

            // ... (rest of your logic)
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating test report for test run {TestRunId}", testRunId);
            throw;
        }


        // Create and return the DTO directly
        return new TestReportDto
        {
            Id = report.Id,
            TestRunId = report.TestRunId,
            TestType = report.TestType,
            CreatedAt = report.CreatedAt,
            TotalTests = report.TotalTests,
            PassedTests = report.PassedTests,
            FailedTests = report.FailedTests,
            Results = results.Select(result => new TestResultDto
            {
                Id = result.Id,
                TestCaseType = result.TestCase?.Type ?? "Unknown",
                Input = result.TestCase?.Input ?? string.Empty,
                ExpectedStatusCode = result.TestCase?.ExpectedStatusCode,
                ActualStatusCode = GetActualStatusCode(result.Details),
                Passed = result.Outcome == "Pass",
                ResponseBody = GetResponseBody(result.Details)
            }).ToList()
        };
    }


    public async Task<List<TestReportDto>> GetAllReportsAsync(Guid userId)
    {
        _logger.LogInformation("Getting all test reports for user {UserId}", userId);

        var reports = await _reportRepository.GetAllAsync(userId);
        _logger.LogInformation("Found {Count} test reports for user {UserId}", reports.Count, userId);

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
                ActualStatusCode = JsonDocument.Parse(result.Details).RootElement.GetProperty("ActualStatusCode").GetInt32(),
                Passed = result.Outcome == "Pass",
                ResponseBody = JsonDocument.Parse(result.Details).RootElement.GetProperty("ResponseBody").GetString()
            }).ToList()
        }).ToList();
    }

    public async Task<TestReportDto?> GetReportByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting test report {ReportId}", id);

        var report = await _reportRepository.GetByIdAsync(id);
        if (report == null)
        {
            _logger.LogWarning("Test report {ReportId} not found", id);
            return null;
        }

        _logger.LogInformation("Successfully retrieved test report {ReportId}", id);


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
                ActualStatusCode = JsonDocument.Parse(result.Details).RootElement.GetProperty("ActualStatusCode").GetInt32(),
                Passed = result.Outcome == "Pass",
                ResponseBody = JsonDocument.Parse(result.Details).RootElement.GetProperty("ResponseBody").GetString()
            }).ToList()
        };
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        _logger.LogInformation("Deleting test report {ReportId} for user {UserId}", id, userId);

        var report = await _reportRepository.GetByIdAsync(id);
        if (report == null)
        {
            _logger.LogWarning("Test report {ReportId} not found", id);
            return false;
        }

        await _reportRepository.DeleteAsync(id);
        _logger.LogInformation("Test report {ReportId} deleted successfully", id);
        return true;
    }

    private int GetActualStatusCode(string details)
    {
        try
        {
            return int.Parse(JsonDocument.Parse(details).RootElement.GetProperty("ActualStatusCode").GetString() ?? "0");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing actual status code from details: {Details}", details);
            return 0;
        }
    }

    private string? GetResponseBody(string details)
    {
        try
        {
            return JsonDocument.Parse(details).RootElement.GetProperty("ResponseBody").GetString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing response body from details: {Details}", details);
            return null;
        }
    }
}
