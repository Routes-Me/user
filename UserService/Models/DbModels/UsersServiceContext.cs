using Microsoft.EntityFrameworkCore;

namespace UserService.Models.DBModels
{
    public partial class UsersServiceContext : DbContext
    {
        public UsersServiceContext()
        {
        }

        public UsersServiceContext(DbContextOptions<UsersServiceContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Phones> Phones { get; set; }
        public virtual DbSet<Users> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("phones_ibfk_1");
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

                entity.Property(e => e.IsEmailVerified).HasColumnName("is_email_verified");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
