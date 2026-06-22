using Microsoft.EntityFrameworkCore;
using BGVSystem.Domain.Entities;

namespace BGVSystem.Persistence.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Candidate> Candidates => Set<Candidate>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Document> Documents => Set<Document>();

    public DbSet<Verification> Verifications { get; set; }

    public DbSet<AuditLog> AuditLogs { get; set; }

    public DbSet<Notification> Notifications { get; set; }

    public DbSet<CandidateAssignment>
        CandidateAssignments
    { get; set; }

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Candidate>()
            .HasQueryFilter(x => !x.IsDeleted);
    }
}