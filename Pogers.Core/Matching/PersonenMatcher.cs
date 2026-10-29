using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Pogers.Core.Matching
{
    /// <summary>
    /// Ordnet Namen aus den Abstimmungslisten des Bundestags den importierten Abgeordneten zu.
    /// Beide Quellen schreiben Namen und Fraktionen unterschiedlich.
    /// </summary>
    public class PersonenMatcher
    {
        // "Heil (Peine)": Wahlkreis, mit dem die Abstimmungsliste Gleichnamige unterscheidet
        private static readonly Regex Ortszusatz = new(@"\s*\([^)]*\)\s*$", RegexOptions.Compiled);

        // Bleibt erhalten, damit aus ae/oe/ue nicht a/o/u wird
        private const char Trema = '̈';

        // Kurzformen der Abstimmungslisten, normalisiert
        private static readonly Dictionary<string, string> FraktionsKurzform = new()
        {
            [Normalisiere("BÜ90/GR")] = Normalisiere("BÜNDNIS 90/DIE GRÜNEN")
        };

        private readonly ILookup<(string Nachname, string Vorname), PersonKandidat> _index;

        public PersonenMatcher(IEnumerable<PersonKandidat> kandidaten) =>
            _index = kandidaten.ToLookup(k => (Normalisiere(k.Nachname), Normalisiere(k.Vorname)));

        /// <summary>
        /// Ohne Ortszusatz, kleingeschrieben, ohne Diakritika ausser dem Trema ("Özoğuz" wird "özoguz")
        /// </summary>
        public static string Normalisiere(string name)
        {
            var ohneOrt = Ortszusatz.Replace(name.Trim(), string.Empty).Trim();
            var zerlegt = ohneOrt.Normalize(NormalizationForm.FormD);

            var aufgebaut = new StringBuilder(zerlegt.Length);
            foreach (var zeichen in zerlegt)
            {
                var istDiakritikum = CharUnicodeInfo.GetUnicodeCategory(zeichen) == UnicodeCategory.NonSpacingMark;
                if (!istDiakritikum || zeichen == Trema)
                    aufgebaut.Append(zeichen);
            }

            var ergebnis = Regex.Replace(aufgebaut.ToString(), @"\s+", " ");
            return ergebnis.Normalize(NormalizationForm.FormC).ToLowerInvariant();
        }

        /// <summary>
        /// Fraktion in vergleichbarer Form, Kurzformen wie "BÜ90/GR" werden aufgeloest
        /// </summary>
        public static string NormalisiereFraktion(string fraktion)
        {
            var normalisiert = Normalisiere(fraktion);
            return FraktionsKurzform.GetValueOrDefault(normalisiert, normalisiert);
        }

        /// <summary>
        /// Id des Abgeordneten oder null, wenn er nicht eindeutig bestimmbar ist
        /// </summary>
        public int? FindePersonId(string nachname, string vorname, string? fraktion)
        {
            var treffer = _index[(Normalisiere(nachname), Normalisiere(vorname))].ToList();

            if (treffer.Count == 0)
                return null;
            if (treffer.Count == 1)
                return treffer[0].PersonId;

            if (!string.IsNullOrWhiteSpace(fraktion))
            {
                var gesucht = NormalisiereFraktion(fraktion);
                var passend = treffer
                    .Where(k => k.Fraktion is not null && NormalisiereFraktion(k.Fraktion) == gesucht)
                    .ToList();
                if (passend.Count == 1)
                    return passend[0].PersonId;
            }

            // Hoechstens eine Fraktion: dieselbe Person mit mehreren DIP-Ids, die kleinste gewinnt
            var fraktionen = treffer
                .Where(k => k.Fraktion is not null)
                .Select(k => NormalisiereFraktion(k.Fraktion!))
                .Distinct()
                .Count();

            // Sonst verschiedene Personen: lieber keine Stimme als eine falsche
            return fraktionen <= 1 ? treffer.Min(k => k.PersonId) : null;
        }
    }
}
