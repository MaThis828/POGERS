using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pogers.Core.WorkItems
{
    /// <summary>
    /// Flach, ohne Navigation Properties — Fraktion ist hier ein string, kein Objekt. Keine Zirkelbezüge
    /// </summary>
    public record PersonListItem(
        int Id, string Vorname, string Nachname, string? Titel, string? Fraktion);
}
