using Microsoft.EntityFrameworkCore;
using AuthApiDemo.Domain.Entities;

namespace AuthApiDemo.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserAuth> UserAuths { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("TutorialAppSchema");
            
            // Configure User entity
            modelBuilder.Entity<User>()
                .ToTable("Users", "TutorialAppSchema")
                .HasKey(u => u.UserId);

            modelBuilder.Entity<User>()
                .Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<User>()
                .Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.Gender)
                .HasMaxLength(50);

            // Configure UserAuth entity
            modelBuilder.Entity<UserAuth>()
                .ToTable("UserAuth", "TutorialAppSchema")
                .HasKey(ua => ua.UserAuthId);

            modelBuilder.Entity<UserAuth>()
                .Property(ua => ua.Email)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<UserAuth>()
                .HasIndex(ua => ua.Email)
                .IsUnique();

            modelBuilder.Entity<UserAuth>()
                .Property(ua => ua.PasswordHash)
                .IsRequired();

            modelBuilder.Entity<UserAuth>()
                .Property(ua => ua.PasswordSalt)
                .IsRequired();

            // Configure relationship between User and UserAuth
            modelBuilder.Entity<UserAuth>()
                .HasOne(ua => ua.User)
                .WithOne()
                .HasForeignKey<UserAuth>(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}