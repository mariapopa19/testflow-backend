﻿using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TestFlow.Application.Interfaces.Repository;
using TestFlow.Application.Models.Requests;
using TestFlow.Domain.Entities;

namespace TestFlow.Infrastructure.Repositories;
public class EndpointRepository : IEndpointRepository
{
    private readonly ApplicationDbContext _context;

    public EndpointRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Endpoint>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Endpoints
            .Where(e => e.UserId == userId)
            .ToListAsync();
    }

    public async Task<Endpoint?> GetByIdAsync(Guid id, Guid userId)
    {
        return await _context.Endpoints
            .Where(e => e.UserId == userId && e.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<int> CountByUserAsync(Guid userId)
    {
        return await _context.Endpoints.CountAsync(e => e.UserId == userId);
    }

    public async Task AddAsync(Endpoint endpoint)
    {
        await _context.Endpoints.AddAsync(endpoint);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Endpoint endpoint)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Get all TestRuns for this endpoint
            var testRuns = await _context.TestRuns
                .Where(tr => tr.EndpointId == endpoint.Id)
                .ToListAsync();

            if (testRuns.Any())
            {
                var testRunIds = testRuns.Select(tr => tr.Id).ToList();

                // 1. First, delete TestReports (they reference TestRuns with Restrict)
                var testReports = await _context.TestReports
                    .Where(tr => testRunIds.Contains(tr.TestRunId))
                    .ToListAsync();

                if (testReports.Any())
                {
                    _context.TestReports.RemoveRange(testReports);
                    await _context.SaveChangesAsync();
                }

                // 2. Delete TestCases for this endpoint (they reference Endpoint directly)
                var testCases = await _context.TestCases
                    .Where(tc => tc.EndpointId == endpoint.Id)
                    .ToListAsync();

                if (testCases.Any())
                {
                    _context.TestCases.RemoveRange(testCases);
                    await _context.SaveChangesAsync();
                }

                // 3. Now delete TestRuns (this will cascade delete TestResults and FuzzRules)
                _context.TestRuns.RemoveRange(testRuns);
                await _context.SaveChangesAsync();
            }

            // 4. Finally, delete the endpoint
            _context.Endpoints.Remove(endpoint);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();

            // Log the exception details
            Console.WriteLine($"DbUpdateException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"InnerException: {ex.InnerException.Message}");

            throw;
        }
    }

    public async Task<bool> UpdateEndpointAsync(Guid id, UpdateEndpointRequest request)
    {
        var endpoint = await _context.Endpoints.FindAsync(id);

        if (endpoint == null)
            return false;

        if (!string.IsNullOrEmpty(request.Name))
            endpoint.Name = request.Name;

        if (!string.IsNullOrEmpty(request.Url))
            endpoint.Url = request.Url;

        if (!string.IsNullOrEmpty(request.Method))
            endpoint.HttpMethod = request.Method;

        if (!string.IsNullOrEmpty(request.RequestBodyModel))
            endpoint.RequestBodyModel = request.RequestBodyModel;

        if (!string.IsNullOrEmpty(request.ResponseBodyModel))
            endpoint.ResponseBodyModel = request.ResponseBodyModel;

        if (request.Headers != null && request.Headers.Any())
            endpoint.HeadersJson = JsonSerializer.Serialize(request.Headers);

        await _context.SaveChangesAsync();
        return true;
    }

}
