using AutoMapper;
using TestFlow.Application.Interfaces.Repository;
using TestFlow.Application.Interfaces.Services;
using TestFlow.Application.Models.Responses;
using TestFlow.Domain.Entities;

namespace TestFlow.Application.Services;
public class TestRunService : ITestRunService
{
    private readonly ITestRunRepository _testRunRepository;
    private readonly IMapper _mapper;

    public TestRunService(ITestRunRepository testRunRepository, IMapper mapper)
    {
        _testRunRepository = testRunRepository;
        _mapper = mapper;
    }
    public async Task<List<TestRunDto>> GetByUserIdAsync(Guid userId)
    {
        var testRuns = await _testRunRepository.GetByUserIdAsync(userId);
        var testRunDtos = _mapper.Map<List<TestRunDto>>(testRuns);
        return testRunDtos;

    }

    public async Task<TestRunDto> GetByIdAsync(Guid id, Guid userId)
    {
        var testRun = await _testRunRepository.GetByIdAsync(id, userId);
        var dto = _mapper.Map<TestRunDto>(testRun);
        return dto;
    }

    public async Task<List<TestRunDto>> GetByEndpointIdAsync(Guid endpointId)
    {
        var testRuns = await _testRunRepository.GetByEndpointIdAsync(endpointId);
        var testRunDtos = _mapper.Map<List<TestRunDto>>(testRuns);
        return testRunDtos;
    }

}
