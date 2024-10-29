using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URLShortenerAPI.Migrations
{
    /// <inheritdoc />
    public partial class deviceinfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBot",
                table: "DeviceInfos");

            migrationBuilder.RenameColumn(
                name: "Model",
                table: "DeviceInfos",
                newName: "Device_Model");

            migrationBuilder.RenameColumn(
                name: "Brand",
                table: "DeviceInfos",
                newName: "Device_Brand");

            migrationBuilder.RenameColumn(
                name: "OS",
                table: "DeviceInfos",
                newName: "OS_Version");

            migrationBuilder.RenameColumn(
                name: "ClientInfo",
                table: "DeviceInfos",
                newName: "OS_Platform");

            migrationBuilder.RenameColumn(
                name: "BotInfo",
                table: "DeviceInfos",
                newName: "OS_Name");

            migrationBuilder.AddColumn<string>(
                name: "BrowserFamily",
                table: "DeviceInfos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Client_Engine",
                table: "DeviceInfos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Client_EngineVersion",
                table: "DeviceInfos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Client_Name",
                table: "DeviceInfos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Client_Type",
                table: "DeviceInfos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Client_Version",
                table: "DeviceInfos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Device_Type",
                table: "DeviceInfos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OSFamily",
                table: "DeviceInfos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "DeviceInfos",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BrowserFamily",
                table: "DeviceInfos");

            migrationBuilder.DropColumn(
                name: "Client_Engine",
                table: "DeviceInfos");

            migrationBuilder.DropColumn(
                name: "Client_EngineVersion",
                table: "DeviceInfos");

            migrationBuilder.DropColumn(
                name: "Client_Name",
                table: "DeviceInfos");

            migrationBuilder.DropColumn(
                name: "Client_Type",
                table: "DeviceInfos");

            migrationBuilder.DropColumn(
                name: "Client_Version",
                table: "DeviceInfos");

            migrationBuilder.DropColumn(
                name: "Device_Type",
                table: "DeviceInfos");

            migrationBuilder.DropColumn(
                name: "OSFamily",
                table: "DeviceInfos");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "DeviceInfos");

            migrationBuilder.RenameColumn(
                name: "Device_Model",
                table: "DeviceInfos",
                newName: "Model");

            migrationBuilder.RenameColumn(
                name: "Device_Brand",
                table: "DeviceInfos",
                newName: "Brand");

            migrationBuilder.RenameColumn(
                name: "OS_Version",
                table: "DeviceInfos",
                newName: "OS");

            migrationBuilder.RenameColumn(
                name: "OS_Platform",
                table: "DeviceInfos",
                newName: "ClientInfo");

            migrationBuilder.RenameColumn(
                name: "OS_Name",
                table: "DeviceInfos",
                newName: "BotInfo");

            migrationBuilder.AddColumn<bool>(
                name: "IsBot",
                table: "DeviceInfos",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
