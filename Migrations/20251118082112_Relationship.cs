using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Databasuppgift_2.Migrations
{
    /// <inheritdoc />
    public partial class Relationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryID",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryID",
                table: "Products");

            migrationBuilder.AddColumn<int>(
                name: "FK_Category",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Products_FK_Category",
                table: "Products",
                column: "FK_Category");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_FK_Category",
                table: "Products",
                column: "FK_Category",
                principalTable: "Categories",
                principalColumn: "CategoryID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_FK_Category",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_FK_Category",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "FK_Category",
                table: "Products");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryID",
                table: "Products",
                column: "CategoryID");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryID",
                table: "Products",
                column: "CategoryID",
                principalTable: "Categories",
                principalColumn: "CategoryID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
