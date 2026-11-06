using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AjoutWorkflowComplet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CommentaireRejetDirecteur",
                table: "Demandes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommentaireReouvrir",
                table: "Demandes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateReouverte",
                table: "Demandes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateValidationDirecteur",
                table: "Demandes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateValidationResponsable",
                table: "Demandes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroDeLot",
                table: "Demandes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ValideParDirecteurId",
                table: "Demandes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ValideParResponsableId",
                table: "Demandes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Demandes_ValideParDirecteurId",
                table: "Demandes",
                column: "ValideParDirecteurId");

            migrationBuilder.CreateIndex(
                name: "IX_Demandes_ValideParResponsableId",
                table: "Demandes",
                column: "ValideParResponsableId");

            migrationBuilder.AddForeignKey(
                name: "FK_Demandes_Utilisateurs_ValideParDirecteurId",
                table: "Demandes",
                column: "ValideParDirecteurId",
                principalTable: "Utilisateurs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Demandes_Utilisateurs_ValideParResponsableId",
                table: "Demandes",
                column: "ValideParResponsableId",
                principalTable: "Utilisateurs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Demandes_Utilisateurs_ValideParDirecteurId",
                table: "Demandes");

            migrationBuilder.DropForeignKey(
                name: "FK_Demandes_Utilisateurs_ValideParResponsableId",
                table: "Demandes");

            migrationBuilder.DropIndex(
                name: "IX_Demandes_ValideParDirecteurId",
                table: "Demandes");

            migrationBuilder.DropIndex(
                name: "IX_Demandes_ValideParResponsableId",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "CommentaireRejetDirecteur",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "CommentaireReouvrir",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "DateReouverte",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "DateValidationDirecteur",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "DateValidationResponsable",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "NumeroDeLot",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "ValideParDirecteurId",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "ValideParResponsableId",
                table: "Demandes");
        }
    }
}
