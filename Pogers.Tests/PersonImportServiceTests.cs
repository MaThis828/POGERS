using Microsoft.EntityFrameworkCore;
using Pogers.Core.WorkItems;
using Pogers.Infrastructure.Dip;
using Pogers.Infrastructure.Services;

namespace Pogers.Tests
{
    public class PersonImportServiceTests
    {
        [Fact]
        public async Task Legt_Personen_mit_DIP_Id_an()
        {
            using var db = new TestDb();

            var bilanz = await ImportiereAsync(db, 21,
                TestFactory.Person(7411, "Friedrich", "Merz", "CDU/CSU"),
                TestFactory.Person(2, "Olaf", "Scholz", "SPD"));

            Assert.Equal(new ImportBilanz(2, 0), bilanz);
            using var pruef = db.NewContext();
            Assert.Equal(new[] { 2, 7411 }, await pruef.Personen.Select(p => p.Id).OrderBy(i => i).ToListAsync());
        }

        [Fact]
        public async Task Uebernimmt_nur_den_akademischen_Titel()
        {
            using var db = new TestDb();

            await ImportiereAsync(db, 21,
                TestFactory.Person(1, "Michael", "Blos", "AfD", titel: "Dr. Michael Blos, MdB, AfD"),
                TestFactory.Person(2, "Olaf", "Scholz", "SPD", titel: "Olaf Scholz, MdB, SPD"));

            using var pruef = db.NewContext();
            var titel = await pruef.Personen.OrderBy(p => p.Id).Select(p => p.Titel).ToListAsync();
            Assert.Equal(new[] { "Dr.", null }, titel);
        }

        [Theory]
        [InlineData("Prof. Dr. Ada Lovelace, MdB, SPD", "Prof. Dr.")]
        [InlineData("Ada Lovelace, MdB, SPD", null)]
        [InlineData(null, null)]
        public void Liest_den_akademischen_Titel_vor_dem_Vornamen(string? dipTitel, string? erwartet)
        {
            var dto = new PersonDto("1", "Ada", "Lovelace", dipTitel, null, null);

            Assert.Equal(erwartet, PersonImportService.LiesAkademischenTitel(dto));
        }

        [Fact]
        public async Task Ueberspringt_Personen_ohne_Fraktion()
        {
            using var db = new TestDb();

            var bilanz = await ImportiereAsync(db, 21,
                TestFactory.Person(1, "Olaf", "Scholz", "SPD"),
                TestFactory.Person(2, "Olaf", "Scholz", fraktion: null));

            Assert.Equal(1, bilanz.Angelegt);
            using var pruef = db.NewContext();
            Assert.Equal(1, (await pruef.Personen.SingleAsync()).Id);
        }

        [Fact]
        public async Task Importiert_Fraktionslose()
        {
            using var db = new TestDb();

            await ImportiereAsync(db, 21, TestFactory.Person(1, "Ada", "Lovelace", "fraktionslos"));

            using var pruef = db.NewContext();
            Assert.Equal("fraktionslos", (await pruef.Fraktionen.SingleAsync()).Name);
        }

        [Fact]
        public async Task Personen_derselben_Fraktion_teilen_eine_Fraktion()
        {
            using var db = new TestDb();

            await ImportiereAsync(db, 21,
                TestFactory.Person(1, "Olaf", "Scholz", "SPD"),
                TestFactory.Person(2, "Lars", "Klingbeil", "SPD"));

            using var pruef = db.NewContext();
            Assert.Equal(1, await pruef.Fraktionen.CountAsync());
            Assert.Equal(2, await pruef.PersonWahlperioden.CountAsync());
        }

        [Fact]
        public async Task Zweiter_Lauf_aktualisiert_statt_zu_duplizieren()
        {
            using var db = new TestDb();

            await ImportiereAsync(db, 21, TestFactory.Person(1, "Angela", "Kasner", "CDU/CSU"));
            var zweite = await ImportiereAsync(db, 21, TestFactory.Person(1, "Angela", "Merkel", "CDU/CSU"));

            Assert.Equal(new ImportBilanz(0, 1), zweite);
            using var pruef = db.NewContext();
            Assert.Equal("Merkel", (await pruef.Personen.SingleAsync()).Nachname);
            Assert.Equal(1, await pruef.Fraktionen.CountAsync());
            Assert.Equal(1, await pruef.PersonWahlperioden.CountAsync());
        }

        [Fact]
        public async Task Fraktionswechsel_laesst_die_fruehere_Wahlperiode_unberuehrt()
        {
            using var db = new TestDb();

            await ImportiereAsync(db, 20, TestFactory.Person(1, "Sahra", "Wagenknecht", "DIE LINKE."));
            await ImportiereAsync(db, 21, TestFactory.Person(1, "Sahra", "Wagenknecht", "BSW"));

            using var pruef = db.NewContext();
            var fraktionen = await pruef.PersonWahlperioden
                .OrderBy(pw => pw.Wahlperiode)
                .Select(pw => pw.Fraktion.Name)
                .ToListAsync();

            Assert.Equal(new[] { "DIE LINKE.", "BSW" }, fraktionen);
        }

        [Fact]
        public async Task Fraktionswechsel_in_derselben_Wahlperiode_wird_uebernommen()
        {
            using var db = new TestDb();

            await ImportiereAsync(db, 21, TestFactory.Person(1, "Ada", "Lovelace", "SPD"));
            await ImportiereAsync(db, 21, TestFactory.Person(1, "Ada", "Lovelace", "fraktionslos"));

            using var pruef = db.NewContext();
            var mitgliedschaft = await pruef.PersonWahlperioden.Include(pw => pw.Fraktion).SingleAsync();
            Assert.Equal("fraktionslos", mitgliedschaft.Fraktion.Name);
        }

        private static async Task<ImportBilanz> ImportiereAsync(TestDb db, int wahlperiode, params string[] personenJson)
        {
            var handler = new FakeHttpMessageHandler()
                .RespondWithJson(TestFactory.PersonSeite(null, personenJson));

            using var ctx = db.NewContext();
            var dienst = new PersonImportService(
                TestFactory.DipClient(handler.CreateClient()), ctx, TestFactory.Logger<PersonImportService>());

            return await dienst.ImportAsync(wahlperiode);
        }
    }
}
