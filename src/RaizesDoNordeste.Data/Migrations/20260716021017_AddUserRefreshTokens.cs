using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaizesDoNordeste.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_refresh_tokens",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    account_id = table.Column<long>(type: "INTEGER", nullable: false),
                    token = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    expires_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    revoked = table.Column<bool>(type: "INTEGER", nullable: false),
                    restaurant_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    active = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_refresh_tokens_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "restaurants",
                keyColumn: "id",
                keyValue: new Guid("9a88024d-2618-4e25-87f5-35217f7a7c8a"),
                column: "email",
                value: "ru@raizesdonordeste.com");

            migrationBuilder.UpdateData(
                table: "restaurants",
                keyColumn: "id",
                keyValue: new Guid("be0b1f01-0d0f-43e6-9575-b1e117ad62cb"),
                column: "email",
                value: "bistro@raizesdonordeste.com");

            migrationBuilder.UpdateData(
                table: "restaurants",
                keyColumn: "id",
                keyValue: new Guid("f02884ad-1725-4fcb-9bb6-cbf0b8f5fef6"),
                column: "email",
                value: "cantina@raizesdonordeste.com");

            migrationBuilder.CreateIndex(
                name: "IX_user_refresh_tokens_account_id",
                table: "user_refresh_tokens",
                column: "account_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_refresh_tokens");

            migrationBuilder.UpdateData(
                table: "restaurants",
                keyColumn: "id",
                keyValue: new Guid("9a88024d-2618-4e25-87f5-35217f7a7c8a"),
                column: "email",
                value: "ru@RaizesDoNordeste.com");

            migrationBuilder.UpdateData(
                table: "restaurants",
                keyColumn: "id",
                keyValue: new Guid("be0b1f01-0d0f-43e6-9575-b1e117ad62cb"),
                column: "email",
                value: "bistro@RaizesDoNordeste.com");

            migrationBuilder.UpdateData(
                table: "restaurants",
                keyColumn: "id",
                keyValue: new Guid("f02884ad-1725-4fcb-9bb6-cbf0b8f5fef6"),
                column: "email",
                value: "cantina@RaizesDoNordeste.com");
        }
    }
}
