using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GardenHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class _004_Adding_Image_Properties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "Plants",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageType",
                table: "Plants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LightingRequirement",
                table: "Plants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "JournalEntries",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageType",
                table: "JournalEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "Gardens",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageType",
                table: "Gardens",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "Equipments",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageType",
                table: "Equipments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "ImageType",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "LightingRequirement",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "ImageType",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "Gardens");

            migrationBuilder.DropColumn(
                name: "ImageType",
                table: "Gardens");

            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "Equipments");

            migrationBuilder.DropColumn(
                name: "ImageType",
                table: "Equipments");
        }
    }
}
