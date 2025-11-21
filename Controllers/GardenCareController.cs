using GardenHub.Models;
using GardenHub.Models.Enums;
using GardenHub.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GardenHub.Controllers
{
    [Authorize]
    public class GardenCareController : Controller
    {
        private readonly IGardenCareService _careService;
        private readonly IImageService _imageService;

        public GardenCareController(IGardenCareService careService, IImageService imageService)
        {
            _careService = careService;
            _imageService = imageService;
        }

        // GET: GardenCare
        public async Task<IActionResult> Index(int? gardenId)
        {
            List<GardenCareActivity> activities;
            
            if (gardenId.HasValue)
            {
                activities = await _careService.GetCareActivitiesByGardenIdAsync(gardenId.Value);
                ViewData["GardenId"] = gardenId.Value;
            }
            else
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                activities = await _careService.GetCareActivitiesByUserIdAsync(userId);
            }

            return View(activities);
        }

        // GET: GardenCare/Create
        public async Task<IActionResult> Create(int? gardenId)
        {
            var gardens = await _careService.GetAllGardensAsync();
            ViewData["GardenId"] = new SelectList(gardens, "GardenId", "GardenName", gardenId);
            
            var model = new GardenCareActivity
            {
                ActivityDate = DateTime.Now,
                GardenId = gardenId ?? 0
            };
            
            return View(model);
        }

        // POST: GardenCare/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GardenCareActivity activity)
        {
            // Ensure DateTime is in UTC for PostgreSQL
            if (activity.ActivityDate.Kind == DateTimeKind.Unspecified)
            {
                activity.ActivityDate = DateTime.SpecifyKind(activity.ActivityDate, DateTimeKind.Utc);
            }
            else if (activity.ActivityDate.Kind == DateTimeKind.Local)
            {
                activity.ActivityDate = activity.ActivityDate.ToUniversalTime();
            }

            // Handle image upload
            if (activity.ImageFile != null)
            {
                activity.ImageData = await _imageService.ConvertFileToByteArrayAsync(activity.ImageFile);
                activity.ImageType = activity.ImageFile.ContentType;
            }

            ModelState.Remove("Garden");
            ModelState.Remove("User");
            ModelState.Remove("UserId");
            ModelState.Remove("ImageFile");

            if (ModelState.IsValid)
            {
                try
                {
                    activity.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    await _careService.CreateCareActivityAsync(activity);
                    TempData["SuccessMessage"] = "Garden care activity has been recorded successfully!";
                    return RedirectToAction("Dashboard", "Gardens", new { id = activity.GardenId });
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"An error occurred while recording the activity: {ex.Message}";
                }
            }

            var gardens = await _careService.GetAllGardensAsync();
            ViewData["GardenId"] = new SelectList(gardens, "GardenId", "GardenName", activity.GardenId);
            return View(activity);
        }

        // GET: GardenCare/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activity = await _careService.GetCareActivityByIdAsync(id.Value);
            if (activity == null)
            {
                return NotFound();
            }

            // Convert image for display
            if (activity.ImageData != null)
            {
                ViewData["ActivityImage"] = _imageService.ConvertByteArrayToFile(
                    activity.ImageData,
                    activity.ImageType,
                    DefaultImage.GardenImage);
            }

            return View(activity);
        }

        // GET: GardenCare/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activity = await _careService.GetCareActivityByIdAsync(id.Value);
            if (activity == null)
            {
                return NotFound();
            }

            var gardens = await _careService.GetAllGardensAsync();
            ViewData["GardenId"] = new SelectList(gardens, "GardenId", "GardenName", activity.GardenId);
            
            return View(activity);
        }

        // POST: GardenCare/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GardenCareActivity activity)
        {
            if (id != activity.ActivityId)
            {
                return NotFound();
            }

            // Ensure DateTime is in UTC
            if (activity.ActivityDate.Kind == DateTimeKind.Unspecified)
            {
                activity.ActivityDate = DateTime.SpecifyKind(activity.ActivityDate, DateTimeKind.Utc);
            }
            else if (activity.ActivityDate.Kind == DateTimeKind.Local)
            {
                activity.ActivityDate = activity.ActivityDate.ToUniversalTime();
            }

            // Handle image upload
            if (activity.ImageFile != null)
            {
                activity.ImageData = await _imageService.ConvertFileToByteArrayAsync(activity.ImageFile);
                activity.ImageType = activity.ImageFile.ContentType;
            }
            else
            {
                // Keep existing image
                var existingActivity = await _careService.GetCareActivityByIdAsync(id);
                if (existingActivity != null)
                {
                    activity.ImageData = existingActivity.ImageData;
                    activity.ImageType = existingActivity.ImageType;
                }
            }

            ModelState.Remove("Garden");
            ModelState.Remove("User");
            ModelState.Remove("UserId");
            ModelState.Remove("ImageFile");

            if (ModelState.IsValid)
            {
                try
                {
                    await _careService.UpdateCareActivityAsync(id, activity);
                    TempData["SuccessMessage"] = "Garden care activity has been updated successfully!";
                    return RedirectToAction("Dashboard", "Gardens", new { id = activity.GardenId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    var exists = await _careService.GetCareActivityByIdAsync(id);
                    if (exists == null)
                    {
                        TempData["ErrorMessage"] = "The activity you are trying to update no longer exists.";
                        return RedirectToAction(nameof(Index));
                    }
                    throw;
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"An error occurred while updating the activity: {ex.Message}";
                }
            }

            var gardens = await _careService.GetAllGardensAsync();
            ViewData["GardenId"] = new SelectList(gardens, "GardenId", "GardenName", activity.GardenId);
            return View(activity);
        }

        // GET: GardenCare/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activity = await _careService.GetCareActivityByIdAsync(id.Value);
            if (activity == null)
            {
                return NotFound();
            }

            return View(activity);
        }

        // POST: GardenCare/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var activity = await _careService.GetCareActivityByIdAsync(id);
                var gardenId = activity?.GardenId;
                await _careService.DeleteCareActivityAsync(id);
                TempData["SuccessMessage"] = "Garden care activity has been deleted successfully!";
                
                if (gardenId.HasValue)
                {
                    return RedirectToAction("Dashboard", "Gardens", new { id = gardenId.Value });
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the activity: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: GardenCare/Dashboard/5
        public async Task<IActionResult> Dashboard(int? id, int? month, int? year)
        {
            if (id == null)
            {
                return NotFound();
            }

            var garden = await _careService.GetGardenWithCareActivitiesAsync(id.Value);

            if (garden == null)
            {
                return NotFound();
            }

            // Set default month/year to current if not specified
            int selectedMonth = month ?? DateTime.Now.Month;
            int selectedYear = year ?? DateTime.Now.Year;

            var viewModel = new GardenHub.Models.ViewModels.GardenCareDashboardViewModel
            {
                Garden = garden,
                SelectedMonth = selectedMonth,
                SelectedYear = selectedYear
            };

            // Get all activities for this garden
            var allActivities = garden.CareActivities.ToList();
            
            // Get available months based on activities
            var activityDates = allActivities
                .Select(a => new { a.ActivityDate.Year, a.ActivityDate.Month })
                .Distinct()
                .OrderByDescending(d => d.Year)
                .ThenByDescending(d => d.Month)
                .ToList();

            viewModel.AvailableMonths = activityDates.Select(d => new GardenHub.Models.ViewModels.MonthYearOption
            {
                Month = d.Month,
                Year = d.Year,
                DisplayText = new DateTime(d.Year, d.Month, 1).ToString("MMMM yyyy")
            }).ToList();

            // If no available months, add current month
            if (!viewModel.AvailableMonths.Any())
            {
                viewModel.AvailableMonths.Add(new GardenHub.Models.ViewModels.MonthYearOption
                {
                    Month = selectedMonth,
                    Year = selectedYear,
                    DisplayText = new DateTime(selectedYear, selectedMonth, 1).ToString("MMMM yyyy")
                });
            }

            // Filter activities for selected month
            var monthlyActivities = allActivities
                .Where(a => a.ActivityDate.Month == selectedMonth && a.ActivityDate.Year == selectedYear)
                .OrderByDescending(a => a.ActivityDate)
                .ToList();

            viewModel.RecentActivities = monthlyActivities.Take(10).ToList();
            viewModel.MostRecentActivity = monthlyActivities.FirstOrDefault();

            // Calculate statistics
            viewModel.TotalActivitiesCount = monthlyActivities.Count;
            viewModel.WateringSessionsCount = monthlyActivities.Count(a => a.WateringPerformed);
            viewModel.NutrientApplicationsCount = monthlyActivities.Count(a => a.NutrientsAdded);
            viewModel.NewPlantingsCount = monthlyActivities.Count(a => a.NewPlantingsAdded);
            viewModel.WeedingSessionsCount = monthlyActivities.Count(a => a.WeedingPerformed);
            viewModel.PruningSessionsCount = monthlyActivities.Count(a => a.PruningPerformed);
            viewModel.PestControlSessionsCount = monthlyActivities.Count(a => a.PestControlPerformed);
            
            viewModel.AverageActivityDuration = monthlyActivities
                .Where(a => a.ActivityDuration.HasValue)
                .Select(a => a.ActivityDuration!.Value)
                .DefaultIfEmpty(0)
                .Average();

            viewModel.TotalWaterAdded = monthlyActivities
                .Where(a => a.WaterAmountAdded.HasValue)
                .Sum(a => a.WaterAmountAdded!.Value);

            viewModel.TotalPlantsAdded = monthlyActivities
                .Where(a => a.NumberOfPlantsAdded.HasValue)
                .Sum(a => a.NumberOfPlantsAdded!.Value);

            // Prepare chart data - Activity counts by type
            var activityTypeGroups = monthlyActivities
                .GroupBy(a => a.ActivityType)
                .OrderByDescending(g => g.Count())
                .ToList();

            viewModel.ActivityTypeLabels = activityTypeGroups.Select(g => g.Key.ToString()).ToList();
            viewModel.ActivityCountByType = activityTypeGroups.Select(g => g.Count()).ToList();

            // Water usage over time
            var waterUsage = monthlyActivities
                .Where(a => a.WaterAmountAdded.HasValue)
                .OrderBy(a => a.ActivityDate)
                .ToList();

            viewModel.ChartLabels = waterUsage.Select(a => a.ActivityDate.ToString("MMM dd")).ToList();
            viewModel.WaterUsageData = waterUsage.Select(a => a.WaterAmountAdded ?? 0).ToList();

            // Activity frequency by day
            var activitiesByDay = monthlyActivities
                .GroupBy(a => a.ActivityDate.Date)
                .OrderBy(g => g.Key)
                .ToList();

            viewModel.ActivityFrequencyData = activitiesByDay.Select(g => g.Count()).ToList();

            // Timeline data
            viewModel.ActivitiesByDate = monthlyActivities
                .GroupBy(a => a.ActivityDate.Date)
                .OrderByDescending(g => g.Key)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Convert garden image
            if (garden.ImageData != null)
            {
                ViewData["GardenImage"] = _imageService.ConvertByteArrayToFile(
                    garden.ImageData,
                    garden.ImageType,
                    DefaultImage.GardenImage);
            }

            return View(viewModel);
        }
    }
}