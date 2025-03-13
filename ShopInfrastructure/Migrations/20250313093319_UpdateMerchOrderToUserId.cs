using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMerchOrderToUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "merchorders_buyerid_fkey",
                table: "MerchOrders");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "MerchOrders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_MerchOrders_UserId",
                table: "MerchOrders",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MerchOrders_Buyers_BuyerId",
                table: "MerchOrders",
                column: "BuyerId",
                principalTable: "Buyers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "merchorders_userid_fkey",
                table: "MerchOrders",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MerchOrders_Buyers_BuyerId",
                table: "MerchOrders");

            migrationBuilder.DropForeignKey(
                name: "merchorders_userid_fkey",
                table: "MerchOrders");

            migrationBuilder.DropIndex(
                name: "IX_MerchOrders_UserId",
                table: "MerchOrders");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "MerchOrders");

            migrationBuilder.AddForeignKey(
                name: "merchorders_buyerid_fkey",
                table: "MerchOrders",
                column: "BuyerId",
                principalTable: "Buyers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
