using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaizesDoNordeste.Data.Migrations
{
    /// <inheritdoc />
    public partial class OrderEnums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "channel",
                table: "orders",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "orders",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "orders",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "channel", "status" },
                values: new object[] { 0, 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "channel",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "status",
                table: "orders");
        }
    }
}

