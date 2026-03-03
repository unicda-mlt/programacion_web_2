using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserToken> UserTokens { get; set; }
        public DbSet<ScrutinyStatus> ScrutinyStatuses { get; set; }
        public DbSet<Scrutiny> Scrutinies { get; set; }
        public DbSet<ScrutinySign> ScrutinySigns { get; set; }
        public DbSet<CandidacyType> CandidacyTypes { get; set; }
        public DbSet<Slate> Slates { get; set; }
        public DbSet<SlateCandidacy> SlateCandidacies { get; set; }
        public DbSet<Vote> Votes { get; set; }

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
                    Password = "9U0zeOGybSi5hk81k/nFzw==.FN5jpe1k2hAMfU0SIg2QuTiwVdhsFdYsC1ykHHAwkzk=",
                    Active = true
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

            modelBuilder.Entity<ScrutinyStatus>(entity =>
            {
                entity.ToTable("scrutiny_statuses");
                entity.HasKey(scrutinyStatus => scrutinyStatus.Id);
                entity.Property(scrutinyStatus => scrutinyStatus.Id);
                entity.Property(scrutinyStatus => scrutinyStatus.Name).IsRequired().HasMaxLength(30);
                entity.Property(scrutinyStatus => scrutinyStatus.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(scrutinyStatus => scrutinyStatus.UpdatedAt);

                entity.HasData(new ScrutinyStatus
                {
                    Id = 1,
                    Name = "OPEN"
                });

                entity.HasData(new ScrutinyStatus
                {
                    Id = 2,
                    Name = "CLOSED"
                });

                entity.HasData(new ScrutinyStatus
                {
                    Id = 3,
                    Name = "SIGNED"
                });
            });

            modelBuilder.Entity<Scrutiny>(entity =>
            {
                entity.ToTable("scrutinies");
                entity.HasKey(scrutiny => scrutiny.Id);
                entity.Property(scrutiny => scrutiny.Id);
                entity.Property(scrutiny => scrutiny.StatusId).IsRequired();
                entity.Property(scrutiny => scrutiny.Title).IsRequired().HasMaxLength(100);
                entity.Property(scrutiny => scrutiny.Description).IsRequired().HasMaxLength(1000);
                entity.Property(scrutiny => scrutiny.StartDate).IsRequired();
                entity.Property(scrutiny => scrutiny.EndDate).IsRequired();
                entity.Property(scrutiny => scrutiny.ImageUrl).HasMaxLength(1000);
                entity.Property(scrutiny => scrutiny.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(scrutiny => scrutiny.UpdatedAt);

                entity.HasOne(scrutiny => scrutiny.ScrutinyStatus)
                      .WithMany(scrutinyStatus => scrutinyStatus.Scrutinies)
                      .HasForeignKey(scrutiny => scrutiny.StatusId)
                      .HasConstraintName("scrutinies_fk_statusid");
            });

            modelBuilder.Entity<ScrutinySign>(entity =>
            {
                entity.ToTable("scrutiny_signs");
                entity.HasKey(scrutinySign => scrutinySign.Id);
                entity.Property(scrutinySign => scrutinySign.Id);
                entity.Property(scrutinySign => scrutinySign.ScrutinyId).IsRequired();
                entity.Property(scrutinySign => scrutinySign.FileUrl).IsRequired().HasMaxLength(300);
                entity.Property(scrutinySign => scrutinySign.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(scrutinySign => scrutinySign.UpdatedAt);

                entity.HasOne(scrutinySign => scrutinySign.Scrutiny)
                      .WithMany(scrutiny => scrutiny.ScrutinySigns)
                      .HasForeignKey(scrutinySign => scrutinySign.ScrutinyId)
                      .HasConstraintName("scrutiny_signs_fk_scrutinyid");
            });

            modelBuilder.Entity<CandidacyType>(entity =>
            {
                entity.ToTable("candidacy_types");
                entity.HasKey(candidacyType => candidacyType.Id);
                entity.Property(candidacyType => candidacyType.Id);
                entity.Property(candidacyType => candidacyType.Name).IsRequired().HasMaxLength(30);
                entity.Property(candidacyType => candidacyType.Position).IsRequired();
                entity.Property(candidacyType => candidacyType.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(candidacyType => candidacyType.UpdatedAt);
            });

            modelBuilder.Entity<Slate>(entity =>
            {
                entity.ToTable("slates");
                entity.HasKey(slate => slate.Id);
                entity.Property(slate => slate.Id);
                entity.Property(slate => slate.ScrutinyId).IsRequired();
                entity.Property(slate => slate.Position).IsRequired();
                entity.Property(slate => slate.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(slate => slate.UpdatedAt);

                entity.HasOne(slate => slate.Scrutiny)
                      .WithMany(scrutiny => scrutiny.Slates)
                      .HasForeignKey(slate => slate.ScrutinyId)
                      .HasConstraintName("slates_fk_scrutinyid");
            });

            modelBuilder.Entity<SlateCandidacy>(entity =>
            {
                entity.ToTable("slate_candidacies");
                entity.HasKey(slateCandidacy => slateCandidacy.Id);
                entity.Property(slateCandidacy => slateCandidacy.Id);
                entity.Property(slateCandidacy => slateCandidacy.SlateId).IsRequired();
                entity.Property(slateCandidacy => slateCandidacy.CandidacyTypeId).IsRequired();
                entity.Property(slateCandidacy => slateCandidacy.Name).IsRequired().HasMaxLength(100);
                entity.Property(slateCandidacy => slateCandidacy.LastName).IsRequired().HasMaxLength(100);
                entity.Property(slateCandidacy => slateCandidacy.ImageUrl).HasMaxLength(300);
                entity.Property(slateCandidacy => slateCandidacy.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(slateCandidacy => slateCandidacy.UpdatedAt);

                entity.HasOne(slateCandidacy => slateCandidacy.Slate)
                      .WithMany(slate => slate.SlateCandidacies)
                      .HasForeignKey(slateCandidacy => slateCandidacy.SlateId)
                      .HasConstraintName("slate_candidacies_fk_slateid");

                entity.HasOne<CandidacyType>()
                      .WithMany()
                      .HasForeignKey(slateCandidacy => slateCandidacy.CandidacyTypeId)
                      .HasConstraintName("slate_candidacies_fk_candidacytypeid");
            });

            modelBuilder.Entity<Vote>(entity =>
            {
                entity.ToTable("votes");
                entity.HasKey(vote => vote.Id);
                entity.Property(vote => vote.Id);
                entity.Property(vote => vote.ScrutinyId).IsRequired();
                entity.Property(vote => vote.SlateId).IsRequired();
                entity.Property(vote => vote.UserId).IsRequired();
                entity.Property(vote => vote.StudentId).IsRequired();
                entity.Property(vote => vote.IssueDate).IsRequired();
                entity.Property(vote => vote.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(vote => vote.UpdatedAt);

                entity.HasOne(vote => vote.Scrutiny)
                      .WithMany(scrutiny => scrutiny.Votes)
                      .HasForeignKey(vote => vote.ScrutinyId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("votes_fk_scrutinyid");

                entity.HasOne(vote => vote.Slate)
                      .WithMany(slate => slate.Votes)
                      .HasForeignKey(vote => vote.SlateId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("votes_fk_slateid");

                entity.HasOne(vote => vote.User)
                      .WithMany()
                      .HasForeignKey(vote => vote.UserId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("votes_fk_userid");

                entity.HasOne(vote => vote.Student)
                      .WithMany()
                      .HasForeignKey(vote => vote.StudentId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("votes_fk_studentid");
            });
        }
    }
}
