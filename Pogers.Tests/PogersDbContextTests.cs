using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Pogers.Core.Entities;

namespace Pogers.Tests
{
    public class PogersDbContextTests
    {
        [Fact]
        public void Migrationen_passen_zum_Modell()
        {
            using var db = new TestDb();
            using var ctx = db.NewContext();

            var snapshot = ctx.GetService<IMigrationsAssembly>().ModelSnapshot!.Model;
            if (snapshot is IMutableModel veraenderbar)
                snapshot = veraenderbar.FinalizeModel();
            snapshot = ctx.GetService<IModelRuntimeInitializer>().Initialize(snapshot);

            var unterschiede = ctx.GetService<IMigrationsModelDiffer>().GetDifferences(
                snapshot.GetRelationalModel(),
                ctx.GetService<IDesignTimeModel>().Model.GetRelationalModel());

            Assert.Empty(unterschiede);
        }

        [Fact]
        public async Task Person_behaelt_die_vorgegebene_Id()
        {
            using var db = new TestDb();
            using (var ctx = db.NewContext())
            {
                ctx.Personen.Add(Person(7411, "Friedrich", "Merz"));
                await ctx.SaveChangesAsync();
            }

            using var pruef = db.NewContext();
            Assert.Equal(7411, (await pruef.Personen.SingleAsync()).Id);
        }

        [Fact]
        public async Task Vorgang_behaelt_die_vorgegebene_Id()
        {
            using var db = new TestDb();
            using (var ctx = db.NewContext())
            {
                ctx.Vorgaenge.Add(new Vorgang { Id = 312345, Titel = "Sportfoerdergesetz" });
                await ctx.SaveChangesAsync();
            }

            using var pruef = db.NewContext();
            Assert.Equal(312345, (await pruef.Vorgaenge.SingleAsync()).Id);
        }

        [Fact]
        public async Task Fraktionsname_ist_eindeutig()
        {
            using var db = new TestDb();
            using var ctx = db.NewContext();
            ctx.Fraktionen.Add(new Fraktion { Name = "SPD" });
            ctx.Fraktionen.Add(new Fraktion { Name = "SPD" });

            await Assert.ThrowsAsync<DbUpdateException>(() => ctx.SaveChangesAsync());
        }

        [Fact]
        public async Task Person_hat_je_Wahlperiode_ihre_eigene_Fraktion()
        {
            using var db = new TestDb();
            using (var ctx = db.NewContext())
            {
                var person = Person(1, "Sahra", "Wagenknecht");
                person.Wahlperioden.Add(new PersonWahlperiode
                    { Wahlperiode = 20, Fraktion = new Fraktion { Name = "DIE LINKE." } });
                person.Wahlperioden.Add(new PersonWahlperiode
                    { Wahlperiode = 21, Fraktion = new Fraktion { Name = "BSW" } });
                ctx.Personen.Add(person);
                await ctx.SaveChangesAsync();
            }

            using var pruef = db.NewContext();
            var fraktionen = await pruef.PersonWahlperioden
                .OrderBy(pw => pw.Wahlperiode)
                .Select(pw => pw.Fraktion!.Name)
                .ToListAsync();

            Assert.Equal(new[] { "DIE LINKE.", "BSW" }, fraktionen);
        }

        [Fact]
        public async Task Pro_Person_und_Wahlperiode_gibt_es_nur_eine_Mitgliedschaft()
        {
            using var db = new TestDb();
            using (var ctx = db.NewContext())
            {
                var person = Person(1, "Olaf", "Scholz");
                person.Wahlperioden.Add(new PersonWahlperiode
                    { Wahlperiode = 21, Fraktion = new Fraktion { Name = "SPD" } });
                ctx.Personen.Add(person);
                await ctx.SaveChangesAsync();
            }

            using var zweiter = db.NewContext();
            var spd = await zweiter.Fraktionen.SingleAsync();
            zweiter.PersonWahlperioden.Add(
                new PersonWahlperiode { PersonId = 1, Wahlperiode = 21, FraktionId = spd.Id });

            await Assert.ThrowsAsync<DbUpdateException>(() => zweiter.SaveChangesAsync());
        }

        [Fact]
        public async Task Abstimmung_ist_je_Wahlperiode_Sitzung_und_Nummer_eindeutig()
        {
            using var db = new TestDb();
            using var ctx = db.NewContext();
            ctx.Abstimmungen.Add(Abstimmung(21, 90, 1));
            ctx.Abstimmungen.Add(Abstimmung(21, 90, 1));

            await Assert.ThrowsAsync<DbUpdateException>(() => ctx.SaveChangesAsync());
        }

        [Fact]
        public async Task Person_stimmt_je_Abstimmung_nur_einmal()
        {
            using var db = new TestDb();
            using var ctx = db.NewContext();
            var person = Person(1, "Olaf", "Scholz");
            var abstimmung = Abstimmung(21, 90, 1);
            ctx.Stimmen.Add(new Stimme { Person = person, Abstimmung = abstimmung, Ergebnis = VoteResult.Ja });
            ctx.Stimmen.Add(new Stimme { Person = person, Abstimmung = abstimmung, Ergebnis = VoteResult.Nein });

            await Assert.ThrowsAsync<DbUpdateException>(() => ctx.SaveChangesAsync());
        }

        [Fact]
        public async Task Ergebnis_wird_als_Text_gespeichert()
        {
            using var db = new TestDb();
            using (var ctx = db.NewContext())
            {
                ctx.Stimmen.Add(new Stimme
                {
                    Person = Person(1, "Olaf", "Scholz"),
                    Abstimmung = Abstimmung(21, 90, 1),
                    Ergebnis = VoteResult.Enthaltung
                });
                await ctx.SaveChangesAsync();
            }

            using var pruef = db.NewContext();
            var roh = await pruef.Database
                .SqlQueryRaw<string>("SELECT Ergebnis AS Value FROM Stimmen")
                .SingleAsync();

            Assert.Equal("Enthaltung", roh);
        }

        [Fact]
        public async Task Geloeschter_Vorgang_laesst_die_Abstimmung_bestehen()
        {
            using var db = new TestDb();
            using (var ctx = db.NewContext())
            {
                var abstimmung = Abstimmung(21, 90, 1);
                abstimmung.Vorgang = new Vorgang { Id = 1, Titel = "Sportfoerdergesetz" };
                ctx.Abstimmungen.Add(abstimmung);
                await ctx.SaveChangesAsync();
            }

            using (var ctx = db.NewContext())
                await ctx.Vorgaenge.ExecuteDeleteAsync();

            using var pruef = db.NewContext();
            var uebrig = await pruef.Abstimmungen.SingleAsync();
            Assert.Null(uebrig.VorgangId);
        }

        private static Person Person(int id, string vorname, string nachname) =>
            new() { Id = id, Vorname = vorname, Nachname = nachname };

        private static Abstimmung Abstimmung(int wahlperiode, int sitzung, int nummer) =>
            new()
            {
                Wahlperiode = wahlperiode,
                Sitzungsnr = sitzung,
                Abstimmnr = nummer,
                Titel = "Testabstimmung",
                Datum = new DateOnly(2026, 7, 10)
            };
    }
}
