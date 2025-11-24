using GardenHub.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GardenHub.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Garden> Gardens { get; set; }
        public DbSet<DailyRecord> DailyRecords { get; set; }
        public DbSet<JournalEntry> JournalEntries { get; set; }
        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<Plant> Plants { get; set; }
        public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }
        public DbSet<GardenCareActivity> GardenCareActivities { get; set; }
        public DbSet<PlantCareActivity> PlantCareActivities { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
    }
}
