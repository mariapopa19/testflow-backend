using TestFlow.Application.Models.Tests;

namespace TestFlow.Application.Models.Responses;
public class TestRunDto
{
    public Guid Id { get; set; }
    public Guid EndpointId { get; set; }
    public string EndpointName { get; set; } = string.Empty;
    public string TestType { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public List<TestResultDto> Results { get; set; } = new();

}
