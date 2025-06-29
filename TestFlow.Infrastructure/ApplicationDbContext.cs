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
    public DbSet<TestCase> TestCases => Set<TestCase>();
    public DbSet<FuzzRule> FuzzRules => Set<FuzzRule>();
    public DbSet<TestReport> TestReports => Set<TestReport>();

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
            .OnDelete(DeleteBehavior.Restrict);

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

        modelBuilder.Entity<TestCase>()
            .HasOne(tc => tc.Endpoint)
            .WithMany(e => e.TestCases)
            .HasForeignKey(tc => tc.EndpointId)
            .OnDelete(DeleteBehavior.Cascade); 

        modelBuilder.Entity<TestCase>()
            .HasOne(tc => tc.TestRun)
            .WithMany(tr => tr.TestCases)
            .HasForeignKey(tc => tc.TestRunId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<TestResult>()
            .HasOne(tr => tr.TestCase)
            .WithMany(tc => tc.TestResults)
            .HasForeignKey(tr => tr.TestCaseId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<TestResult>()
            .HasOne(tr => tr.Report)
            .WithMany(r => r.Results)
            .HasForeignKey(tr => tr.ReportId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<TestReport>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.TestRun)
                .WithMany()
                .HasForeignKey(e => e.TestRunId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
