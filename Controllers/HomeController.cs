using System.Diagnostics;
using GardenHub.Models;
using GardenHub.Models.ViewModels;
using GardenHub.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GardenHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IGardenService _gardenService;
        private readonly IPlantService _plantService;
        private readonly IDailyRecordService _dailyRecordService;
        private readonly IJournalEntriesService _journalEntriesService;
        private readonly IEquipmentService _equipmentService;
        private readonly IImageService _imageService;

        public HomeController(
            ILogger<HomeController> logger,
            IGardenService gardenService,
            IPlantService plantService,
            IDailyRecordService dailyRecordService,
            IJournalEntriesService journalEntriesService,
            IEquipmentService equipmentService,
            IImageService imageService)
        {
            _logger = logger;
            _gardenService = gardenService;
            _plantService = plantService;
            _dailyRecordService = dailyRecordService;
            _journalEntriesService = journalEntriesService;
            _equipmentService = equipmentService;
            _imageService = imageService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var viewModel = new DashboardViewModel();

            // Get all data
            var allGardens = await _gardenService.GetAllGardens();
            var allPlants = await _plantService.GetAllPlantsAsync();
            var allDailyRecords = await _dailyRecordService.GetAllDailyRecordsAsync();
            var allJournalEntries = await _journalEntriesService.GetAllJournalEntriesAsync();
            var allEquipment = await _equipmentService.GetAllEquipmentsAsync();

            // Aggregate Statistics
            viewModel.TotalGardens = allGardens.Count;
            viewModel.TotalPlants = allPlants.Count;
            viewModel.TotalEquipment = allEquipment.Count;
            viewModel.TotalDailyRecords = allDailyRecords.Count;
            viewModel.TotalJournalEntries = allJournalEntries.Count;

            // Recent Items (last 5)
            viewModel.RecentGardens = allGardens
                .OrderByDescending(g => g.StartDate)
                .Take(5)
                .ToList();

            viewModel.RecentPlants = allPlants
                .OrderByDescending(p => p.DatePlanted)
                .Take(5)
                .ToList();

            viewModel.RecentDailyRecords = allDailyRecords
                .OrderByDescending(r => r.CreatedDate)
                .Take(5)
                .ToList();

            viewModel.RecentJournalEntries = allJournalEntries
                .OrderByDescending(j => j.EntryDate)
                .Take(5)
                .ToList();

            viewModel.RecentEquipment = allEquipment
                .OrderByDescending(e => e.PurchaseDate)
                .Take(5)
                .ToList();

            // All Gardens for Quick Access
            viewModel.AllGardens = allGardens.OrderBy(g => g.GardenName).ToList();

            // Group by Type Statistics
            viewModel.PlantsByType = allPlants
                .GroupBy(p => p.PlantType.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            viewModel.EquipmentByType = allEquipment
                .GroupBy(e => e.EquipmentType.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            viewModel.GardensByType = allGardens
                .GroupBy(g => g.Type.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            // Calculate Average Metrics from Last 30 Days of Records
            var recentRecords = allDailyRecords
                .Where(r => r.CreatedDate >= DateTime.UtcNow.AddDays(-30))
                .ToList();

            if (recentRecords.Any())
            {
                viewModel.AverageInsideTemperature = Math.Round(recentRecords.Average(r => r.InsideTemperature), 1);
                viewModel.AverageOutsideTemperature = Math.Round(recentRecords.Average(r => r.OutsideTemperature), 1);
                viewModel.AverageInsideHumidity = Math.Round(recentRecords.Average(r => r.InsideHumidity), 1);
                viewModel.AverageOutsideHumidity = Math.Round(recentRecords.Average(r => r.OutsideHumidity), 1);
            }

            // Convert images for display
            foreach (var garden in viewModel.RecentGardens.Concat(viewModel.AllGardens).Distinct())
            {
                ViewData[$"GardenImage_{garden.GardenId}"] = _imageService.ConvertByteArrayToFile(
                    garden.ImageData,
                    garden.ImageType,
                    Models.Enums.DefaultImage.GardenImage);
            }

            foreach (var plant in viewModel.RecentPlants)
            {
                ViewData[$"PlantImage_{plant.PlantId}"] = _imageService.ConvertByteArrayToFile(
                    plant.ImageData,
                    plant.ImageType,
                    Models.Enums.DefaultImage.PlantImage);
            }

            foreach (var equipment in viewModel.RecentEquipment)
            {
                ViewData[$"EquipmentImage_{equipment.EquipmentId}"] = _imageService.ConvertByteArrayToFile(
                    equipment.ImageData,
                    equipment.ImageType,
                    Models.Enums.DefaultImage.EquipmentImage);
            }

            foreach (var entry in viewModel.RecentJournalEntries)
            {
                ViewData[$"JournalImage_{entry.EntryId}"] = _imageService.ConvertByteArrayToFile(
                    entry.ImageData,
                    entry.ImageType,
                    Models.Enums.DefaultImage.GardenImage);
            }

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
