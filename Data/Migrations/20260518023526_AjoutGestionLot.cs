using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AjoutGestionLot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CommentaireLiberationLot",
                table: "Demandes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateBlocageLot",
                table: "Demandes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateLiberationLot",
                table: "Demandes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotifBlocageLot",
                table: "Demandes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatutLot",
                table: "Demandes",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommentaireLiberationLot",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "DateBlocageLot",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "DateLiberationLot",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "MotifBlocageLot",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "StatutLot",
                table: "Demandes");
        }
    }
}
