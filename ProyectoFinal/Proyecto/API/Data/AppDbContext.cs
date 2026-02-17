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
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.ToTable("user_roles");
                entity.HasKey(userRole => userRole.Id);
                entity.Property(userRole => userRole.Id);
                entity.Property(userRole => userRole.Name).IsRequired().HasMaxLength(30);
                entity.Property(userRole => userRole.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(userRole => userRole.UpdatedAt);

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
                entity.HasKey(user => user.Id);
                entity.Property(user => user.Id);
                entity.Property(user => user.UserRoleId).IsRequired();
                entity.Property(user => user.UserName).IsRequired().HasMaxLength(30);
                entity.Property(user => user.Password).IsRequired().HasMaxLength(100);
                entity.Property(user => user.Active).HasDefaultValue(false);
                entity.Property(user => user.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(user => user.UpdatedAt);

                entity.HasOne(user => user.UserRole)
                      .WithMany(userRole => userRole.Users)
                      .HasForeignKey(user => user.UserRoleId)
                      .HasConstraintName("users_fk_userroleid");

                entity.HasData(new User()
                {
                    Id = new Guid("bfe03e22-65e4-4007-8420-07c1b53c4726"),
                    UserRoleId = 1,
                    UserName = "admin",
                    Password = "9U0zeOGybSi5hk81k/nFzw==.FN5jpe1k2hAMfU0SIg2QuTiwVdhsFdYsC1ykHHAwkzk="
                });
            });

            modelBuilder.Entity<Student>(entity =>
            {
                entity.ToTable("students");
                entity.HasKey(student => student.Id);
                entity.Property(student => student.Id);
                entity.Property(student => student.RegistrationNumber).IsRequired().HasMaxLength(30);
                entity.Property(student => student.Name).IsRequired().HasMaxLength(100);
                entity.Property(student => student.LastName).IsRequired().HasMaxLength(100);
                entity.Property(student => student.Graduated).HasDefaultValue(false);
                entity.Property(student => student.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(student => student.UpdatedAt);

                entity.HasOne(student => student.User)
                      .WithMany(user => user.Students)
                      .HasForeignKey(student => student.UserId)
                      .HasConstraintName("students_fk_userid");
            });
        }
    }
}
