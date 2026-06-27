using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestauranteUni.Data.Migrations
{
    /// <inheritdoc />
    public partial class OrderAndOrderItemsTotalPriceAndQuantityColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "total_price",
                table: "orders",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "quantity",
                table: "order_items",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "order_items",
                keyColumn: "id",
                keyValue: 1L,
                column: "quantity",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "order_items",
                keyColumn: "id",
                keyValue: 2L,
                column: "quantity",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "orders",
                keyColumn: "id",
                keyValue: 1L,
                column: "total_price",
                value: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "total_price",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "quantity",
                table: "order_items");
        }
    }
}
