using FUNewsManagement_AIAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FUNewsManagement_AIAPI.Data;

public class AiDbContext : DbContext
{
    public AiDbContext(DbContextOptions<AiDbContext> options)
        : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    public virtual DbSet<Tag> Tags { get; set; }
    public virtual DbSet<TagLearningCache> TagLearningCaches { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.TagId).HasName("PK_HashTag");

            entity.ToTable("Tag");

            entity.Property(e => e.TagId)
                .ValueGeneratedNever()
                .HasColumnName("TagID");
            entity.Property(e => e.Note).HasMaxLength(400);
            entity.Property(e => e.TagName).HasMaxLength(50);
        });

        modelBuilder.Entity<TagLearningCache>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("TagLearningCache");

            entity.Property(e => e.Keyword).HasMaxLength(255).IsRequired();
            entity.Property(e => e.TagName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.LastUpdated).HasColumnType("datetime");
            
            // To prevent duplicate keywords for same tag, create a unique index
            entity.HasIndex(e => new { e.Keyword, e.TagName }).IsUnique();
        });
    }
}
