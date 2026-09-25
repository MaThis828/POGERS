namespace Pogers.Core.Entities
{
    /// <summary>
    /// Klasse Vorgang 
    /// </summary>
    public class Vorgang
    {
        public int Id { get; set; }               
        public string Titel { get; set; } = null!;
        public string? Vorgangstyp { get; set; }
        public int? Wahlperiode { get; set; }
        public string? Datum { get; set; }    
        public string? Beratungsstand { get; set; }

        public List<Abstimmung> Abstimmungen { get; set; } = new();
    }
}
