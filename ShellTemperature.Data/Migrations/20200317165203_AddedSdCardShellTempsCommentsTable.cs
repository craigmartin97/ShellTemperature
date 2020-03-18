using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ShellTemperature.Data.Migrations
{
    public partial class AddedSdCardShellTempsCommentsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SdCardShellTemperatureComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CommentId = table.Column<Guid>(nullable: false),
                    ShellTempId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SdCardShellTemperatureComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SdCardShellTemperatureComments_ReadingComments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "ReadingComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SdCardShellTemperatureComments_SdCardShellTemperatures_ShellTempId",
                        column: x => x.ShellTempId,
                        principalTable: "SdCardShellTemperatures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SdCardShellTemperatureComments_CommentId",
                table: "SdCardShellTemperatureComments",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_SdCardShellTemperatureComments_ShellTempId",
                table: "SdCardShellTemperatureComments",
                column: "ShellTempId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SdCardShellTemperatureComments");
        }
    }
}
