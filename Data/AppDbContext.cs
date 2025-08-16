using Microsoft.EntityFrameworkCore;
using AuthApiDemo.Models;

namespace AuthApiDemo.Data
{
    public class AppDbContext : DbContext
    {

        private readonly IConfiguration _configuration;

        public AppDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public DbSet<User> Users { get; set; }

        
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("TutorialAppSchema");
        modelBuilder.Entity<User>()
        .ToTable("Users", "TutorialAppSchema")
        .HasKey(u => u.UserId);


    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(_configuration.GetConnectionString("DefaultConnection"),
            optionsBuilder => optionsBuilder.EnableRetryOnFailure());
            
        }
    } 
    }
} 