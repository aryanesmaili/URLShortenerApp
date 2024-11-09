using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace URLShortenerAPI.Migrations
{
    /// <inheritdoc />
    public partial class payment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FinancialID",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PurchaseID",
                table: "URLs",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FinancialRecords",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialRecords", x => x.ID);
                    table.ForeignKey(
                        name: "FK_FinancialRecords_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Deposits",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Amount = table.Column<double>(type: "double precision", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsSuccessful = table.Column<bool>(type: "boolean", nullable: false),
                    FinanceID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deposits", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Deposits_FinancialRecords_FinanceID",
                        column: x => x.FinanceID,
                        principalTable: "FinancialRecords",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Purchases",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Amount = table.Column<double>(type: "double precision", nullable: false),
                    CustomURLID = table.Column<int>(type: "integer", nullable: false),
                    FinanceID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purchases", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Purchases_FinancialRecords_FinanceID",
                        column: x => x.FinanceID,
                        principalTable: "FinancialRecords",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Purchases_URLs_CustomURLID",
                        column: x => x.CustomURLID,
                        principalTable: "URLs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_FinancialID",
                table: "Users",
                column: "FinancialID");

            migrationBuilder.CreateIndex(
                name: "IX_Deposits_FinanceID",
                table: "Deposits",
                column: "FinanceID");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialRecords_UserID",
                table: "FinancialRecords",
                column: "UserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_CustomURLID",
                table: "Purchases",
                column: "CustomURLID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_FinanceID",
                table: "Purchases",
                column: "FinanceID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Deposits");

            migrationBuilder.DropTable(
                name: "Purchases");

            migrationBuilder.DropTable(
                name: "FinancialRecords");

            migrationBuilder.DropIndex(
                name: "IX_Users_FinancialID",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FinancialID",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PurchaseID",
                table: "URLs");
        }
    }
}
