using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dominex.Entity.Migrations
{
    /// <inheritdoc />
    public partial class dotnet9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_NormalizedEmail",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_NormalizedUsername",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_CountryLocalization_ParentId",
                table: "CountryLocalization");

            migrationBuilder.DropColumn(
                name: "AccessFailedCount",
                table: "User");

            migrationBuilder.DropColumn(
                name: "EmailConfirmed",
                table: "User");

            migrationBuilder.DropColumn(
                name: "LockoutEnabled",
                table: "User");

            migrationBuilder.DropColumn(
                name: "LockoutEnd",
                table: "User");

            migrationBuilder.DropColumn(
                name: "MigrationId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "NormalizedEmail",
                table: "User");

            migrationBuilder.DropColumn(
                name: "NormalizedUsername",
                table: "User");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "User");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "User");

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "Role");

            migrationBuilder.RenameColumn(
                name: "SecurityStamp",
                table: "User",
                newName: "IdentityProviderExternalId");

            migrationBuilder.CreateIndex(
                name: "IX_User_IdentityProviderExternalId",
                table: "User",
                column: "IdentityProviderExternalId",
                unique: true,
                filter: "Deleted IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CountryLocalization_ParentId_LanguageId",
                table: "CountryLocalization",
                columns: new[] { "ParentId", "LanguageId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_IdentityProviderExternalId",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_CountryLocalization_ParentId_LanguageId",
                table: "CountryLocalization");

            migrationBuilder.RenameColumn(
                name: "IdentityProviderExternalId",
                table: "User",
                newName: "SecurityStamp");

            migrationBuilder.AddColumn<int>(
                name: "AccessFailedCount",
                table: "User",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "EmailConfirmed",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LockoutEnabled",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LockoutEnd",
                table: "User",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MigrationId",
                table: "User",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedEmail",
                table: "User",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedUsername",
                table: "User",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "User",
                type: "nvarchar(max)",
                maxLength: 2147483647,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "User",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "Role",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_NormalizedEmail",
                table: "User",
                column: "NormalizedEmail",
                unique: true,
                filter: "Deleted IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_User_NormalizedUsername",
                table: "User",
                column: "NormalizedUsername",
                unique: true,
                filter: "Deleted IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CountryLocalization_ParentId",
                table: "CountryLocalization",
                column: "ParentId");
        }
    }
}
