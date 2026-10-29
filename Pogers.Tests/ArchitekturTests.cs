namespace Pogers.Tests
{
    /// <summary>
    /// Prueft die Abhaengigkeitsregeln an den .csproj-Dateien. Nicht an der Assembly,
    /// weil der Compiler ungenutzte Referenzen dort wegoptimiert.
    /// </summary>
    public class ArchitekturTests
    {
        [Fact]
        public void Core_hat_keine_einzige_PackageReference()
        {
            var csproj = LiesProjekt("Pogers.Core");

            Assert.DoesNotContain("PackageReference", csproj);
        }

        [Fact]
        public void Core_referenziert_kein_anderes_Projekt()
        {
            var csproj = LiesProjekt("Pogers.Core");

            Assert.DoesNotContain("ProjectReference", csproj);
        }

        [Fact]
        public void Api_und_Worker_referenzieren_einander_nicht()
        {
            Assert.DoesNotContain("Pogers.Worker", LiesProjekt("Pogers.Api"));
            Assert.DoesNotContain("Pogers.Api", LiesProjekt("Pogers.Worker"));
        }

        private static string LiesProjekt(string name) =>
            File.ReadAllText(Path.Combine(RepoWurzel(), name, $"{name}.csproj"));

        private static string RepoWurzel()
        {
            var verzeichnis = new DirectoryInfo(AppContext.BaseDirectory);
            while (verzeichnis is not null &&
                   !File.Exists(Path.Combine(verzeichnis.FullName, "Pogers.slnx")))
            {
                verzeichnis = verzeichnis.Parent;
            }

            return verzeichnis?.FullName
                ?? throw new InvalidOperationException("Pogers.slnx nicht gefunden.");
        }
    }
}
