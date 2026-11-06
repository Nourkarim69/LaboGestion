using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AjoutWorkflowApprobation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CommentaireRejet",
                table: "Demandes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateValidation",
                table: "Demandes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ValidePar",
                table: "Demandes",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommentaireRejet",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "DateValidation",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "ValidePar",
                table: "Demandes");
        }
    }
}
