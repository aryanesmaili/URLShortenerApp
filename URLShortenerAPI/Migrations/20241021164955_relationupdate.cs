using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URLShortenerAPI.Migrations
{
    /// <inheritdoc />
    public partial class relationupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_URLs_URLCategories_CategoryID",
                table: "URLs");

            migrationBuilder.DropIndex(
                name: "IX_URLs_CategoryID",
                table: "URLs");

            migrationBuilder.CreateTable(
                name: "URLCategoryModelURLModel",
                columns: table => new
                {
                    CategoriesID = table.Column<int>(type: "integer", nullable: false),
                    URLsID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_URLCategoryModelURLModel", x => new { x.CategoriesID, x.URLsID });
                    table.ForeignKey(
                        name: "FK_URLCategoryModelURLModel_URLCategories_CategoriesID",
                        column: x => x.CategoriesID,
                        principalTable: "URLCategories",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_URLCategoryModelURLModel_URLs_URLsID",
                        column: x => x.URLsID,
                        principalTable: "URLs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_URLCategoryModelURLModel_URLsID",
                table: "URLCategoryModelURLModel",
                column: "URLsID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "URLCategoryModelURLModel");

            migrationBuilder.CreateIndex(
                name: "IX_URLs_CategoryID",
                table: "URLs",
                column: "CategoryID");

            migrationBuilder.AddForeignKey(
                name: "FK_URLs_URLCategories_CategoryID",
                table: "URLs",
                column: "CategoryID",
                principalTable: "URLCategories",
                principalColumn: "ID");
        }
    }
}
