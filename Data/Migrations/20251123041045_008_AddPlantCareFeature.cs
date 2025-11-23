using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GardenHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class _008_AddPlantCareFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlantCareActivities",
                columns: table => new
                {
                    ActivityId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlantId = table.Column<int>(type: "integer", nullable: false),
                    ActivityDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActivityType = table.Column<int>(type: "integer", nullable: false),
                    PlantingDepth = table.Column<double>(type: "double precision", nullable: true),
                    PlantingMethod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GrowingMedium = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    WateringPerformed = table.Column<bool>(type: "boolean", nullable: false),
                    WaterAmount = table.Column<double>(type: "double precision", nullable: true),
                    MoistureLevel = table.Column<int>(type: "integer", nullable: true),
                    FertilizerApplied = table.Column<bool>(type: "boolean", nullable: false),
                    FertilizerType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FertilizerAmount = table.Column<double>(type: "double precision", nullable: true),
                    NpkRatio = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LightHours = table.Column<double>(type: "double precision", nullable: true),
                    LightSource = table.Column<int>(type: "integer", nullable: true),
                    LightIntensity = table.Column<int>(type: "integer", nullable: true),
                    PruningPerformed = table.Column<bool>(type: "boolean", nullable: false),
                    PruningDetails = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SupportAdded = table.Column<bool>(type: "boolean", nullable: false),
                    SupportDetails = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    PestControlPerformed = table.Column<bool>(type: "boolean", nullable: false),
                    PestDiseaseIdentified = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    TreatmentApplied = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Temperature = table.Column<double>(type: "double precision", nullable: true),
                    Humidity = table.Column<int>(type: "integer", nullable: true),
                    PhLevel = table.Column<double>(type: "double precision", nullable: true),
                    PlantHealthStatus = table.Column<int>(type: "integer", nullable: true),
                    PlantHeight = table.Column<double>(type: "double precision", nullable: true),
                    LeafCount = table.Column<int>(type: "integer", nullable: true),
                    IsFloweringFruiting = table.Column<bool>(type: "boolean", nullable: false),
                    FloweringFruitingDetails = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    HarvestPerformed = table.Column<bool>(type: "boolean", nullable: false),
                    HarvestAmount = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    HarvestQuality = table.Column<int>(type: "integer", nullable: true),
                    PlantRemoved = table.Column<bool>(type: "boolean", nullable: false),
                    RemovalReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ActivityDuration = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ImageData = table.Column<byte[]>(type: "bytea", nullable: true),
                    ImageType = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantCareActivities", x => x.ActivityId);
                    table.ForeignKey(
                        name: "FK_PlantCareActivities_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlantCareActivities_Plants_PlantId",
                        column: x => x.PlantId,
                        principalTable: "Plants",
                        principalColumn: "PlantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlantCareActivities_PlantId",
                table: "PlantCareActivities",
                column: "PlantId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantCareActivities_UserId",
                table: "PlantCareActivities",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlantCareActivities");
        }
    }
}
