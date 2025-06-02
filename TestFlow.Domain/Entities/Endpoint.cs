namespace TestFlow.Domain.Entities;
public class Endpoint
{
    public Guid Id { get; set; }
    public string Url { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string HttpMethod { get; set; } = null!;
    public string RequestBodyModel { get; set; } = null!;
    public string ResponseBodyModel { get; set; } = null!;
    public string? HeadersJson { get; set; } 

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<TestRun> TestRuns { get; set; } = new List<TestRun>();

    public ICollection<TestCase> TestCases { get; set; } = new List<TestCase>();
}
