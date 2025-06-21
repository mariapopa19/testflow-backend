using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
    private readonly IMapper _mapper;

    public TestReportService(
        ITestReportRepository reportRepository,
        ITestRunRepository testRunRepository,
        ITestResultRepository testResultRepository,
        ILogger<TestReportService> logger,
        IMapper mapper)
    {
        _reportRepository = reportRepository;
        _testRunRepository = testRunRepository;
        _testResultRepository = testResultRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<TestReportDto> GenerateReportFromTestRunAsync(Guid testRunId, Guid userId)
    {
        using var transaction = await _reportRepository.BeginTransactionAsync();

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
            await ((IDbContextTransaction)transaction).CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating test report for test run {TestRunId}", testRunId);
            throw;
        }

        var savedReport = await _reportRepository.GetByIdAsync(report.Id);
        var reportDto = _mapper.Map<TestReportDto>(savedReport);

        // Sum durations of all test results (handle nulls)
        reportDto.Duration = savedReport?.Results?.Where(r => r.Duration.HasValue).Select(r => r.Duration!.Value).DefaultIfEmpty(TimeSpan.Zero).Aggregate((a, b) => a + b);


        return reportDto;
    }


    public async Task<List<TestReportDto>> GetAllReportsAsync(Guid userId)
    {
        _logger.LogInformation("Getting all test reports for user {UserId}", userId);

        var reports = await _reportRepository.GetAllAsync(userId);
        _logger.LogInformation("Found {Count} test reports for user {UserId}", reports.Count, userId);

        var reportDtos = _mapper.Map<List<TestReportDto>>(reports);

        // Set Duration for each report DTO
        for (int i = 0; i < reports.Count; i++)
        {
            var report = reports[i];
            var dto = reportDtos[i];
            dto.Duration = report?.Results?
                .Where(r => r.Duration.HasValue)
                .Select(r => r.Duration!.Value)
                .DefaultIfEmpty(TimeSpan.Zero)
                .Aggregate((a, b) => a + b);
        }

        return reportDtos;
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


        var reportDto = _mapper.Map<TestReportDto>(report);
        reportDto.Duration = report.Results?.Where(r => r.Duration.HasValue).Select(r => r.Duration.Value).DefaultIfEmpty(TimeSpan.Zero).Aggregate((a, b) => a + b);

        return reportDto;

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

    public async Task<List<TestReportDto>> GetRecentReportsAsync(Guid userId, int limit)
    {
        var reports = await _reportRepository.GetRecentByUserAsync(userId, limit);
        return _mapper.Map<List<TestReportDto>>(reports);
    }

}
