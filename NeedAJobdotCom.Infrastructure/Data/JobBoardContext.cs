using Microsoft.EntityFrameworkCore;
using NeedAJobdotCom.Core.Entities;
using System.Text.Json;

namespace NeedAJobdotCom.Infrastructure.Data
{
    public class JobBoardContext : DbContext
    {
        public JobBoardContext(DbContextOptions<JobBoardContext> options) : base(options)
        {
        }
        
        public DbSet<Job> Jobs { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Job>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);
                    
                entity.Property(e => e.Company)
                    .IsRequired()
                    .HasMaxLength(100);
                    
                entity.Property(e => e.Description)
                    .IsRequired();
                    
                entity.Property(e => e.Location)
                    .IsRequired()
                    .HasMaxLength(100);
                    
                entity.Property(e => e.Category)
                    .IsRequired()
                    .HasMaxLength(50);
                    
                entity.Property(e => e.Source)
                    .IsRequired()
                    .HasMaxLength(50);
                
                // SQLite-compatible JSON storage
                entity.Property(e => e.Tags)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                    )
                    .HasColumnType("TEXT"); // Changed from nvarchar(max) to TEXT
                
                // Database indexes
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.Location);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.PostedDate);
                
                // SQLite-compatible unique constraint
                entity.HasIndex(e => new { e.ExternalId, e.Source })
                    .IsUnique();
                
                // SQLite-compatible defaults
                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);
                    
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("datetime('now')"); // SQLite syntax
                    
                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("datetime('now')"); // SQLite syntax
            });
        }
    }
}