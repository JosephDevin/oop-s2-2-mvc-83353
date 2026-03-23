using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using oop_s2_2_mvc_83303.Models;

namespace oop_s2_2_mvc_83303.Data;

/// <summary>
/// The primary database context, extending IdentityDbContext to support users and roles.
/// </summary>
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

    /// <summary>
    /// Configures the database schema and relationships.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Define relationship: One Premises to many Inspections
        builder.Entity<Premises>()
            .HasMany(p => p.Inspections)
            .WithOne(i => i.Premises)
            .HasForeignKey(i => i.PremisesId)
            .OnDelete(DeleteBehavior.Cascade); // If premises deleted, delete inspections

        // Define relationship: One Inspection to many FollowUps
        builder.Entity<Inspection>()
            .HasMany(i => i.FollowUps)
            .WithOne(f => f.Inspection)
            .HasForeignKey(f => f.InspectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
