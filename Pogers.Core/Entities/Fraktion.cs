namespace Pogers.Core.Entities
{
    /// <summary>
    /// Klasse Fraktion 
    /// </summary>
    public class Fraktion
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public List<PersonWahlperiode> Mitglieder { get; set; } = new();
    }
}
