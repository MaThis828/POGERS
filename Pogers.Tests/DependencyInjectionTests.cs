using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pogers.Core.Services;
using Pogers.Infrastructure;
using Pogers.Infrastructure.Dip;

namespace Pogers.Tests
{
    /// <summary>
    /// Baut den ServiceProvider mit voller Validierung, damit Lebensdauer-Fehler
    /// hier auffallen und nicht erst zur Laufzeit.
    /// </summary>
    public class DependencyInjectionTests
    {
        private static IConfiguration Konfiguration(string? apiKey = "testkey")
        {
            var werte = new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = "Data Source=:memory:",
                ["DipApi:BaseUrl"] = "https://dip.test/api/v1"
            };
            if (apiKey is not null)
                werte["DipApi:ApiKey"] = apiKey;

            return new ConfigurationBuilder().AddInMemoryCollection(werte).Build();
        }

        private static ServiceProvider Baue(Action<IServiceCollection> registrieren)
        {
            var dienste = new ServiceCollection();
            dienste.AddLogging();
            registrieren(dienste);

            return dienste.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });
        }

        [Fact]
        public void AddPersistence_registriert_den_DbContext()
        {
            using var anbieter = Baue(d => d.AddPersistence(Konfiguration()));

            using var bereich = anbieter.CreateScope();
            Assert.NotNull(bereich.ServiceProvider.GetRequiredService<PogersDbContext>());
        }

        [Fact]
        public void AddImport_registriert_den_Personenimport()
        {
            var konfiguration = Konfiguration();
            using var anbieter = Baue(d => d.AddPersistence(konfiguration).AddImport(konfiguration));

            using var bereich = anbieter.CreateScope();
            Assert.NotNull(bereich.ServiceProvider.GetRequiredService<IPersonImportService>());
            Assert.Equal("https://dip.test/api/v1",
                anbieter.GetRequiredService<IOptions<DipApiOptions>>().Value.BaseUrl);
        }

        [Fact]
        public void AddImport_ohne_Key_meldet_wie_man_ihn_setzt()
        {
            var konfiguration = Konfiguration(apiKey: null);
            using var anbieter = Baue(d => d.AddPersistence(konfiguration).AddImport(konfiguration));

            var fehler = Assert.Throws<OptionsValidationException>(
                () => anbieter.GetRequiredService<IOptions<DipApiOptions>>().Value);

            Assert.Contains("user-secrets", fehler.Message);
        }
    }
}
