using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ShellTemperature.Models.Migrations
{
    public partial class AddedCommentsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShellTemperatureComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Comment = table.Column<string>(nullable: false),
                    ShellTempId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShellTemperatureComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShellTemperatureComments_ShellTemperatures_ShellTempId",
                        column: x => x.ShellTempId,
                        principalTable: "ShellTemperatures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShellTemperatureComments_ShellTempId",
                table: "ShellTemperatureComments",
                column: "ShellTempId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShellTemperatureComments");
        }
    }
}
