using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dominex.Entity.Migrations
{
    public partial class PresetKingdomUniqueName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "PresetKingdom",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_PresetKingdom_Name",
                table: "PresetKingdom",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PresetKingdom_Name",
                table: "PresetKingdom");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "PresetKingdom",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
