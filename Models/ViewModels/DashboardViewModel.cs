namespace GardenHub.Models.ViewModels
{
    public class DashboardViewModel
    {
        // Aggregate Statistics
        public int TotalGardens { get; set; }
        public int TotalPlants { get; set; }
        public int TotalEquipment { get; set; }
        public int TotalDailyRecords { get; set; }
        public int TotalJournalEntries { get; set; }

        // Recent Items
        public List<Garden> RecentGardens { get; set; } = new List<Garden>();
        public List<Plant> RecentPlants { get; set; } = new List<Plant>();
        public List<DailyRecord> RecentDailyRecords { get; set; } = new List<DailyRecord>();
        public List<JournalEntry> RecentJournalEntries { get; set; } = new List<JournalEntry>();
        public List<Equipment> RecentEquipment { get; set; } = new List<Equipment>();

        // All Gardens for Quick Access
        public List<Garden> AllGardens { get; set; } = new List<Garden>();

        // Additional Statistics
        public Dictionary<string, int> PlantsByType { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> EquipmentByType { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> GardensByType { get; set; } = new Dictionary<string, int>();
        
        // Average Metrics from Recent Daily Records
        public double AverageInsideTemperature { get; set; }
        public double AverageOutsideTemperature { get; set; }
        public double AverageInsideHumidity { get; set; }
        public double AverageOutsideHumidity { get; set; }

        // Equipment Maintenance Notifications
        public List<Equipment> EquipmentUnderMaintenance { get; set; } = new List<Equipment>();
        public List<Equipment> EquipmentMaintenanceRequested { get; set; } = new List<Equipment>();
        public int OperationalEquipmentCount { get; set; }
    }
}