using System.Diagnostics;
using GardenHub.Models;
using GardenHub.Models.Enums;
using GardenHub.Models.ViewModels;
using GardenHub.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
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
        private readonly IEmailSender _emailSender;

        public HomeController(
            ILogger<HomeController> logger,
            IGardenService gardenService,
            IPlantService plantService,
            IDailyRecordService dailyRecordService,
            IJournalEntriesService journalEntriesService,
            IEquipmentService equipmentService,
            IImageService imageService,
            IEmailSender emailSender)
        {
            _logger = logger;
            _gardenService = gardenService;
            _plantService = plantService;
            _dailyRecordService = dailyRecordService;
            _journalEntriesService = journalEntriesService;
            _equipmentService = equipmentService;
            _imageService = imageService;
            _emailSender = emailSender;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Build email content
                    var emailSubject = $"GardenHub Contact Form: {model.Subject}";
                    var emailBody = $@"
                        <html>
                        <head>
                            <style>
                                body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                                .header {{ background: linear-gradient(135deg, #56ab2f 0%, #a8e063 100%); color: white; padding: 20px; border-radius: 5px 5px 0 0; }}
                                .content {{ background: #f9f9f9; padding: 20px; border: 1px solid #ddd; }}
                                .field {{ margin-bottom: 15px; }}
                                .label {{ font-weight: bold; color: #56ab2f; }}
                                .value {{ margin-top: 5px; padding: 10px; background: white; border-left: 3px solid #56ab2f; }}
                                .footer {{ text-align: center; padding: 15px; color: #666; font-size: 12px; }}
                            </style>
                        </head>
                        <body>
                            <div class='container'>
                                <div class='header'>
                                    <h2>?? GardenHub Contact Form Submission</h2>
                                </div>
                                <div class='content'>
                                    <div class='field'>
                                        <div class='label'>?? From:</div>
                                        <div class='value'>{model.Name} ({model.Email})</div>
                                    </div>
                                    <div class='field'>
                                        <div class='label'>?? Category:</div>
                                        <div class='value'>{model.Category.ToString()}</div>
                                    </div>
                                    <div class='field'>
                                        <div class='label'>?? Subject:</div>
                                        <div class='value'>{model.Subject}</div>
                                    </div>
                                    <div class='field'>
                                        <div class='label'>?? Message:</div>
                                        <div class='value'>{model.Message.Replace("\n", "<br>")}</div>
                                    </div>
                                    <div class='field'>
                                        <div class='label'>?? Submitted:</div>
                                        <div class='value'>{DateTime.Now.ToString("MMMM dd, yyyy h:mm tt")}</div>
                                    </div>
                                </div>
                                <div class='footer'>
                                    <p>This message was sent from the GardenHub Contact Form</p>
                                    <p>GardenHub v1.0.0 | © 2025 GardenHub</p>
                                </div>
                            </div>
                        </body>
                        </html>";

                    // Send email to your address
                    await _emailSender.SendEmailAsync(
                        "your-email@example.com", // Replace with your email
                        emailSubject,
                        emailBody
                    );

                    // Send confirmation email to user
                    var confirmationBody = $@"
                        <html>
                        <head>
                            <style>
                                body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                                .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; border-radius: 5px 5px 0 0; text-align: center; }}
                                .content {{ background: #f9f9f9; padding: 20px; border: 1px solid #ddd; }}
                                .footer {{ text-align: center; padding: 15px; color: #666; font-size: 12px; }}
                                .message-box {{ background: white; padding: 15px; border-left: 4px solid #667eea; margin: 15px 0; }}
                            </style>
                        </head>
                        <body>
                            <div class='container'>
                                <div class='header'>
                                    <h2>Thank You for Contacting GardenHub! ??</h2>
                                </div>
                                <div class='content'>
                                    <p>Hi {model.Name},</p>
                                    <p>Thank you for reaching out to us! We've received your message and will get back to you as soon as possible.</p>
                                    <div class='message-box'>
                                        <p><strong>Your Message Summary:</strong></p>
                                        <p><strong>Category:</strong> {model.Category.ToString()}</p>
                                        <p><strong>Subject:</strong> {model.Subject}</p>
                                    </div>
                                    <p>We typically respond within 24-48 hours during business days.</p>
                                    <p>In the meantime, you might find answers to common questions in our <a href='https://yourdomain.com/Home/FAQs' style='color: #667eea;'>FAQ section</a>.</p>
                                    <p>Best regards,<br>The GardenHub Team</p>
                                </div>
                                <div class='footer'>
                                    <p>GardenHub - Your Modern Garden Management Solution</p>
                                    <p>© 2025 GardenHub | v1.0.0</p>
                                </div>
                            </div>
                        </body>
                        </html>";

                    await _emailSender.SendEmailAsync(
                        model.Email,
                        "Thank you for contacting GardenHub",
                        confirmationBody
                    );

                    TempData["SuccessMessage"] = "Thank you for contacting us! We've received your message and will respond soon.";
                    return RedirectToAction(nameof(Contact));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending contact form email");
                    TempData["ErrorMessage"] = "There was an error sending your message. Please try again later or contact us directly.";
                }
            }

            return View(model);
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

            // Equipment Maintenance Status
            viewModel.EquipmentUnderMaintenance = await _equipmentService.GetEquipmentByStatusAsync(MaintenanceStatus.UnderMaintenance);
            viewModel.EquipmentMaintenanceRequested = await _equipmentService.GetEquipmentByStatusAsync(MaintenanceStatus.MaintenanceRequested);
            viewModel.OperationalEquipmentCount = allEquipment.Count(e => e.MaintenanceStatus == MaintenanceStatus.Operational);

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

            // Convert images for maintenance equipment
            foreach (var equipment in viewModel.EquipmentUnderMaintenance.Concat(viewModel.EquipmentMaintenanceRequested))
            {
                ViewData[$"EquipmentImage_{equipment.EquipmentId}"] = _imageService.ConvertByteArrayToFile(
                    equipment.ImageData,
                    equipment.ImageType,
                    Models.Enums.DefaultImage.EquipmentImage);
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
