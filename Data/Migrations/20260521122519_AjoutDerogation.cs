using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AjoutDerogation : Migration
    {
        /// <inheritdoc />  
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Derogation",
                table: "Resultats",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "JustificationDerogation",
                table: "Resultats",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DerogationGlobale",
                table: "Demandes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Derogation",
                table: "Resultats");

            migrationBuilder.DropColumn(
                name: "JustificationDerogation",
                table: "Resultats");

            migrationBuilder.DropColumn(
                name: "DerogationGlobale",
                table: "Demandes");
        }
    }
}
