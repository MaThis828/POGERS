using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pogers.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FraktionOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "FraktionId",
                table: "PersonWahlperioden",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Ohne Fraktion gab es vorher nicht, sonst wuerde daraus die ungueltige FraktionId 0
            migrationBuilder.Sql("DELETE FROM PersonWahlperioden WHERE FraktionId IS NULL;");

            migrationBuilder.AlterColumn<int>(
                name: "FraktionId",
                table: "PersonWahlperioden",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}
