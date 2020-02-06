using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ShellTemperature.Models.Migrations
{
    public partial class AddForeignKeyTempToDevices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DeviceId",
                table: "ShellTemperatures",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShellTemperatures_DeviceId",
                table: "ShellTemperatures",
                column: "DeviceId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShellTemperatures_Devices_DeviceId",
                table: "ShellTemperatures",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShellTemperatures_Devices_DeviceId",
                table: "ShellTemperatures");

            migrationBuilder.DropIndex(
                name: "IX_ShellTemperatures_DeviceId",
                table: "ShellTemperatures");

            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "ShellTemperatures");
        }
    }
}
