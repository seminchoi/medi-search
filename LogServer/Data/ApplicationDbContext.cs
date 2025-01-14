using LogServer.Models;
using Microsoft.EntityFrameworkCore;

namespace LogServer.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Institution> Institutions { get; set; } = null!;
    public DbSet<InstitutionHour> InstitutionHour { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Institution>(entity =>
        {
            entity.ToTable("institution");
            
            entity.Property(e => e.Location)
                .HasColumnType("geography");
            
            entity
                .HasIndex(e => e.Location)
                .HasDatabaseName("IX_Location");

            entity.HasOne<InstitutionHour>(e => e.InstitutionHour)
                .WithOne(e => e.Institution)
                .HasForeignKey<InstitutionHour>(e => e.InstitutionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<InstitutionHour>(entity =>
        {
            entity.ToTable("InstitutionHour");
        });
    }
}