using GardenHub.Models.Enums;

namespace GardenHub.Models.ViewModels
{
    public class PlantCareDashboardViewModel
    {
        public Plant Plant { get; set; } = null!;
        public List<PlantCareActivity> RecentActivities { get; set; } = new List<PlantCareActivity>();
        public PlantCareActivity? MostRecentActivity { get; set; }
        
        // Statistics
        public int TotalActivitiesCount { get; set; }
        public int WateringCount { get; set; }
        public int FertilizingCount { get; set; }
        public int PruningCount { get; set; }
        public int PestControlCount { get; set; }
        public double AverageActivityDuration { get; set; }
        public double TotalWaterAmount { get; set; }
        
        // Chart data
        public List<string> ChartLabels { get; set; } = new List<string>();
        public List<int> ActivityCountByType { get; set; } = new List<int>();
        public List<string> ActivityTypeLabels { get; set; } = new List<string>();
        public List<int> HealthStatusData { get; set; } = new List<int>();
        
        // Timeline data
        public Dictionary<DateTime, List<PlantCareActivity>> ActivitiesByDate { get; set; } = new Dictionary<DateTime, List<PlantCareActivity>>();
        
        // Month/Year filtering
        public int SelectedMonth { get; set; }
        public int SelectedYear { get; set; }
        public List<MonthYearOption> AvailableMonths { get; set; } = new List<MonthYearOption>();
    }
}