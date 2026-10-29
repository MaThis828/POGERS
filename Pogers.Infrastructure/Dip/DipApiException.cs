namespace Pogers.Infrastructure.Dip
{
    /// <summary>
    /// Fehler der DIP-API mit lesbarer Meldung
    /// </summary>
    public class DipApiException : Exception
    {
        public DipApiException(string nachricht) : base(nachricht) { }
    }
}
