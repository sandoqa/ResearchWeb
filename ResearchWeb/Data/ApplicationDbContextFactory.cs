using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ResearchWeb.Data
{
    public class ApplicationDbContextFactory
        : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(
            string[] args)
        {
            var optionsBuilder =
                new DbContextOptionsBuilder<ApplicationDbContext>();

            var connectionString =
                Environment.GetEnvironmentVariable("DATABASE_URL");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString =
                    "Host=localhost;Port=5432;Database=researchweb;Username=postgres;Password=postgres";
            }

            optionsBuilder.UseNpgsql(connectionString);

            return new ApplicationDbContext(
                optionsBuilder.Options
            );
        }
    }
}