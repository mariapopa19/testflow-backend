using Microsoft.EntityFrameworkCore;
using TestFlow.Domain.Entities;

namespace TestFlow.Infrastructure;
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserOAuthAccount> UserOAuthAccounts => Set<UserOAuthAccount>();
    public DbSet<Endpoint> Endpoints => Set<Endpoint>();
    public DbSet<TestRun> TestRuns => Set<TestRun>();
    public DbSet<TestResult> TestResults => Set<TestResult>();
    public DbSet<FuzzRule> FuzzRules => Set<FuzzRule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasMany(u => u.OAuthAccounts)
            .WithOne(o => o.User)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Endpoints)
            .WithOne(e => e.User)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasMany(u => u.TestRuns)
            .WithOne(tr => tr.User)
            .HasForeignKey(tr => tr.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Endpoint>()
            .HasMany(e => e.TestRuns)
            .WithOne(tr => tr.Endpoint)
            .HasForeignKey(tr => tr.EndpointId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TestRun>()
            .HasMany(tr => tr.Results)
            .WithOne(r => r.TestRun)
            .HasForeignKey(r => r.TestRunId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TestRun>()
            .HasMany(tr => tr.FuzzRules)
            .WithOne(f => f.TestRun)
            .HasForeignKey(f => f.TestRunId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
