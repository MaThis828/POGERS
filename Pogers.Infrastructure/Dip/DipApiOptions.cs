namespace Pogers.Infrastructure.Dip
{
    /// <summary>
    /// Zugangsdaten der DIP-API aus dem Abschnitt "DipApi"
    /// </summary>
    public class DipApiOptions
    {
        public string BaseUrl { get; set; } = "https://search.dip.bundestag.de/api/v1";
        public string ApiKey { get; set; } = null!;
    }
}
