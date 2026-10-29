using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pogers.Infrastructure;

namespace Pogers.Tests
{
    /// <summary>
    /// Baut den ServiceProvider mit voller Validierung, damit Lebensdauer-Fehler
    /// hier auffallen und nicht erst zur Laufzeit.
    /// </summary>
    public class DependencyInjectionTests
    {
        private static IConfiguration Konfiguration() =>
            new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = "Data Source=:memory:"
            }).Build();

        [Fact]
        public void AddPersistence_registriert_den_DbContext()
        {
            var dienste = new ServiceCollection();
            dienste.AddPersistence(Konfiguration());

            using var anbieter = dienste.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

            using var bereich = anbieter.CreateScope();
            Assert.NotNull(bereich.ServiceProvider.GetRequiredService<PogersDbContext>());
        }
    }
}
