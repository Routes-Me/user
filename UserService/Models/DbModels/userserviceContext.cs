using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace UserService.Models.DBModels
{
    public partial class userserviceContext : DbContext
    {
        public userserviceContext()
        {
        }

        public userserviceContext(DbContextOptions<userserviceContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Applications> Applications { get; set; }
        public virtual DbSet<Phones> Phones { get; set; }
        public virtual DbSet<Privileges> Privileges { get; set; }
        public virtual DbSet<Roles> Roles { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<UsersRoles> UsersRoles { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Applications>(entity =>
            {
                entity.HasKey(e => e.ApplicationId)
                    .HasName("PRIMARY");

                entity.ToTable("applications");

                entity.Property(e => e.ApplicationId).HasColumnName("application_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("timestamp");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");
            });

            modelBuilder.Entity<Phones>(entity =>
            {
                entity.HasKey(e => e.PhoneId)
                    .HasName("PRIMARY");

                entity.ToTable("phones");

                entity.HasIndex(e => e.UserId)
                    .HasName("user_id");

                entity.Property(e => e.PhoneId).HasColumnName("phone_id");

                entity.Property(e => e.IsVerified).HasColumnName("is_verified");

                entity.Property(e => e.Number)
                    .HasColumnName("number")
                    .HasColumnType("varchar(20)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Phones)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("phones_ibfk_1");
            });

            modelBuilder.Entity<Privileges>(entity =>
            {
                entity.HasKey(e => e.PrivilegeId)
                    .HasName("PRIMARY");

                entity.ToTable("privileges");

                entity.Property(e => e.PrivilegeId).HasColumnName("privilege_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("timestamp");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");
            });

            modelBuilder.Entity<Roles>(entity =>
            {
                entity.HasKey(e => new { e.ApplicationId, e.PrivilegeId })
                    .HasName("PRIMARY");

                entity.ToTable("roles");

                entity.HasIndex(e => e.PrivilegeId)
                    .HasName("privilege_id");

                entity.Property(e => e.ApplicationId).HasColumnName("application_id");

                entity.Property(e => e.PrivilegeId).HasColumnName("privilege_id");

                entity.HasOne(d => d.Application)
                    .WithMany(p => p.Roles)
                    .HasForeignKey(d => d.ApplicationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("roles_ibfk_2");

                entity.HasOne(d => d.Privilege)
                    .WithMany(p => p.Roles)
                    .HasForeignKey(d => d.PrivilegeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("roles_ibfk_1");
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasKey(e => e.UserId)
                    .HasName("PRIMARY");

                entity.ToTable("users");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("timestamp");

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .HasColumnType("varchar(40)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.IsEmailVerified).HasColumnName("is_email_verified");

                entity.Property(e => e.LastLoginDate)
                    .HasColumnName("last_login_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Password)
                    .HasColumnName("password")
                    .HasColumnType("char(64)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");
            });

            modelBuilder.Entity<UsersRoles>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.ApplicationId, e.PrivilegeId })
                    .HasName("PRIMARY");

                entity.ToTable("users_roles");

                entity.HasIndex(e => new { e.ApplicationId, e.PrivilegeId })
                    .HasName("application_id");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.ApplicationId).HasColumnName("application_id");

                entity.Property(e => e.PrivilegeId).HasColumnName("privilege_id");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UsersRoles)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("users_roles_ibfk_1");

                entity.HasOne(d => d.Roles)
                    .WithMany(p => p.UsersRoles)
                    .HasForeignKey(d => new { d.ApplicationId, d.PrivilegeId })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("users_roles_ibfk_2");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
