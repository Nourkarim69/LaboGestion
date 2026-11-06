using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFilialeEtDemande2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FilialeId",
                table: "Utilisateurs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Filiales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Adresse = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ville = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telephone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Produit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Filiales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Demandes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Reference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateDemande = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilialeId = table.Column<int>(type: "int", nullable: false),
                    UtilisateurId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Demandes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Demandes_Filiales_FilialeId",
                        column: x => x.FilialeId,
                        principalTable: "Filiales",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Demandes_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_FilialeId",
                table: "Utilisateurs",
                column: "FilialeId");

            migrationBuilder.CreateIndex(
                name: "IX_Demandes_FilialeId",
                table: "Demandes",
                column: "FilialeId");

            migrationBuilder.CreateIndex(
                name: "IX_Demandes_UtilisateurId",
                table: "Demandes",
                column: "UtilisateurId");

            migrationBuilder.AddForeignKey(
                name: "FK_Utilisateurs_Filiales_FilialeId",
                table: "Utilisateurs",
                column: "FilialeId",
                principalTable: "Filiales",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Utilisateurs_Filiales_FilialeId",
                table: "Utilisateurs");

            migrationBuilder.DropTable(
                name: "Demandes");

            migrationBuilder.DropTable(
                name: "Filiales");

            migrationBuilder.DropIndex(
                name: "IX_Utilisateurs_FilialeId",
                table: "Utilisateurs");

            migrationBuilder.DropColumn(
                name: "FilialeId",
                table: "Utilisateurs");
        }
    }
}
