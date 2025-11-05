using Microsoft.EntityFrameworkCore;
using NiceWebCode.Domain.Entities;

namespace NiceWebCode.Infrastructure.Data;

/// <summary>
/// 应用数据库上下文
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<AiTask> AiTasks => Set<AiTask>();
    public DbSet<OutputChunk> OutputChunks => Set<OutputChunk>();
    public DbSet<WorkspaceFile> WorkspaceFiles => Set<WorkspaceFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Session配置
        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.ShareToken).IsUnique();
        });

        // AiTask配置
        modelBuilder.Entity<AiTask>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Prompt).IsRequired();
            entity.Property(e => e.CliToolName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).HasConversion<string>();

            entity.HasOne(e => e.Session)
                .WithMany(s => s.Tasks)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.Status);
        });

        // OutputChunk配置
        modelBuilder.Entity<OutputChunk>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Content).IsRequired();

            entity.HasOne(e => e.Session)
                .WithMany(s => s.OutputChunks)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.SequenceNumber);
        });

        // WorkspaceFile配置
        modelBuilder.Entity<WorkspaceFile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RelativePath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);

            entity.HasOne(e => e.Session)
                .WithMany()
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.SessionId);
        });
    }
}
