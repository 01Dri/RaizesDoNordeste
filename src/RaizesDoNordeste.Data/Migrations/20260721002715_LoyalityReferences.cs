using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaizesDoNordeste.Data.Migrations
{
    /// <inheritdoc />
    public partial class LoyalityReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_loyalit_programs_restaurant_id",
                table: "loyalit_programs");

            migrationBuilder.AddColumn<long>(
                name: "LoyalityProgramId",
                table: "restaurants",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LoyalitProgramId",
                table: "accounts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "restaurants",
                keyColumn: "id",
                keyValue: new Guid("9a88024d-2618-4e25-87f5-35217f7a7c8a"),
                column: "LoyalityProgramId",
                value: null);

            migrationBuilder.UpdateData(
                table: "restaurants",
                keyColumn: "id",
                keyValue: new Guid("be0b1f01-0d0f-43e6-9575-b1e117ad62cb"),
                column: "LoyalityProgramId",
                value: null);

            migrationBuilder.UpdateData(
                table: "restaurants",
                keyColumn: "id",
                keyValue: new Guid("f02884ad-1725-4fcb-9bb6-cbf0b8f5fef6"),
                column: "LoyalityProgramId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_loyalit_programs_account_id",
                table: "loyalit_programs",
                column: "account_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_loyalit_programs_restaurant_id",
                table: "loyalit_programs",
                column: "restaurant_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_loyalit_programs_account_id",
                table: "loyalit_programs");

            migrationBuilder.DropIndex(
                name: "IX_loyalit_programs_restaurant_id",
                table: "loyalit_programs");

            migrationBuilder.DropColumn(
                name: "LoyalityProgramId",
                table: "restaurants");

            migrationBuilder.DropColumn(
                name: "LoyalitProgramId",
                table: "accounts");

            migrationBuilder.CreateIndex(
                name: "IX_loyalit_programs_restaurant_id",
                table: "loyalit_programs",
                column: "restaurant_id");
        }
    }
}
