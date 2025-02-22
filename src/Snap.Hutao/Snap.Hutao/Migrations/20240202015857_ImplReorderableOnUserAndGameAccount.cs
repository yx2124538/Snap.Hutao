// <auto-generated />
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snap.Hutao.Migrations
{
    /// <inheritdoc />
    public partial class ImplReorderableOnUserAndGameAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_cultivate_entry_level_informations_EntryId",
                table: "cultivate_entry_level_informations");

            migrationBuilder.AddColumn<int>(
                name: "Index",
                table: "users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Index",
                table: "game_accounts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_cultivate_entry_level_informations_EntryId",
                table: "cultivate_entry_level_informations",
                column: "EntryId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_cultivate_entry_level_informations_EntryId",
                table: "cultivate_entry_level_informations");

            migrationBuilder.DropColumn(
                name: "Index",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Index",
                table: "game_accounts");

            migrationBuilder.CreateIndex(
                name: "IX_cultivate_entry_level_informations_EntryId",
                table: "cultivate_entry_level_informations",
                column: "EntryId");
        }
    }
}
