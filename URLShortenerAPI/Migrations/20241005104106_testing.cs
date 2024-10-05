using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace URLShortenerAPI.Migrations
{
    /// <inheritdoc />
    public partial class testing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Browser",
                table: "DeviceInfos");

            migrationBuilder.AddColumn<string>(
                name: "Latitude",
                table: "LocationInfos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Longitude",
                table: "LocationInfos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "OS",
                table: "DeviceInfos",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Brand",
                table: "DeviceInfos",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "BotInfo",
                table: "DeviceInfos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientInfo",
                table: "DeviceInfos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Device",
                table: "DeviceInfos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBot",
                table: "DeviceInfos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "DeviceInfos",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Token = table.Column<string>(type: "text", nullable: false),
                    Expires = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Revoked = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "LocationInfos");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "LocationInfos");

            migrationBuilder.DropColumn(
                name: "BotInfo",
                table: "DeviceInfos");

            migrationBuilder.DropColumn(
                name: "ClientInfo",
                table: "DeviceInfos");

            migrationBuilder.DropColumn(
                name: "Device",
                table: "DeviceInfos");

            migrationBuilder.DropColumn(
                name: "IsBot",
                table: "DeviceInfos");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "DeviceInfos");

            migrationBuilder.AlterColumn<string>(
                name: "OS",
                table: "DeviceInfos",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Brand",
                table: "DeviceInfos",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Browser",
                table: "DeviceInfos",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
