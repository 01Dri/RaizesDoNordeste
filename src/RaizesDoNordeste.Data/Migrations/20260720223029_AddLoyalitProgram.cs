using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaizesDoNordeste.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLoyalitProgram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "loyalit_program",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    active = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    joined_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    leaved_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    points = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    account_id = table.Column<long>(type: "INTEGER", nullable: false),
                    restaurant_id = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loyalit_program", x => x.id);
                    table.ForeignKey(
                        name: "FK_loyalit_program_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_loyalit_program_restaurants_restaurant_id",
                        column: x => x.restaurant_id,
                        principalTable: "restaurants",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "loyalit_program_movements",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    type = table.Column<int>(type: "INTEGER", nullable: false),
                    points = table.Column<int>(type: "INTEGER", nullable: false),
                    loyality_program_id = table.Column<long>(type: "INTEGER", nullable: false),
                    movement_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loyalit_program_movements", x => x.id);
                    table.ForeignKey(
                        name: "FK_loyalit_program_movements_loyalit_program_loyality_program_id",
                        column: x => x.loyality_program_id,
                        principalTable: "loyalit_program",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_loyalit_program_account_id",
                table: "loyalit_program",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_loyalit_program_restaurant_id",
                table: "loyalit_program",
                column: "restaurant_id");

            migrationBuilder.CreateIndex(
                name: "IX_loyalit_program_movements_loyality_program_id",
                table: "loyalit_program_movements",
                column: "loyality_program_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "loyalit_program_movements");

            migrationBuilder.DropTable(
                name: "loyalit_program");
        }
    }
}
