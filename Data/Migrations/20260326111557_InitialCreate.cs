using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Analyses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValeurMin = table.Column<double>(type: "float", nullable: false),
                    ValeurMax = table.Column<double>(type: "float", nullable: false),
                    Unite = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Analyses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Utilisateurs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prenom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MotDePasse = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateurs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Echantillons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Reference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DatePrelevement = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UtilisateurId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Echantillons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Echantillons_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Resultats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Valeur = table.Column<double>(type: "float", nullable: false),
                    EstAnomal = table.Column<bool>(type: "bit", nullable: false),
                    DateSaisie = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EchantillonId = table.Column<int>(type: "int", nullable: false),
                    AnalyseId = table.Column<int>(type: "int", nullable: false),
                    UtilisateurId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resultats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Resultats_Analyses_AnalyseId",
                        column: x => x.AnalyseId,
                        principalTable: "Analyses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Resultats_Echantillons_EchantillonId",
                        column: x => x.EchantillonId,
                        principalTable: "Echantillons",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Resultats_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Echantillons_UtilisateurId",
                table: "Echantillons",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_Resultats_AnalyseId",
                table: "Resultats",
                column: "AnalyseId");

            migrationBuilder.CreateIndex(
                name: "IX_Resultats_EchantillonId",
                table: "Resultats",
                column: "EchantillonId");

            migrationBuilder.CreateIndex(
                name: "IX_Resultats_UtilisateurId",
                table: "Resultats",
                column: "UtilisateurId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Resultats");

            migrationBuilder.DropTable(
                name: "Analyses");

            migrationBuilder.DropTable(
                name: "Echantillons");

            migrationBuilder.DropTable(
                name: "Utilisateurs");
        }
    }
}
