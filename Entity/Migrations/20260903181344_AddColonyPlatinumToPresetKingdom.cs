using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dominex.Entity.Migrations
{
    /// <inheritdoc />
    public partial class AddColonyPlatinumToPresetKingdom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AddColonyAndPlatinum",
                table: "PresetKingdom",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddColonyAndPlatinum",
                table: "PresetKingdom");
        }
    }
}
