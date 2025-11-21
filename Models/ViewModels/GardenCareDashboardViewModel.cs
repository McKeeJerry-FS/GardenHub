using GardenHub.Models.Enums;

namespace GardenHub.Models.ViewModels
{
    public class GardenCareDashboardViewModel
    {
        public Garden Garden { get; set; } = null!;
        public List<GardenCareActivity> RecentActivities { get; set; } = new List<GardenCareActivity>();
        public GardenCareActivity? MostRecentActivity { get; set; }
        
        // Statistics
        public int TotalActivitiesCount { get; set; }
        public int WateringSessionsCount { get; set; }
        public int NutrientApplicationsCount { get; set; }
        public int NewPlantingsCount { get; set; }
        public int WeedingSessionsCount { get; set; }
        public int PruningSessionsCount { get; set; }
        public int PestControlSessionsCount { get; set; }
        public double AverageActivityDuration { get; set; }
        public double TotalWaterAdded { get; set; }
        public int TotalPlantsAdded { get; set; }
        
        // Chart data
        public List<string> ChartLabels { get; set; } = new List<string>();
        public List<int> ActivityCountByType { get; set; } = new List<int>();
        public List<string> ActivityTypeLabels { get; set; } = new List<string>();
        public List<double> WaterUsageData { get; set; } = new List<double>();
        public List<int> ActivityFrequencyData { get; set; } = new List<int>();
        
        // Timeline data
        public Dictionary<DateTime, List<GardenCareActivity>> ActivitiesByDate { get; set; } = new Dictionary<DateTime, List<GardenCareActivity>>();
        
        // Month/Year filtering
        public int SelectedMonth { get; set; }
        public int SelectedYear { get; set; }
        public List<MonthYearOption> AvailableMonths { get; set; } = new List<MonthYearOption>();
    }
}