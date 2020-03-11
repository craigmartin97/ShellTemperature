using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ShellTemperature.Data.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DevicesInfo",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DeviceAddress = table.Column<string>(nullable: false),
                    DeviceName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevicesInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShellTemperatures",
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
                    table.PrimaryKey("PK_ShellTemperatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShellTemperatures_DevicesInfo_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "DevicesInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_ShellTemperatures_DeviceId",
                table: "ShellTemperatures",
                column: "DeviceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShellTemperatureComments");

            migrationBuilder.DropTable(
                name: "ShellTemperatures");

            migrationBuilder.DropTable(
                name: "DevicesInfo");
        }
    }
}
