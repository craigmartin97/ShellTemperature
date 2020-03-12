using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ShellTemperature.Data.Migrations
{
    public partial class AddShellTempsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShellTemperaturePositions",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ShellTempId = table.Column<Guid>(nullable: false),
                    PositionId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShellTemperaturePositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShellTemperaturePositions_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShellTemperaturePositions_ShellTemperatures_ShellTempId",
                        column: x => x.ShellTempId,
                        principalTable: "ShellTemperatures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShellTemperaturePositions_PositionId",
                table: "ShellTemperaturePositions",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_ShellTemperaturePositions_ShellTempId",
                table: "ShellTemperaturePositions",
                column: "ShellTempId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShellTemperaturePositions");
        }
    }
}
