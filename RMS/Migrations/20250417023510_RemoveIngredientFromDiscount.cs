using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RMS.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIngredientFromDiscount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Discounts_Ingredients_IngredientId",
                table: "Discounts");

            migrationBuilder.DropIndex(
                name: "IX_Discounts_IngredientId",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "IngredientId",
                table: "Discounts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IngredientId",
                table: "Discounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Discounts_IngredientId",
                table: "Discounts",
                column: "IngredientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Discounts_Ingredients_IngredientId",
                table: "Discounts",
                column: "IngredientId",
                principalTable: "Ingredients",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
