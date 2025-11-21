using GardenHub.Models.Enums;

namespace GardenHub.Models.ViewModels
{
    public class GardenDashboardViewModel
    {
        public Garden Garden { get; set; } = null!;
        
        // Most recent daily record for current conditions
        public DailyRecord? MostRecentRecord { get; set; }
        
        // Chart data for the selected month
        public List<string> ChartLabels { get; set; } = new List<string>();
        
        // Temperature data
        public List<double> InsideTemperatureData { get; set; } = new List<double>();
        public List<double> OutsideTemperatureData { get; set; } = new List<double>();
        
        // Humidity data
        public List<double> InsideHumidityData { get; set; } = new List<double>();
        public List<double> OutsideHumidityData { get; set; } = new List<double>();
        
        // VPD data
        public List<double> InsideVPDData { get; set; } = new List<double>();
        public List<double> OutsideVPDData { get; set; } = new List<double>();
        
        // Month selection
        public int SelectedMonth { get; set; }
        public int SelectedYear { get; set; }
        
        // Available months (for dropdown)
        public List<MonthYearOption> AvailableMonths { get; set; } = new List<MonthYearOption>();

        // Equipment Maintenance for this garden
        public List<Equipment> EquipmentUnderMaintenance { get; set; } = new List<Equipment>();
        public List<Equipment> EquipmentMaintenanceRequested { get; set; } = new List<Equipment>();
        public int OperationalEquipmentCount { get; set; }
    }
    
    public class MonthYearOption
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string DisplayText { get; set; } = string.Empty;
    }
}