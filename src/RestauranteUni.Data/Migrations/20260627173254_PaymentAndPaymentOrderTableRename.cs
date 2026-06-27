using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestauranteUni.Data.Migrations
{
    /// <inheritdoc />
    public partial class PaymentAndPaymentOrderTableRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentOrders_Payments_payment_id",
                table: "PaymentOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentOrders_orders_order_id",
                table: "PaymentOrders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payments",
                table: "Payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentOrders",
                table: "PaymentOrders");

            migrationBuilder.RenameTable(
                name: "Payments",
                newName: "payment");

            migrationBuilder.RenameTable(
                name: "PaymentOrders",
                newName: "payment_order");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentOrders_payment_id",
                table: "payment_order",
                newName: "IX_payment_order_payment_id");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentOrders_order_id",
                table: "payment_order",
                newName: "IX_payment_order_order_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_payment",
                table: "payment",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_payment_order",
                table: "payment_order",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_payment_order_orders_order_id",
                table: "payment_order",
                column: "order_id",
                principalTable: "orders",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_payment_order_payment_payment_id",
                table: "payment_order",
                column: "payment_id",
                principalTable: "payment",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payment_order_orders_order_id",
                table: "payment_order");

            migrationBuilder.DropForeignKey(
                name: "FK_payment_order_payment_payment_id",
                table: "payment_order");

            migrationBuilder.DropPrimaryKey(
                name: "PK_payment_order",
                table: "payment_order");

            migrationBuilder.DropPrimaryKey(
                name: "PK_payment",
                table: "payment");

            migrationBuilder.RenameTable(
                name: "payment_order",
                newName: "PaymentOrders");

            migrationBuilder.RenameTable(
                name: "payment",
                newName: "Payments");

            migrationBuilder.RenameIndex(
                name: "IX_payment_order_payment_id",
                table: "PaymentOrders",
                newName: "IX_PaymentOrders_payment_id");

            migrationBuilder.RenameIndex(
                name: "IX_payment_order_order_id",
                table: "PaymentOrders",
                newName: "IX_PaymentOrders_order_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentOrders",
                table: "PaymentOrders",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payments",
                table: "Payments",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentOrders_Payments_payment_id",
                table: "PaymentOrders",
                column: "payment_id",
                principalTable: "Payments",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentOrders_orders_order_id",
                table: "PaymentOrders",
                column: "order_id",
                principalTable: "orders",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
