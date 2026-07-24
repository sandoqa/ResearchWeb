using Microsoft.EntityFrameworkCore;
using ResearchWeb.Models;

namespace ResearchWeb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Research> Researches { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Research>()
                .ToTable("الابحاث العلمية 2026");

            base.OnModelCreating(modelBuilder);
        }
    }
}