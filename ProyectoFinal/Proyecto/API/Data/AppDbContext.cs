using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserToken> UserTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>(entity =>
            {
                entity.ToTable("students");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id);
                entity.Property(x => x.RegistrationNumber).IsRequired().HasMaxLength(30);
                entity.Property(x => x.Name).IsRequired().HasMaxLength(100);
                entity.Property(x => x.LastName).IsRequired().HasMaxLength(100);
                entity.Property(x => x.Graduated).HasDefaultValue(false);
                entity.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(x => x.UpdatedAt);
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.ToTable("user_roles");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id);
                entity.Property(x => x.Name).IsRequired().HasMaxLength(30);
                entity.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(x => x.UpdatedAt);

                entity.HasData(new UserRole {
                    Id=1,
                    Name="ADMIN"
                });

                entity.HasData(new UserRole
                {
                    Id = 2,
                    Name = "STUDENT"
                });
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id);
                entity.Property(x => x.UserRoleId).IsRequired();
                entity.Property(x => x.StudentId);
                entity.Property(x => x.UserName).IsRequired().HasMaxLength(30);
                entity.Property(x => x.Password).IsRequired().HasMaxLength(100);
                entity.Property(x => x.Active).HasDefaultValue(false);
                entity.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(x => x.UpdatedAt);

                entity.HasOne(x => x.UserRole)
                      .WithMany(x => x.Users)
                      .HasForeignKey(x => x.UserRoleId)
                      .HasConstraintName("users_fk_userroleid");

                entity.HasOne(x => x.Student)
                      .WithMany(x => x.Users)
                      .HasForeignKey(x => x.StudentId)
                      .HasConstraintName("users_fk_studentid");

                entity.HasData(new User()
                {
                    Id = new Guid("bfe03e22-65e4-4007-8420-07c1b53c4726"),
                    UserRoleId = 1,
                    UserName = "admin",
                    Password = "9U0zeOGybSi5hk81k/nFzw==.FN5jpe1k2hAMfU0SIg2QuTiwVdhsFdYsC1ykHHAwkzk="
                });
            });
        }
    }
}
