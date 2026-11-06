using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDemande : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CommentaireLabo",
                table: "Demandes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateReception",
                table: "Demandes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateRenduEstimee",
                table: "Demandes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateRenduReelle",
                table: "Demandes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NombreEchantillons",
                table: "Demandes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Priorite",
                table: "Demandes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TechnicienId",
                table: "Demandes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TypeProduit",
                table: "Demandes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Demandes_TechnicienId",
                table: "Demandes",
                column: "TechnicienId");

            migrationBuilder.AddForeignKey(
                name: "FK_Demandes_Utilisateurs_TechnicienId",
                table: "Demandes",
                column: "TechnicienId",
                principalTable: "Utilisateurs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Demandes_Utilisateurs_TechnicienId",
                table: "Demandes");

            migrationBuilder.DropIndex(
                name: "IX_Demandes_TechnicienId",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "CommentaireLabo",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "DateReception",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "DateRenduEstimee",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "DateRenduReelle",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "NombreEchantillons",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "Priorite",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "TechnicienId",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "TypeProduit",
                table: "Demandes");
        }
    }
}
