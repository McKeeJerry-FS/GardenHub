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
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Payment entity
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasIndex(e => e.AppUserId);
                entity.HasIndex(e => e.StripePaymentIntentId).IsUnique();
                entity.HasIndex(e => e.PaymentDate);
                entity.HasIndex(e => e.Status);

                entity.Property(e => e.Amount)
                    .HasPrecision(18, 2);

                entity.Property(e => e.RefundedAmount)
                    .HasPrecision(18, 2);
            });

            // Configure AppUser relationships
            modelBuilder.Entity<AppUser>()
                .HasMany(u => u.Payments)  // Changed from .HasMany<Payment>()
                .WithOne(p => p.AppUser)
                .HasForeignKey(p => p.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
