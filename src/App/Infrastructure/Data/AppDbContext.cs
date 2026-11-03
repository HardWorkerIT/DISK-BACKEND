using Microsoft.EntityFrameworkCore;
using Common.Abstracts;
using App.Infrastructure.Data;
using Common.Infrastructure.Data;
using Users.Domain.Entities;
using Access.Domain.Entities;
using Files.Domain.Entities;
using File = Files.Domain.Entities.File;

namespace App.Infrastructure.Data;

#pragma warning disable 1591
public class AppDbContext : DbContext, IDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<AppFile> AppFiles { get; set; } = default!;
    public DbSet<File> Files { get; set; } = default!;
    public DbSet<Folder> Folders { get; set; } = default!;
    public DbSet<User> Users { get; set; } = default!;
    public DbSet<AccessToken> AccessTokens { get; set; } = default!;
    public DbSet<Log> Logs { get; set; } = default!;

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        //todo а точно это тут должно быть?

        // Настройка сущности File
        modelBuilder.Entity<File>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("files");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Uuid).HasColumnName("uuid");
            entity.Property(e => e.FolderId).HasColumnName("folder_id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            
            // Альтернативный ключ для связи с AppFile
            entity.HasAlternateKey(e => e.Uuid);
            entity.HasIndex(e => new { e.FolderId, e.DeletedAt })
                .HasDatabaseName("IX_files_folder_deleted_at");
            
            // Связь File -> Folder (многие к одному)
            entity.HasOne<Folder>()
                .WithMany()
                .HasForeignKey(e => e.FolderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Настройка сущности Folder
        modelBuilder.Entity<Folder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("folders");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.Name).HasColumnName("group_name");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.HasIndex(e => new { e.ParentId, e.DeletedAt })
                .HasDatabaseName("IX_folders_parent_deleted_at");
            
            // Самореферентная связь Folder -> Folder (иерархия папок)
            entity.HasOne<Folder>()
                .WithMany()
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Настройка сущности User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("users");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Login).HasColumnName("login");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Username).HasColumnName("username");
            entity.Property(e => e.Password).HasColumnName("password");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.HasIndex(e => e.Login)
                .IsUnique()
                .HasFilter("deleted_at IS NULL")
                .HasDatabaseName("UX_users_active_login");
            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasFilter("deleted_at IS NULL AND email IS NOT NULL")
                .HasDatabaseName("UX_users_active_email");
        });

        // Настройка сущности AccessToken
        modelBuilder.Entity<AccessToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("tokens");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Token).HasColumnName("token");
            entity.Property(e => e.ExpiredAt).HasColumnName("expired_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(e => new { e.UserId, e.ExpiredAt })
                .HasDatabaseName("IX_tokens_user_expired_at");
            entity.HasIndex(e => e.Token).IsUnique().HasDatabaseName("UX_tokens_token");
            
            // Связь AccessToken -> User (многие к одному)
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });


        // Настройка сущности Log
        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("logs");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.IpAddress).HasColumnName("ip_address");
            entity.Property(e => e.UserAgent).HasColumnName("user_agent");
            entity.Property(e => e.Action).HasColumnName("action");
            entity.Property(e => e.EntityType).HasColumnName("entity_type");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.Details).HasColumnName("details");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
        });

        // Настройка сущности AppFile
        modelBuilder.Entity<AppFile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("app_files");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Uuid).HasColumnName("uuid");
            entity.Property(e => e.Filename).HasColumnName("filename");
            entity.Property(e => e.Path).HasColumnName("path");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            
            // Связь AppFile -> File (один к одному через Uuid)
            entity.HasOne<File>()
                .WithOne()
                .HasPrincipalKey<File>(f => f.Uuid)
                .HasForeignKey<AppFile>(af => af.Uuid)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
