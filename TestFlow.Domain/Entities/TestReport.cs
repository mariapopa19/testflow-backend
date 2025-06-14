namespace TestFlow.Domain.Entities;
public class TestReport
{
    public Guid Id { get; set; }
    public Guid TestRunId { get; set; }
    public Guid UserId { get; set; }
    public string TestType { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public int TotalTests { get; set; }
    public int PassedTests { get; set; }
    public int FailedTests { get; set; }

    // Navigation properties
    public TestRun TestRun { get; set; } = null!;
    public User User { get; set; } = null!;
    public virtual ICollection<TestResult> Results { get; set; } = new List<TestResult>();

}
