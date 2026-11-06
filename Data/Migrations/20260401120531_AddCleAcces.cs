using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCleAcces : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClesAcces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilialeId = table.Column<int>(type: "int", nullable: true),
                    EstUtilisee = table.Column<bool>(type: "bit", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateExpiration = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreePar = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClesAcces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClesAcces_Filiales_FilialeId",
                        column: x => x.FilialeId,
                        principalTable: "Filiales",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClesAcces_FilialeId",
                table: "ClesAcces",
                column: "FilialeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClesAcces");
        }
    }
}
