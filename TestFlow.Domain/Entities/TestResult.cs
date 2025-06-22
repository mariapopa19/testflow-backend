namespace TestFlow.Domain.Entities;
public class TestResult
{
    public Guid Id { get; set; }
    public Guid TestRunId { get; set; }
    public TestRun TestRun { get; set; } = null!;
    public string? CalledUrl { get; set; }

    public Guid? TestCaseId { get; set; } // Nullable if some results may not be linked to a test case
    public TestCase? TestCase { get; set; }

    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public TimeSpan? Duration { get; set; }

    public string Outcome { get; set; } = null!;  // ex: "Pass", "Fail"
    public string Details { get; set; } = null!;  // JSON or plain error message

    // Add these lines:
    public Guid? ReportId { get; set; }
    public TestReport? Report { get; set; }
}

