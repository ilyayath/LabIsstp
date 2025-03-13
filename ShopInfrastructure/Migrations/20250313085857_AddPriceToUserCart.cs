using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceToUserCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "UserCarts",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "UserCarts");
        }
    }
}
