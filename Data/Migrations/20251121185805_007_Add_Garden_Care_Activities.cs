using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GardenHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class _007_Add_Garden_Care_Activities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GardenCareActivities",
                columns: table => new
                {
                    ActivityId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GardenId = table.Column<int>(type: "integer", nullable: false),
                    ActivityDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActivityType = table.Column<int>(type: "integer", nullable: false),
                    PlantInspectionStatus = table.Column<int>(type: "integer", nullable: true),
                    EquipmentInspectionStatus = table.Column<int>(type: "integer", nullable: true),
                    WaterLevel = table.Column<double>(type: "double precision", nullable: true),
                    WateringPerformed = table.Column<bool>(type: "boolean", nullable: false),
                    WaterAmountAdded = table.Column<double>(type: "double precision", nullable: true),
                    NutrientsAdded = table.Column<bool>(type: "boolean", nullable: false),
                    NutrientType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    NutrientAmount = table.Column<double>(type: "double precision", nullable: true),
                    NewPlantingsAdded = table.Column<bool>(type: "boolean", nullable: false),
                    NumberOfPlantsAdded = table.Column<int>(type: "integer", nullable: true),
                    PlantTypesAdded = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    WeedingPerformed = table.Column<bool>(type: "boolean", nullable: false),
                    WeedingDuration = table.Column<int>(type: "integer", nullable: true),
                    PruningPerformed = table.Column<bool>(type: "boolean", nullable: false),
                    PruningNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PestControlPerformed = table.Column<bool>(type: "boolean", nullable: false),
                    PestControlDetails = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PhLevel = table.Column<double>(type: "double precision", nullable: true),
                    EcLevel = table.Column<int>(type: "integer", nullable: true),
                    GardenStatus = table.Column<int>(type: "integer", nullable: true),
                    ActivityDuration = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ImageData = table.Column<byte[]>(type: "bytea", nullable: true),
                    ImageType = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GardenCareActivities", x => x.ActivityId);
                    table.ForeignKey(
                        name: "FK_GardenCareActivities_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GardenCareActivities_Gardens_GardenId",
                        column: x => x.GardenId,
                        principalTable: "Gardens",
                        principalColumn: "GardenId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GardenCareActivities_GardenId",
                table: "GardenCareActivities",
                column: "GardenId");

            migrationBuilder.CreateIndex(
                name: "IX_GardenCareActivities_UserId",
                table: "GardenCareActivities",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GardenCareActivities");
        }
    }
}
