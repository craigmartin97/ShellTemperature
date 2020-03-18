using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ShellTemperature.Data.Migrations
{
    public partial class AddedSdCardDataTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SdCardShellTemperatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Temperature = table.Column<double>(nullable: false),
                    RecordedDateTime = table.Column<DateTime>(nullable: false),
                    Latitude = table.Column<float>(nullable: true),
                    Longitude = table.Column<float>(nullable: true),
                    DeviceId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SdCardShellTemperatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SdCardShellTemperatures_DevicesInfo_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "DevicesInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SdCardShellTemperatures_DeviceId",
                table: "SdCardShellTemperatures",
                column: "DeviceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SdCardShellTemperatures");
        }
    }
}
