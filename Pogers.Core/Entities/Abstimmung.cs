namespace Pogers.Core.Entities
{
    /// <summary>
    /// Klasse Abstimmung 
    /// </summary>
    public class Abstimmung
    {
        public int Id { get; set; }

        public int? VorgangId { get; set; }
        public Vorgang? Vorgang { get; set; }

        public int Wahlperiode { get; set; }
        public int Sitzungsnr { get; set; }
        public int Abstimmnr { get; set; }
        public string Titel { get; set; } = null!;
        public string? Thema { get; set; }
        public DateOnly Datum { get; set; }
        public string? QuelleUrl { get; set; }

        public List<Stimme> Stimmen { get; set; } = new();
    }
}
