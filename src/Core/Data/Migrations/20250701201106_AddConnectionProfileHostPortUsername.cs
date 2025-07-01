using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConnectionManager.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddConnectionProfileHostPortUsername : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Host",
                table: "ConnectionProfiles",
                type: "TEXT",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<ushort>(
                name: "Port",
                table: "ConnectionProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: (ushort)0);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "ConnectionProfiles",
                type: "TEXT",
                maxLength: 512,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Host",
                table: "ConnectionProfiles");

            migrationBuilder.DropColumn(
                name: "Port",
                table: "ConnectionProfiles");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "ConnectionProfiles");
        }
    }
}
