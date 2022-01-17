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

        public virtual DbSet<registration_notifications> registration_notifications { get; set; }
        public virtual DbSet<android_devices> android_devices { get; set; }
        public virtual DbSet<iphone_devices> iphone_devices { get; set; }
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
            });

            modelBuilder.Entity<iphone_devices>(entity =>
            {
                entity.HasKey(e => e.iphone_device_id)
                    .HasName("PRIMARY");

                entity.ToTable("iphone_devices");

                entity.HasIndex(e => e.DeviceId)
                    .HasName("device_id");

                entity.Property(e => e.DeviceId).HasColumnName("device_id");

            });

            modelBuilder.Entity<android_devices>(entity =>
            {
                entity.HasKey(e => e.android_device_id)
                    .HasName("PRIMARY");

                entity.ToTable("android_devices");

                entity.HasIndex(e => e.DeviceId)
                    .HasName("device_id");

                entity.Property(e => e.DeviceId).HasColumnName("device_id");


            });

            modelBuilder.Entity<registration_notifications>(entity =>
            {
                entity.HasKey(e => e.RegisteredNotificationId)
                    .HasName("PRIMARY");

                entity.ToTable("registration_notifications");

                entity.HasIndex(e => e.DeviceId)
                    .HasName("device_id");

                entity.Property(e => e.DeviceId).HasColumnName("device_id");

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

            //modelBuilder.Entity<Users>().HasData(

            //    new Users
            //    {
            //        UserId = 1,
            //        Name = "User-1",
            //        Email = "user1@email.com",
            //        CreatedAt = System.DateTime.Now
            //    },
            //    new Users
            //    {
            //        UserId = 2,
            //        Name = "User-2",
            //        Email = "user2@email.com",
            //        CreatedAt = System.DateTime.Now
            //    },
            //    new Users
            //    {
            //        UserId = 3,
            //        Name = "User-3",
            //        Email = "user3@email.com",
            //        CreatedAt = System.DateTime.Now
            //    },
            //    new Users
            //    {
            //        UserId = 4,
            //        Name = "User-4",
            //        Email = "user4@email.com",
            //        CreatedAt = System.DateTime.Now
            //    },
            //    new Users
            //    {
            //        UserId = 5,
            //        Name = "User-5",
            //        Email = "user5@email.com",
            //        CreatedAt = System.DateTime.Now
            //    }

            //    );

            //modelBuilder.Entity<Phones>().HasData(

            //    new Phones
            //    {
            //        PhoneId = 1,
            //        Number = "123456781",
            //        UserId = 1
            //    },
            //    new Phones
            //    {
            //        PhoneId = 2,
            //        Number = "123456782",
            //        UserId = 2
            //    },
            //    new Phones
            //    {
            //        PhoneId = 3,
            //        Number = "123456783",
            //        UserId = 3
            //    },
            //    new Phones
            //    {
            //        PhoneId = 4,
            //        Number = "123456784",
            //        UserId = 4
            //    },
            //    new Phones
            //    {
            //        PhoneId = 5,
            //        Number = "123456785",
            //        UserId = 5
            //    }

            //    );

            //modelBuilder.Entity<Devices>().HasData(

            //    new Devices
            //    {
            //        DeviceId = 1,
            //        UniqueId = "123456781",
            //        os = "andriod",
            //        UserId = 1
            //    },
            //    new Devices
            //    {
            //        DeviceId = 2,
            //        UniqueId = "111222333",
            //        os = "iphone",
            //        UserId = 1
            //    }

            //    );
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
