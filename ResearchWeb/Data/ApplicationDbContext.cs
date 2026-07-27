using Microsoft.EntityFrameworkCore;
using ResearchWeb.Models;

namespace ResearchWeb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }


        // المستخدمون
        public DbSet<User> Users { get; set; } = null!;


        // الأبحاث العلمية
        public DbSet<Research2026> Researches { get; set; } = null!;


        // الزوار
        public DbSet<Visitor> Visitors { get; set; } = null!;



        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);



            // جدول الأبحاث
            modelBuilder.Entity<Research2026>()
                .ToTable("الابحاث العلمية 2026");



            // جدول الزوار
            modelBuilder.Entity<Visitor>()
                .ToTable("Visitors");



            // المفتاح الأساسي للزوار
            modelBuilder.Entity<Visitor>()
                .HasKey(x => x.ID);

        }
    }
}