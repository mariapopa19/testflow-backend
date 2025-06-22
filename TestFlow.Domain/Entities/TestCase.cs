namespace TestFlow.Domain.Entities;

public class TestCase
{
    public Guid Id { get; set; }
    public Guid EndpointId { get; set; }
    public Endpoint Endpoint { get; set; } = null!;
    public string? CustomUrl { get; set; }

    public Guid? TestRunId { get; set; }
    public TestRun? TestRun { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Input { get; set; } = string.Empty;
    public string? ExpectedResponse { get; set; }
    public List<int>? ExpectedStatusCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
}
