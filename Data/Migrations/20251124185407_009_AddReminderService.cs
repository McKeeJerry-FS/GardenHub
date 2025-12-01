using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GardenHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class _009_AddReminderService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reminders",
                columns: table => new
                {
                    ReminderId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReminderDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReminderType = table.Column<int>(type: "integer", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsRecurring = table.Column<bool>(type: "boolean", nullable: false),
                    RecurrencePattern = table.Column<int>(type: "integer", nullable: true),
                    RecurrenceInterval = table.Column<int>(type: "integer", nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    DailyRecordId = table.Column<int>(type: "integer", nullable: true),
                    GardenCareActivityId = table.Column<int>(type: "integer", nullable: true),
                    GardenId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reminders", x => x.ReminderId);
                    table.ForeignKey(
                        name: "FK_Reminders_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reminders_DailyRecords_DailyRecordId",
                        column: x => x.DailyRecordId,
                        principalTable: "DailyRecords",
                        principalColumn: "RecordId");
                    table.ForeignKey(
                        name: "FK_Reminders_GardenCareActivities_GardenCareActivityId",
                        column: x => x.GardenCareActivityId,
                        principalTable: "GardenCareActivities",
                        principalColumn: "ActivityId");
                    table.ForeignKey(
                        name: "FK_Reminders_Gardens_GardenId",
                        column: x => x.GardenId,
                        principalTable: "Gardens",
                        principalColumn: "GardenId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_DailyRecordId",
                table: "Reminders",
                column: "DailyRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_GardenCareActivityId",
                table: "Reminders",
                column: "GardenCareActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_GardenId",
                table: "Reminders",
                column: "GardenId");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_UserId",
                table: "Reminders",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reminders");
        }
    }
}
