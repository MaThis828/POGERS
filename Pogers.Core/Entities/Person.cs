namespace Pogers.Core.Entities
{
    /// <summary>
    /// Klasse Person 
    /// </summary>
    public class Person
    {
        public int Id { get; set; }    
        public string Vorname { get; set; } = null!;
        public string Nachname { get; set; } = null!;
        public string? Titel { get; set; }

        public List<PersonWahlperiode> Wahlperioden { get; set; } = new();
        public List<Stimme> Stimmen { get; set; } = new();
    }
}

