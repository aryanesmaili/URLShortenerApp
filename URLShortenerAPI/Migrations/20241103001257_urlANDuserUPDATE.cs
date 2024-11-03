using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URLShortenerAPI.Migrations
{
    /// <inheritdoc />
    public partial class urlANDuserUPDATE : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ResetCode",
                table: "Users",
                newName: "PasswordResetCode");

            migrationBuilder.AddColumn<string>(
                name: "EmailResetCode",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMonetized",
                table: "URLs",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailResetCode",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsMonetized",
                table: "URLs");

            migrationBuilder.RenameColumn(
                name: "PasswordResetCode",
                table: "Users",
                newName: "ResetCode");
        }
    }
}
