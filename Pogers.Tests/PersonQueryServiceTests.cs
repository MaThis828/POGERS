using Pogers.Core.Entities;
using Pogers.Infrastructure.Services;

namespace Pogers.Tests
{
    public class PersonQueryServiceTests
    {
        [Fact]
        public async Task Ohne_Filter_kommen_alle_Abgeordneten_der_Wahlperiode()
        {
            var ergebnis = await AbfrageAsync(21, null);

            Assert.Equal(4, ergebnis.Count);
        }

        [Fact]
        public async Task Ergebnis_ist_nach_Nachname_sortiert()
        {
            var ergebnis = await AbfrageAsync(21, null);

            Assert.Equal(new[] { "Klingbeil", "Lovelace", "Merz", "Scholz" }, ergebnis.Select(p => p.Nachname));
        }

        [Fact]
        public async Task Filter_liefert_nur_Mitglieder_dieser_Fraktion()
        {
            var ergebnis = await AbfrageAsync(21, "SPD");

            Assert.Equal(new[] { "Klingbeil", "Scholz" }, ergebnis.Select(p => p.Nachname));
            Assert.All(ergebnis, p => Assert.Equal("SPD", p.Fraktion));
        }

        [Fact]
        public async Task Unbekannte_Fraktion_liefert_leere_Liste()
        {
            Assert.Empty(await AbfrageAsync(21, "Gibtsnicht"));
        }

        [Fact]
        public async Task Liefert_Titel_und_Fraktion_der_Person()
        {
            var merz = Assert.Single(await AbfrageAsync(21, "CDU/CSU"));

            Assert.Equal("Dr.", merz.Titel);
            Assert.Equal("CDU/CSU", merz.Fraktion);
        }

        [Fact]
        public async Task Liefert_die_Fraktion_der_angefragten_Wahlperiode()
        {
            var gysi = Assert.Single(await AbfrageAsync(20, null));

            Assert.Equal("Gysi", gysi.Nachname);
            Assert.Equal("DIE LINKE.", gysi.Fraktion);
        }

        [Fact]
        public async Task Fraktionsfilter_gilt_nur_innerhalb_der_Wahlperiode()
        {
            Assert.Empty(await AbfrageAsync(21, "DIE LINKE."));
        }

        private static async Task<IReadOnlyList<Core.WorkItems.PersonListItem>> AbfrageAsync(int wahlperiode, string? fraktion)
        {
            using var db = new TestDb();
            using (var ctx = db.NewContext())
            {
                var spd = new Fraktion { Name = "SPD" };
                var cdu = new Fraktion { Name = "CDU/CSU" };

                ctx.Personen.AddRange(
                    Person(1, "Olaf", "Scholz", null, Mitglied(21, spd)),
                    Person(2, "Lars", "Klingbeil", null, Mitglied(21, spd)),
                    Person(3, "Friedrich", "Merz", "Dr.", Mitglied(21, cdu)),
                    Person(4, "Ada", "Lovelace", null, Mitglied(21, new Fraktion { Name = "fraktionslos" })),
                    Person(5, "Gregor", "Gysi", null, Mitglied(20, new Fraktion { Name = "DIE LINKE." })));

                await ctx.SaveChangesAsync();
            }

            using var abfrage = db.NewContext();
            return await new PersonQueryService(abfrage).GetPersonenAsync(wahlperiode, fraktion);
        }

        private static Person Person(int id, string vorname, string nachname, string? titel, PersonWahlperiode mitglied) =>
            new() { Id = id, Vorname = vorname, Nachname = nachname, Titel = titel, Wahlperioden = { mitglied } };

        private static PersonWahlperiode Mitglied(int wahlperiode, Fraktion fraktion) =>
            new() { Wahlperiode = wahlperiode, Fraktion = fraktion };
    }
}
