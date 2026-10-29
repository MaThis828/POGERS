using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Pogers.Infrastructure
{
    /// <summary>
    /// Damit "dotnet ef" ohne Startprojekt funktioniert. Wird nur zur Entwurfszeit benutzt.
    /// </summary>
    public class PogersDbContextFactory : IDesignTimeDbContextFactory<PogersDbContext>
    {
        public PogersDbContext CreateDbContext(string[] args)
        {
            var optionen = new DbContextOptionsBuilder<PogersDbContext>()
                .UseSqlite("Data Source=pogers.db")
                .Options;

            return new PogersDbContext(optionen);
        }
    }
}
