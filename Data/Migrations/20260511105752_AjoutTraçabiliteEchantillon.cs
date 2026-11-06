using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.ComponentModel.DataAnnotations.Schema;


#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AjoutTraçabiliteEchantillon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConditionConservation",
                table: "Echantillons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateReception",
                table: "Echantillons",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DelaiTransportHeures",
                table: "Echantillons",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DemandeId",
                table: "Echantillons",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroLot",
                table: "Echantillons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Observations",
                table: "Echantillons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PrelevePar",
                table: "Echantillons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReceptionPar",
                table: "Echantillons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "TemperatureTransport",
                table: "Echantillons",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConditionConservation",
                table: "Echantillons");

            migrationBuilder.DropColumn(
                name: "DateReception",
                table: "Echantillons");

            migrationBuilder.DropColumn(
                name: "DelaiTransportHeures",
                table: "Echantillons");

            migrationBuilder.DropColumn(
                name: "DemandeId",
                table: "Echantillons");

            migrationBuilder.DropColumn(
                name: "NumeroLot",
                table: "Echantillons");

            migrationBuilder.DropColumn(
                name: "Observations",
                table: "Echantillons");

            migrationBuilder.DropColumn(
                name: "PrelevePar",
                table: "Echantillons");

            migrationBuilder.DropColumn(
                name: "ReceptionPar",
                table: "Echantillons");

            migrationBuilder.DropColumn(
                name: "TemperatureTransport",
                table: "Echantillons");

        }
        [Column(TypeName = "decimal(5,2)")]
        public decimal? TemperatureTransport { get; set; }
    }
}
