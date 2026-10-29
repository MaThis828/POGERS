using Pogers.Core.Matching;

namespace Pogers.Tests
{
    public class PersonenMatcherTests
    {
        [Theory]
        [InlineData("Heil (Peine)", "heil")]
        [InlineData("Müller (Braunschweig)", "müller")]
        [InlineData("  Schneider  (Erfurt) ", "schneider")]
        [InlineData("  MERKEL ", "merkel")]
        [InlineData("Jan  Wenzel", "jan wenzel")]
        public void Normalisiere_entfernt_Ortszusatz_Leerraum_und_Grossschreibung(string eingabe, string erwartet)
        {
            Assert.Equal(erwartet, PersonenMatcher.Normalisiere(eingabe));
        }

        [Theory]
        [InlineData("Özoğuz", "özoguz")]
        [InlineData("Ahmetović", "ahmetovic")]
        [InlineData("Weiß", "weiß")]
        public void Normalisiere_entfernt_Diakritika_aber_keine_Umlaute(string eingabe, string erwartet)
        {
            Assert.Equal(erwartet, PersonenMatcher.Normalisiere(eingabe));
        }

        [Theory]
        [InlineData("BÜ90/GR", "BÜNDNIS 90/DIE GRÜNEN")]
        [InlineData("Die Linke", "DIE LINKE")]
        [InlineData("Fraktionslos", "fraktionslos")]
        [InlineData("CDU/CSU", "CDU/CSU")]
        public void Fraktionen_beider_Quellen_sind_gleich(string abstimmungsliste, string dip)
        {
            Assert.Equal(PersonenMatcher.NormalisiereFraktion(dip), PersonenMatcher.NormalisiereFraktion(abstimmungsliste));
        }

        [Fact]
        public void Findet_den_eindeutigen_Abgeordneten()
        {
            var matcher = Matcher(new PersonKandidat(1, "Lars", "Klingbeil", "SPD"));

            Assert.Equal(1, matcher.FindePersonId("Klingbeil", "Lars", "SPD"));
        }

        [Fact]
        public void Findet_den_Abgeordneten_trotz_Ortszusatz_und_Diakritika()
        {
            var matcher = Matcher(
                new PersonKandidat(1268, "Hubertus", "Heil", "SPD"),
                new PersonKandidat(1185, "Aydan", "Özoguz", "SPD"));

            Assert.Equal(1268, matcher.FindePersonId("Heil (Peine)", "Hubertus", "SPD"));
            Assert.Equal(1185, matcher.FindePersonId("Özoğuz", "Aydan", "SPD"));
        }

        [Fact]
        public void Bei_einem_Treffer_stoert_eine_abweichende_Fraktion_nicht()
        {
            var matcher = Matcher(new PersonKandidat(1, "Ada", "Lovelace", "SPD"));

            Assert.Equal(1, matcher.FindePersonId("Lovelace", "Ada", "Fraktionslos"));
        }

        [Fact]
        public void Unterscheidet_Gleichnamige_ueber_die_Fraktion()
        {
            var matcher = Matcher(
                new PersonKandidat(1, "Michael", "Müller", "SPD"),
                new PersonKandidat(2, "Michael", "Müller", "BÜNDNIS 90/DIE GRÜNEN"));

            Assert.Equal(2, matcher.FindePersonId("Müller (Erlangen)", "Michael", "BÜ90/GR"));
        }

        [Fact]
        public void Ordnet_einen_Fraktionswechsler_mit_zwei_DIP_Ids_ueber_die_Fraktion_zu()
        {
            var matcher = Matcher(
                new PersonKandidat(7406, "Jan Wenzel", "Schmidt", "AfD"),
                new PersonKandidat(8220, "Jan Wenzel", "Schmidt", "fraktionslos"));

            Assert.Equal(7406, matcher.FindePersonId("Schmidt", "Jan Wenzel", "AfD"));
            Assert.Equal(8220, matcher.FindePersonId("Schmidt", "Jan Wenzel", "Fraktionslos"));
        }

        [Fact]
        public void Findet_Regierungsmitglieder_ohne_Fraktion_ueber_den_Namen()
        {
            var matcher = Matcher(new PersonKandidat(8167, "Friedrich", "Merz", null));

            Assert.Equal(8167, matcher.FindePersonId("Merz", "Friedrich", "CDU/CSU"));
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("CDU/CSU", "CDU/CSU")]
        public void Fuehrt_mehrere_DIP_Ids_derselben_Person_zur_kleinsten_zusammen(string? fraktionA, string? fraktionB)
        {
            var matcher = Matcher(
                new PersonKandidat(8253, "Dorothee", "Bär", fraktionA),
                new PersonKandidat(1060, "Dorothee", "Bär", fraktionB));

            Assert.Equal(1060, matcher.FindePersonId("Bär", "Dorothee", "CDU/CSU"));
        }

        [Fact]
        public void Bevorzugt_den_Eintrag_mit_passender_Fraktion()
        {
            var matcher = Matcher(
                new PersonKandidat(1, "Carsten", "Linnemann", null),
                new PersonKandidat(2, "Carsten", "Linnemann", "CDU/CSU"));

            Assert.Equal(2, matcher.FindePersonId("Linnemann", "Carsten", "CDU/CSU"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("CDU/CSU")]
        public void Gleichnamige_ohne_passende_Fraktion_ergeben_keine_Zuordnung(string? fraktion)
        {
            var matcher = Matcher(
                new PersonKandidat(1, "Michael", "Müller", "SPD"),
                new PersonKandidat(2, "Michael", "Müller", "AfD"));

            Assert.Null(matcher.FindePersonId("Müller", "Michael", fraktion));
        }

        [Fact]
        public void Unbekannter_Name_ergibt_keine_Zuordnung()
        {
            var matcher = Matcher(new PersonKandidat(1, "Lars", "Klingbeil", "SPD"));

            Assert.Null(matcher.FindePersonId("Scholz", "Olaf", "SPD"));
        }

        private static PersonenMatcher Matcher(params PersonKandidat[] kandidaten) => new(kandidaten);
    }
}
