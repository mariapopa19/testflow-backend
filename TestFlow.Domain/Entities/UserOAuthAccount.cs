namespace TestFlow.Domain.Entities;

public class UserOAuthAccount
{
    public Guid Id { get; set; }
    public string Provider { get; set; } = null!;  // e.g. "Google"
    public string ProviderUserId { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
