using Microsoft.EntityFrameworkCore;
using UserService.Models.DbModels;

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

        public virtual DbSet<RegistrationNotifications> RegistrationNotifications { get; set; }
        public virtual DbSet<AndroidDevices> AndroidDevices { get; set; }
        public virtual DbSet<IphoneDevices> IphoneDevices { get; set; }
        public virtual DbSet<Phones> Phones { get; set; }
        public virtual DbSet<Devices> Devices { get; set; }
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

            modelBuilder.Entity<Devices>(entity =>
            {
                entity.HasKey(e => e.DeviceId)
                    .HasName("PRIMARY");

                entity.ToTable("devices");

                entity.HasIndex(e => e.UserId)
                    .HasName("user_id");

                entity.Property(e => e.DeviceId).HasColumnName("device_id");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Devices)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("devices_ibfk_1");

                entity.Property(e => e.OS)
                    .HasColumnName("os")
                    .HasColumnType("enum('android','ios')")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            });

            modelBuilder.Entity<IphoneDevices>(entity =>
            {
                entity.HasKey(e => e.IphoneDeviceId)
                    .HasName("PRIMARY");

                entity.ToTable("iphone_devices");

                entity.HasIndex(e => e.DeviceId)
                    .HasName("device_id");

                entity.Property(e => e.DeviceId).HasColumnName("device_id");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.IosIdentifier).HasColumnName("ios_identifier");

                entity.Property(e => e.IphoneDeviceId).HasColumnName("iphone_device_id");

            });

            modelBuilder.Entity<AndroidDevices>(entity =>
            {
                entity.HasKey(e => e.AndroidDeviceId)
                    .HasName("PRIMARY");

                entity.ToTable("android_devices");

                entity.HasIndex(e => e.DeviceId)
                    .HasName("device_id");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.DeviceId).HasColumnName("device_id");

                entity.Property(e => e.AndroidIdentifier).HasColumnName("android_identifier");

                entity.Property(e => e.AndroidDeviceId).HasColumnName("android_device_id");


            });

            modelBuilder.Entity<RegistrationNotifications>(entity =>
            {
                entity.HasKey(e => e.RegisteredNotificationId)
                    .HasName("PRIMARY");

                entity.ToTable("registration_notifications");

                entity.HasIndex(e => e.DeviceId)
                    .HasName("device_id");

                entity.Property(e => e.DeviceId).HasColumnName("device_id");

                entity.Property(e => e.FcmToken).HasColumnName("fcm_token");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.RegisteredNotificationId).HasColumnName("regitered_notification_id");

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
                    .HasColumnType("varchar(40)")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
