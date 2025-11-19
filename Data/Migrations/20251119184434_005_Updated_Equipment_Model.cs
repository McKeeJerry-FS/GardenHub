using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GardenHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class _005_Updated_Equipment_Model : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EquipmentType",
                table: "Equipments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PurchasePrice",
                table: "Equipments",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EquipmentType",
                table: "Equipments");

            migrationBuilder.DropColumn(
                name: "PurchasePrice",
                table: "Equipments");
        }
    }
}
