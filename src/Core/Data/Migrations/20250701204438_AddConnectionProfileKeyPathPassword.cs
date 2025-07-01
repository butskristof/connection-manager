using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConnectionManager.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddConnectionProfileKeyPathPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KeyPath",
                table: "ConnectionProfiles",
                type: "TEXT",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "ConnectionProfiles",
                type: "TEXT",
                maxLength: 512,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KeyPath",
                table: "ConnectionProfiles");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "ConnectionProfiles");
        }
    }
}
