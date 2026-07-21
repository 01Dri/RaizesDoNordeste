using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaizesDoNordeste.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLoyalitProgramUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_loyalit_program_accounts_account_id",
                table: "loyalit_program");

            migrationBuilder.DropForeignKey(
                name: "FK_loyalit_program_restaurants_restaurant_id",
                table: "loyalit_program");

            migrationBuilder.DropForeignKey(
                name: "FK_loyalit_program_movements_loyalit_program_loyality_program_id",
                table: "loyalit_program_movements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_loyalit_program",
                table: "loyalit_program");

            migrationBuilder.DropIndex(
                name: "IX_loyalit_program_account_id",
                table: "loyalit_program");

            migrationBuilder.RenameTable(
                name: "loyalit_program",
                newName: "loyalit_programs");

            migrationBuilder.RenameIndex(
                name: "IX_loyalit_program_restaurant_id",
                table: "loyalit_programs",
                newName: "IX_loyalit_programs_restaurant_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_loyalit_programs",
                table: "loyalit_programs",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_loyalit_programs_account_restaurant",
                table: "loyalit_programs",
                columns: new[] { "account_id", "restaurant_id" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_loyalit_program_movements_loyalit_programs_loyality_program_id",
                table: "loyalit_program_movements",
                column: "loyality_program_id",
                principalTable: "loyalit_programs",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_loyalit_programs_accounts_account_id",
                table: "loyalit_programs",
                column: "account_id",
                principalTable: "accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_loyalit_programs_restaurants_restaurant_id",
                table: "loyalit_programs",
                column: "restaurant_id",
                principalTable: "restaurants",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_loyalit_program_movements_loyalit_programs_loyality_program_id",
                table: "loyalit_program_movements");

            migrationBuilder.DropForeignKey(
                name: "FK_loyalit_programs_accounts_account_id",
                table: "loyalit_programs");

            migrationBuilder.DropForeignKey(
                name: "FK_loyalit_programs_restaurants_restaurant_id",
                table: "loyalit_programs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_loyalit_programs",
                table: "loyalit_programs");

            migrationBuilder.DropIndex(
                name: "ix_loyalit_programs_account_restaurant",
                table: "loyalit_programs");

            migrationBuilder.RenameTable(
                name: "loyalit_programs",
                newName: "loyalit_program");

            migrationBuilder.RenameIndex(
                name: "IX_loyalit_programs_restaurant_id",
                table: "loyalit_program",
                newName: "IX_loyalit_program_restaurant_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_loyalit_program",
                table: "loyalit_program",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_loyalit_program_account_id",
                table: "loyalit_program",
                column: "account_id");

            migrationBuilder.AddForeignKey(
                name: "FK_loyalit_program_accounts_account_id",
                table: "loyalit_program",
                column: "account_id",
                principalTable: "accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_loyalit_program_restaurants_restaurant_id",
                table: "loyalit_program",
                column: "restaurant_id",
                principalTable: "restaurants",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_loyalit_program_movements_loyalit_program_loyality_program_id",
                table: "loyalit_program_movements",
                column: "loyality_program_id",
                principalTable: "loyalit_program",
                principalColumn: "id");
        }
    }
}
