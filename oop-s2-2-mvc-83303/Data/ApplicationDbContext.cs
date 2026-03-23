using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using oop_s2_2_mvc_83303.Models;

namespace oop_s2_2_mvc_83303.Data;

/// The primary database context, extending IdentityDbContext to support users and roles.
public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Database Tables
    public DbSet<Premises> Premises { get; set; }
    public DbSet<Inspection> Inspections { get; set; }
    public DbSet<FollowUp> FollowUps { get; set; }

    /// Configures the database schema and relationships.
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Define relationship: One Premises to many Inspections
        builder.Entity<Premises>()
            .HasMany(p => p.Inspections)
            .WithOne(i => i.Premises)
            .HasForeignKey(i => i.PremisesId)
            .OnDelete(DeleteBehavior.Cascade);

        // Define relationship: One Inspection to many FollowUps
        builder.Entity<Inspection>()
            .HasMany(i => i.FollowUps)
            .WithOne(f => f.Inspection)
            .HasForeignKey(f => f.InspectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
