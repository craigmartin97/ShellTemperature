using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ShellTemperature.Data.Migrations
{
    public partial class AddCommentsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comment",
                table: "ShellTemperatureComments");

            migrationBuilder.AddColumn<Guid>(
                name: "CommentId",
                table: "ShellTemperatureComments",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ReadingComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Comment = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReadingComments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShellTemperatureComments_CommentId",
                table: "ShellTemperatureComments",
                column: "CommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShellTemperatureComments_ReadingComments_CommentId",
                table: "ShellTemperatureComments",
                column: "CommentId",
                principalTable: "ReadingComments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShellTemperatureComments_ReadingComments_CommentId",
                table: "ShellTemperatureComments");

            migrationBuilder.DropTable(
                name: "ReadingComments");

            migrationBuilder.DropIndex(
                name: "IX_ShellTemperatureComments_CommentId",
                table: "ShellTemperatureComments");

            migrationBuilder.DropColumn(
                name: "CommentId",
                table: "ShellTemperatureComments");

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "ShellTemperatureComments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
