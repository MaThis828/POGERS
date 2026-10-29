using Microsoft.EntityFrameworkCore;
using Pogers.Core.Entities;
using Pogers.Core.WorkItems;
using Pogers.Infrastructure.Services;

namespace Pogers.Tests
{
    public class VorgangImportServiceTests
    {
        [Fact]
        public async Task Legt_Vorgaenge_mit_allen_Feldern_an()
        {
            using var db = new TestDb();

            var bilanz = await ImportiereAsync(db, new FakeHttpMessageHandler().RespondWithJson(TestFactory.Seite(null,
                TestFactory.Vorgang(310123, "Sportfoerdergesetz", "Gesetzgebung", "Dem Bundesrat zugeleitet"))));

            Assert.Equal(new ImportBilanz(1, 0), bilanz);
            using var pruef = db.NewContext();
            var vorgang = await pruef.Vorgaenge.SingleAsync();
            Assert.Equal(310123, vorgang.Id);
            Assert.Equal("Sportfoerdergesetz", vorgang.Titel);
            Assert.Equal("Gesetzgebung", vorgang.Vorgangstyp);
            Assert.Equal(21, vorgang.Wahlperiode);
            Assert.Equal("2026-07-10", vorgang.Datum);
            Assert.Equal("Dem Bundesrat zugeleitet", vorgang.Beratungsstand);
        }

        [Fact]
        public async Task Folgt_dem_Cursor_ueber_mehrere_Seiten()
        {
            using var db = new TestDb();

            await ImportiereAsync(db, new FakeHttpMessageHandler()
                .RespondWithJson(TestFactory.Seite("seite2", TestFactory.Vorgang(1, "Erster")))
                .RespondWithJson(TestFactory.Seite("seite2", TestFactory.Vorgang(2, "Zweiter"))));

            using var pruef = db.NewContext();
            Assert.Equal(2, await pruef.Vorgaenge.CountAsync());
        }

        [Fact]
        public async Task Zweiter_Lauf_aktualisiert_den_Beratungsstand_statt_zu_duplizieren()
        {
            using var db = new TestDb();

            await ImportiereAsync(db, new FakeHttpMessageHandler().RespondWithJson(TestFactory.Seite(null,
                TestFactory.Vorgang(1, "Sportfoerdergesetz", beratungsstand: "Noch nicht beraten"))));
            var zweite = await ImportiereAsync(db, new FakeHttpMessageHandler().RespondWithJson(TestFactory.Seite(null,
                TestFactory.Vorgang(1, "Sportfoerdergesetz", beratungsstand: "Verabschiedet"))));

            Assert.Equal(new ImportBilanz(0, 1), zweite);
            using var pruef = db.NewContext();
            Assert.Equal("Verabschiedet", (await pruef.Vorgaenge.SingleAsync()).Beratungsstand);
        }

        [Fact]
        public async Task Aktualisiert_auch_Vorgaenge_einer_anderen_Wahlperiode()
        {
            using var db = new TestDb();
            using (var ctx = db.NewContext())
            {
                ctx.Vorgaenge.Add(new Vorgang { Id = 1, Titel = "Alt", Wahlperiode = 20 });
                await ctx.SaveChangesAsync();
            }

            await ImportiereAsync(db, new FakeHttpMessageHandler().RespondWithJson(TestFactory.Seite(null,
                TestFactory.Vorgang(1, "Neu"))));

            using var pruef = db.NewContext();
            var vorgang = await pruef.Vorgaenge.SingleAsync();
            Assert.Equal(21, vorgang.Wahlperiode);
            Assert.Equal("Neu", vorgang.Titel);
        }

        [Fact]
        public async Task Fragt_den_Endpunkt_vorgang_mit_der_Wahlperiode_ab()
        {
            using var db = new TestDb();
            var handler = new FakeHttpMessageHandler().RespondWithJson(TestFactory.Seite(null));

            await ImportiereAsync(db, handler, wahlperiode: 20);

            Assert.Contains("/vorgang?f.wahlperiode=20", handler.AngefragteUrls[0]);
        }

        private static async Task<ImportBilanz> ImportiereAsync(
            TestDb db, FakeHttpMessageHandler handler, int wahlperiode = 21)
        {
            using var ctx = db.NewContext();
            var dienst = new VorgangImportService(
                TestFactory.DipClient(handler.CreateClient()), ctx, TestFactory.Logger<VorgangImportService>());

            return await dienst.ImportAsync(wahlperiode);
        }
    }
}
