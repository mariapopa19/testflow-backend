namespace TestFlow.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = "User";

    public ICollection<UserOAuthAccount> OAuthAccounts { get; set; } = new List<UserOAuthAccount>();
    public ICollection<Endpoint> Endpoints { get; set; } = new List<Endpoint>();
    public ICollection<TestRun> TestRuns { get; set; } = new List<TestRun>();
}
