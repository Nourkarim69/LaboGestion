using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AjoutAuditLogEtFonctionnalites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ValidePar",
                table: "Demandes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CommentaireRejet",
                table: "Demandes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ContreAnalyseRequise",
                table: "Demandes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateAssignation",
                table: "Demandes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateBlocage",
                table: "Demandes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LotBloque",
                table: "Demandes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MotifBlocage",
                table: "Demandes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NumeroCertificat",
                table: "Demandes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SlaHeures",
                table: "Demandes",
                type: "int",
                nullable: false,
                defaultValue: 48);

            migrationBuilder.AddColumn<int>(
                name: "TechnicienAssigneParId",
                table: "Demandes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemandeId = table.Column<int>(type: "int", nullable: true),
                    UtilisateurId = table.Column<int>(type: "int", nullable: false),
                    NomUtilisateur = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateAction = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "ContreAnalyseRequise",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "DateAssignation",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "DateBlocage",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "LotBloque",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "MotifBlocage",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "NumeroCertificat",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "SlaHeures",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "TechnicienAssigneParId",
                table: "Demandes");

            migrationBuilder.AlterColumn<int>(
                name: "ValidePar",
                table: "Demandes",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CommentaireRejet",
                table: "Demandes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
