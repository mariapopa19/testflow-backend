namespace TestFlow.Domain.Entities;
public class TestRun
{
    public Guid Id { get; set; }
    public Guid EndpointId { get; set; }
    public Endpoint Endpoint { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime StartedAt { get; set; }
    public string TestType { get; set; } = null!;  // optional: convert to enum

    public ICollection<TestResult> Results { get; set; } = new List<TestResult>();
    public ICollection<FuzzRule> FuzzRules { get; set; } = new List<FuzzRule>();
}
