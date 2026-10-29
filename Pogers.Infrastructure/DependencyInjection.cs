using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pogers.Core.Services;
using Pogers.Infrastructure.Dip;
using Pogers.Infrastructure.Services;

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

        /// <summary>
        /// Lesende Abfragen fuer die Api
        /// </summary>
        public static IServiceCollection AddAbfragen(this IServiceCollection dienste)
        {
            dienste.AddScoped<IPersonQueryService, PersonQueryService>();

            return dienste;
        }

        /// <summary>
        /// Import aus der DIP-API. Braucht nur der Worker, darum prueft nur er den Key.
        /// </summary>
        public static IServiceCollection AddImport(
            this IServiceCollection dienste, IConfiguration konfiguration)
        {
            dienste.AddOptions<DipApiOptions>()
                .Bind(konfiguration.GetSection("DipApi"))
                .Validate(o => !string.IsNullOrWhiteSpace(o.ApiKey),
                    "DipApi:ApiKey fehlt. Setzen mit: " +
                    "dotnet user-secrets set \"DipApi:ApiKey\" \"<key>\" --project Pogers.Worker")
                .ValidateOnStart();

            dienste.AddHttpClient<IDipApiClient, DipApiClient>();
            dienste.AddScoped<IPersonImportService, PersonImportService>();
            dienste.AddScoped<IVorgangImportService, VorgangImportService>();

            return dienste;
        }
    }
}
