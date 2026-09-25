namespace Pogers.Core.Entities
{
    /// <summary>
    /// Klasse Stimme 
    /// </summary>
    public class Stimme
    {
        public int Id { get; set; }

        public int AbstimmungId { get; set; }
        public Abstimmung Abstimmung { get; set; } = null!;

        public int PersonId { get; set; }
        public Person Person { get; set; } = null!;

        public VoteResult Ergebnis { get; set; }
    }
}
