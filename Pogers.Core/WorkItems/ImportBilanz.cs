
namespace Pogers.Core.WorkItems
{
    /// <summary>
    /// Das Ergebnis eines Importlaufs. Zwei Zahlen, damit der Worker melden kann, ob er etwas Neues gefunden hat.
    /// </summary>
    /// <param name="Angelegt"></param>
    /// <param name="Aktualisiert"></param>
    public record ImportBilanz(int Angelegt, int Aktualisiert);
}
