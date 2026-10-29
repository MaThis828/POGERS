using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Pogers.Infrastructure
{
    /// <summary>
    /// Registrierungen der Infrastruktur. Jeder Host setzt sich zusammen, was er braucht.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Datenbankzugriff. Brauchen Worker und Api.
        /// </summary>
        public static IServiceCollection AddPersistence(
            this IServiceCollection dienste, IConfiguration konfiguration)
        {
            dienste.AddDbContext<PogersDbContext>(o =>
                o.UseSqlite(konfiguration.GetConnectionString("Default")));

            return dienste;
        }
    }
}
