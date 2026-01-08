using GardenHub.Data;
using GardenHub.Models;
using GardenHub.Models.ViewModels;
using GardenHub.Models.Enums;
using GardenHub.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace GardenHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IReminderService? _reminderService;

        public HomeController(
            ILogger<HomeController> logger, 
            ApplicationDbContext context, 
            UserManager<AppUser> userManager,
            IReminderService? reminderService = null)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _reminderService = reminderService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Features()
        {
            return View();
        }

        public IActionResult FAQs()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var viewModel = new DashboardViewModel();
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index");
            }

            // Get current user for Pro feature checks
            var currentUser = await _userManager.FindByIdAsync(userId);
            ViewBag.CurrentUser = currentUser;

            // Aggregate Statistics
            viewModel.TotalGardens = await _context.Gardens.CountAsync(g => g.UserId == userId);
            viewModel.TotalPlants = await _context.Plants.CountAsync(p => p.UserId == userId);
            viewModel.TotalEquipment = await _context.Equipments.CountAsync(e => e.UserId == userId);
            viewModel.TotalDailyRecords = await _context.DailyRecords.CountAsync(d => d.UserId == userId);
            viewModel.TotalJournalEntries = await _context.JournalEntries.CountAsync(j => j.UserId == userId);

            // Recent Items (Last 5)
            viewModel.RecentGardens = await _context.Gardens
                .Where(g => g.UserId == userId)
                .OrderByDescending(g => g.GardenId)
                .Take(5)
                .ToListAsync();

            viewModel.RecentPlants = await _context.Plants
                .Where(p => p.UserId == userId)
                .Include(p => p.Garden)
                .OrderByDescending(p => p.PlantId)
                .Take(5)
                .ToListAsync();

            viewModel.RecentDailyRecords = await _context.DailyRecords
                .Where(d => d.UserId == userId)
                .Include(d => d.Garden)
                .OrderByDescending(d => d.CreatedDate)
                .Take(5)
                .ToListAsync();

            viewModel.RecentJournalEntries = await _context.JournalEntries
                .Where(j => j.UserId == userId)
                .Include(j => j.Garden)
                .OrderByDescending(j => j.EntryDate)
                .Take(5)
                .ToListAsync();

            viewModel.RecentEquipment = await _context.Equipments
                .Where(e => e.UserId == userId)
                .Include(e => e.Garden)
                .OrderByDescending(e => e.EquipmentId)
                .Take(5)
                .ToListAsync();

            // All Gardens for Quick Access
            viewModel.AllGardens = await _context.Gardens
                .Where(g => g.UserId == userId)
                .OrderBy(g => g.GardenName)
                .ToListAsync();

            // Additional Statistics
            var plants = await _context.Plants.Where(p => p.UserId == userId).ToListAsync();
            viewModel.PlantsByType = plants.GroupBy(p => p.PlantType.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            var equipment = await _context.Equipments.Where(e => e.UserId == userId).ToListAsync();
            viewModel.EquipmentByType = equipment.GroupBy(e => e.EquipmentType.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            var gardens = await _context.Gardens.Where(g => g.UserId == userId).ToListAsync();
            viewModel.GardensByType = gardens.GroupBy(g => g.Type.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            // Average Metrics from Recent Daily Records (last 30 days)
            var recentRecords = await _context.DailyRecords
                .Where(d => d.UserId == userId && d.CreatedDate >= DateTime.UtcNow.AddDays(-30))
                .ToListAsync();

            if (recentRecords.Any())
            {
                viewModel.AverageInsideTemperature = recentRecords.Average(r => r.InsideTemperature);
                viewModel.AverageOutsideTemperature = recentRecords.Average(r => r.OutsideTemperature);
                viewModel.AverageInsideHumidity = recentRecords.Average(r => r.InsideHumidity);
                viewModel.AverageOutsideHumidity = recentRecords.Average(r => r.OutsideHumidity);
            }

            // Equipment Maintenance Notifications
            viewModel.EquipmentUnderMaintenance = await _context.Equipments
                .Where(e => e.UserId == userId && e.MaintenanceStatus == MaintenanceStatus.UnderMaintenance)
                .Include(e => e.Garden)
                .ToListAsync();

            viewModel.EquipmentMaintenanceRequested = await _context.Equipments
                .Where(e => e.UserId == userId && e.MaintenanceStatus == MaintenanceStatus.MaintenanceRequested)
                .Include(e => e.Garden)
                .ToListAsync();

            viewModel.OperationalEquipmentCount = await _context.Equipments
                .CountAsync(e => e.UserId == userId && e.MaintenanceStatus == MaintenanceStatus.Operational);

            // Load Reminders - FREE FEATURE available to all users
            if (_reminderService != null)
            {
                try
                {
                    // Get overdue reminders
                    viewModel.OverdueReminders = await _reminderService.GetOverdueRemindersAsync(userId);
                    
                    // Get today's reminders
                    var todayStart = DateTime.UtcNow.Date;
                    var todayEnd = todayStart.AddDays(1);
                    viewModel.TodayReminders = await _reminderService.GetRemindersByDateRangeAsync(userId, todayStart, todayEnd);
                    viewModel.TodayReminders = viewModel.TodayReminders.Where(r => !r.IsCompleted).ToList();
                    
                    // Get upcoming reminders (next 7 days, excluding today)
                    viewModel.UpcomingReminders = await _reminderService.GetUpcomingRemindersAsync(userId, 7);
                    viewModel.UpcomingReminders = viewModel.UpcomingReminders
                        .Where(r => r.ReminderDateTime >= todayEnd)
                        .Take(5)
                        .ToList();
                    
                    // Total active reminders
                    viewModel.TotalActiveReminders = await _context.Reminders
                        .CountAsync(r => r.UserId == userId && !r.IsCompleted);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading reminders for user {UserId}", userId);
                    // Continue without reminders if there's an error
                }
            }

            // Store images in ViewData for recent items
            foreach (var garden in viewModel.RecentGardens)
            {
                if (garden.ImageData != null)
                {
                    var base64 = Convert.ToBase64String(garden.ImageData);
                    ViewData[$"GardenImage_{garden.GardenId}"] = $"data:{garden.ImageType};base64,{base64}";
                }
            }

            foreach (var plant in viewModel.RecentPlants)
            {
                if (plant.ImageData != null)
                {
                    var base64 = Convert.ToBase64String(plant.ImageData);
                    ViewData[$"PlantImage_{plant.PlantId}"] = $"data:{plant.ImageType};base64,{base64}";
                }
            }

            foreach (var eq in viewModel.RecentEquipment)
            {
                if (eq.ImageData != null)
                {
                    var base64 = Convert.ToBase64String(eq.ImageData);
                    ViewData[$"EquipmentImage_{eq.EquipmentId}"] = $"data:{eq.ImageType};base64,{base64}";
                }
            }

            foreach (var entry in viewModel.RecentJournalEntries)
            {
                if (entry.ImageData != null)
                {
                    var base64 = Convert.ToBase64String(entry.ImageData);
                    ViewData[$"JournalImage_{entry.EntryId}"] = $"data:{entry.ImageType};base64,{base64}";
                }
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
