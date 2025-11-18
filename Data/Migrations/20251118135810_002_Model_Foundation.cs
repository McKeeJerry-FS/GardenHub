using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GardenHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class _002_Model_Foundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Gardens",
                columns: table => new
                {
                    GardenId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GardenName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    GardenDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    GardenLocation = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    GardenGrowMethod = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gardens", x => x.GardenId);
                    table.ForeignKey(
                        name: "FK_Gardens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyRecords",
                columns: table => new
                {
                    RecordId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GardenId = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InsideTemperature = table.Column<double>(type: "double precision", nullable: false),
                    OutsideTemperature = table.Column<double>(type: "double precision", nullable: false),
                    InsideHumidity = table.Column<double>(type: "double precision", nullable: false),
                    OutsideHumidity = table.Column<double>(type: "double precision", nullable: false),
                    InsideVPD = table.Column<double>(type: "double precision", nullable: false),
                    OutsideVPD = table.Column<double>(type: "double precision", nullable: false),
                    LightingOn = table.Column<double>(type: "double precision", nullable: false),
                    LightingOff = table.Column<double>(type: "double precision", nullable: false),
                    LightingIntensity = table.Column<int>(type: "integer", nullable: false),
                    WaterAmount = table.Column<int>(type: "integer", nullable: false),
                    NutrientAmount = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyRecords", x => x.RecordId);
                    table.ForeignKey(
                        name: "FK_DailyRecords_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DailyRecords_Gardens_GardenId",
                        column: x => x.GardenId,
                        principalTable: "Gardens",
                        principalColumn: "GardenId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Equipments",
                columns: table => new
                {
                    EquipmentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EquipmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EquipmentDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastMaintenanceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GardenId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipments", x => x.EquipmentId);
                    table.ForeignKey(
                        name: "FK_Equipments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Equipments_Gardens_GardenId",
                        column: x => x.GardenId,
                        principalTable: "Gardens",
                        principalColumn: "GardenId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JournalEntries",
                columns: table => new
                {
                    EntryId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GardenId = table.Column<int>(type: "integer", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Content = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntries", x => x.EntryId);
                    table.ForeignKey(
                        name: "FK_JournalEntries_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JournalEntries_Gardens_GardenId",
                        column: x => x.GardenId,
                        principalTable: "Gardens",
                        principalColumn: "GardenId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Plants",
                columns: table => new
                {
                    PlantId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlantName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PlantDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PlantType = table.Column<int>(type: "integer", nullable: false),
                    GrowMethod = table.Column<int>(type: "integer", nullable: false),
                    DatePlanted = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GardenId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plants", x => x.PlantId);
                    table.ForeignKey(
                        name: "FK_Plants_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Plants_Gardens_GardenId",
                        column: x => x.GardenId,
                        principalTable: "Gardens",
                        principalColumn: "GardenId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyRecords_GardenId",
                table: "DailyRecords",
                column: "GardenId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyRecords_UserId",
                table: "DailyRecords",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipments_GardenId",
                table: "Equipments",
                column: "GardenId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipments_UserId",
                table: "Equipments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Gardens_UserId",
                table: "Gardens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_GardenId",
                table: "JournalEntries",
                column: "GardenId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_UserId",
                table: "JournalEntries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Plants_GardenId",
                table: "Plants",
                column: "GardenId");

            migrationBuilder.CreateIndex(
                name: "IX_Plants_UserId",
                table: "Plants",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyRecords");

            migrationBuilder.DropTable(
                name: "Equipments");

            migrationBuilder.DropTable(
                name: "JournalEntries");

            migrationBuilder.DropTable(
                name: "Plants");

            migrationBuilder.DropTable(
                name: "Gardens");
        }
    }
}
