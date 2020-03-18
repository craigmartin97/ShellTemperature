using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ShellTemperature.Data.Migrations
{
    public partial class EditedColName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SdCardShellTemperatureComments_SdCardShellTemperatures_ShellTempId",
                table: "SdCardShellTemperatureComments");

            migrationBuilder.DropIndex(
                name: "IX_SdCardShellTemperatureComments_ShellTempId",
                table: "SdCardShellTemperatureComments");

            migrationBuilder.DropColumn(
                name: "ShellTempId",
                table: "SdCardShellTemperatureComments");

            migrationBuilder.AddColumn<Guid>(
                name: "SdCardShellTempId",
                table: "SdCardShellTemperatureComments",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_SdCardShellTemperatureComments_SdCardShellTempId",
                table: "SdCardShellTemperatureComments",
                column: "SdCardShellTempId");

            migrationBuilder.AddForeignKey(
                name: "FK_SdCardShellTemperatureComments_SdCardShellTemperatures_SdCardShellTempId",
                table: "SdCardShellTemperatureComments",
                column: "SdCardShellTempId",
                principalTable: "SdCardShellTemperatures",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SdCardShellTemperatureComments_SdCardShellTemperatures_SdCardShellTempId",
                table: "SdCardShellTemperatureComments");

            migrationBuilder.DropIndex(
                name: "IX_SdCardShellTemperatureComments_SdCardShellTempId",
                table: "SdCardShellTemperatureComments");

            migrationBuilder.DropColumn(
                name: "SdCardShellTempId",
                table: "SdCardShellTemperatureComments");

            migrationBuilder.AddColumn<Guid>(
                name: "ShellTempId",
                table: "SdCardShellTemperatureComments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_SdCardShellTemperatureComments_ShellTempId",
                table: "SdCardShellTemperatureComments",
                column: "ShellTempId");

            migrationBuilder.AddForeignKey(
                name: "FK_SdCardShellTemperatureComments_SdCardShellTemperatures_ShellTempId",
                table: "SdCardShellTemperatureComments",
                column: "ShellTempId",
                principalTable: "SdCardShellTemperatures",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
