namespace Pogers.Core.Entities
{
    /// <summary>
    /// Klasse PersonWahlperiode 
    /// </summary>
    public class PersonWahlperiode
    {
        public int PersonId { get; set; }
        public Person Person { get; set; } = null!;

        public int Wahlperiode { get; set; }

        public int FraktionId { get; set; }
        public Fraktion Fraktion { get; set; } = null!;
    }
}
