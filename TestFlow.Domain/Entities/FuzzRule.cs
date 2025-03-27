namespace TestFlow.Domain.Entities;
public class FuzzRule
{
    public Guid Id { get; set; }
    public Guid TestRunId { get; set; }
    public TestRun TestRun { get; set; } = null!;

    public string Rule { get; set; } = null!;  // JSON or string
}
