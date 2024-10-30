using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URLShortenerAPI.Migrations
{
    /// <inheritdoc />
    public partial class fixdeviceinfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserAgent",
                table: "DeviceInfos",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "OSFamily",
                table: "DeviceInfos",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "BrowserFamily",
                table: "DeviceInfos",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<bool>(
                name: "Client_PreventNull",
                table: "DeviceInfos",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Device_PreventNull",
                table: "DeviceInfos",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OS_PreventNull",
                table: "DeviceInfos",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Client_PreventNull",
                table: "DeviceInfos");

            migrationBuilder.DropColumn(
                name: "Device_PreventNull",
                table: "DeviceInfos");

            migrationBuilder.DropColumn(
                name: "OS_PreventNull",
                table: "DeviceInfos");

            migrationBuilder.AlterColumn<string>(
                name: "UserAgent",
                table: "DeviceInfos",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OSFamily",
                table: "DeviceInfos",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BrowserFamily",
                table: "DeviceInfos",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
