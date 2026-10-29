using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pogers.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Fraktionen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fraktionen", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Personen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Vorname = table.Column<string>(type: "TEXT", nullable: false),
                    Nachname = table.Column<string>(type: "TEXT", nullable: false),
                    Titel = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personen", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vorgaenge",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Titel = table.Column<string>(type: "TEXT", nullable: false),
                    Vorgangstyp = table.Column<string>(type: "TEXT", nullable: true),
                    Wahlperiode = table.Column<int>(type: "INTEGER", nullable: true),
                    Datum = table.Column<string>(type: "TEXT", nullable: true),
                    Beratungsstand = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vorgaenge", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PersonWahlperioden",
                columns: table => new
                {
                    PersonId = table.Column<int>(type: "INTEGER", nullable: false),
                    Wahlperiode = table.Column<int>(type: "INTEGER", nullable: false),
                    FraktionId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonWahlperioden", x => new { x.PersonId, x.Wahlperiode });
                    table.ForeignKey(
                        name: "FK_PersonWahlperioden_Fraktionen_FraktionId",
                        column: x => x.FraktionId,
                        principalTable: "Fraktionen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PersonWahlperioden_Personen_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Personen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Abstimmungen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VorgangId = table.Column<int>(type: "INTEGER", nullable: true),
                    Wahlperiode = table.Column<int>(type: "INTEGER", nullable: false),
                    Sitzungsnr = table.Column<int>(type: "INTEGER", nullable: false),
                    Abstimmnr = table.Column<int>(type: "INTEGER", nullable: false),
                    Titel = table.Column<string>(type: "TEXT", nullable: false),
                    Thema = table.Column<string>(type: "TEXT", nullable: true),
                    Datum = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    QuelleUrl = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Abstimmungen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Abstimmungen_Vorgaenge_VorgangId",
                        column: x => x.VorgangId,
                        principalTable: "Vorgaenge",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Stimmen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AbstimmungId = table.Column<int>(type: "INTEGER", nullable: false),
                    PersonId = table.Column<int>(type: "INTEGER", nullable: false),
                    Ergebnis = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stimmen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stimmen_Abstimmungen_AbstimmungId",
                        column: x => x.AbstimmungId,
                        principalTable: "Abstimmungen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Stimmen_Personen_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Personen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Abstimmungen_VorgangId",
                table: "Abstimmungen",
                column: "VorgangId");

            migrationBuilder.CreateIndex(
                name: "IX_Abstimmungen_Wahlperiode_Sitzungsnr_Abstimmnr",
                table: "Abstimmungen",
                columns: new[] { "Wahlperiode", "Sitzungsnr", "Abstimmnr" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fraktionen_Name",
                table: "Fraktionen",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonWahlperioden_FraktionId",
                table: "PersonWahlperioden",
                column: "FraktionId");

            migrationBuilder.CreateIndex(
                name: "IX_Stimmen_AbstimmungId_PersonId",
                table: "Stimmen",
                columns: new[] { "AbstimmungId", "PersonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stimmen_PersonId",
                table: "Stimmen",
                column: "PersonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonWahlperioden");

            migrationBuilder.DropTable(
                name: "Stimmen");

            migrationBuilder.DropTable(
                name: "Fraktionen");

            migrationBuilder.DropTable(
                name: "Abstimmungen");

            migrationBuilder.DropTable(
                name: "Personen");

            migrationBuilder.DropTable(
                name: "Vorgaenge");
        }
    }
}
