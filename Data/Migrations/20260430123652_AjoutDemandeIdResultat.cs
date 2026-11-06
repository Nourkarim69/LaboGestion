using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AjoutDemandeIdResultat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DemandeId",
                table: "Resultats",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Resultats_DemandeId",
                table: "Resultats",
                column: "DemandeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Resultats_Demandes_DemandeId",
                table: "Resultats",
                column: "DemandeId",
                principalTable: "Demandes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Resultats_Demandes_DemandeId",
                table: "Resultats");

            migrationBuilder.DropIndex(
                name: "IX_Resultats_DemandeId",
                table: "Resultats");

            migrationBuilder.DropColumn(
                name: "DemandeId",
                table: "Resultats");
        }
    }
}
