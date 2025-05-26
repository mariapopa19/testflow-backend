namespace TestFlow.Domain.Entities;
public class TestResult
{
    public Guid Id { get; set; }
    public Guid TestRunId { get; set; }
    public TestRun TestRun { get; set; } = null!;
    public DateTime? StartedAt { get; set; } = null!;

    public string Outcome { get; set; } = null!;  // ex: "Pass", "Fail"
    public string Details { get; set; } = null!;  // JSON or plain error message
}

