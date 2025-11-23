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
    public class PlantCareController : Controller
    {
        private readonly IPlantCareService _careService;
        private readonly IImageService _imageService;

        public PlantCareController(IPlantCareService careService, IImageService imageService)
        {
            _careService = careService;
            _imageService = imageService;
        }

        // GET: PlantCare
        public async Task<IActionResult> Index(int? plantId)
        {
            List<PlantCareActivity> activities;
            
            if (plantId.HasValue)
            {
                activities = await _careService.GetCareActivitiesByPlantIdAsync(plantId.Value);
                ViewData["PlantId"] = plantId.Value;
            }
            else
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                activities = await _careService.GetCareActivitiesByUserIdAsync(userId);
            }

            return View(activities);
        }

        // GET: PlantCare/Create
        public async Task<IActionResult> Create(int? plantId)
        {
            var plants = await _careService.GetAllPlantsAsync();
            ViewData["PlantId"] = new SelectList(plants, "PlantId", "PlantName", plantId);
            
            var model = new PlantCareActivity
            {
                ActivityDate = DateTime.Now,
                PlantId = plantId ?? 0
            };
            
            return View(model);
        }

        // POST: PlantCare/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PlantCareActivity activity)
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

            ModelState.Remove("Plant");
            ModelState.Remove("User");
            ModelState.Remove("UserId");
            ModelState.Remove("ImageFile");

            if (ModelState.IsValid)
            {
                try
                {
                    activity.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    await _careService.CreateCareActivityAsync(activity);
                    TempData["SuccessMessage"] = "Plant care activity has been recorded successfully!";
                    return RedirectToAction("Details", "Plants", new { id = activity.PlantId });
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"An error occurred while recording the activity: {ex.Message}";
                }
            }

            var plants = await _careService.GetAllPlantsAsync();
            ViewData["PlantId"] = new SelectList(plants, "PlantId", "PlantName", activity.PlantId);
            return View(activity);
        }

        // GET: PlantCare/Details/5
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
                    DefaultImage.PlantImage);
            }

            return View(activity);
        }

        // GET: PlantCare/Edit/5
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

            var plants = await _careService.GetAllPlantsAsync();
            ViewData["PlantId"] = new SelectList(plants, "PlantId", "PlantName", activity.PlantId);
            
            return View(activity);
        }

        // POST: PlantCare/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PlantCareActivity activity)
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

            ModelState.Remove("Plant");
            ModelState.Remove("User");
            ModelState.Remove("UserId");
            ModelState.Remove("ImageFile");

            if (ModelState.IsValid)
            {
                try
                {
                    await _careService.UpdateCareActivityAsync(id, activity);
                    TempData["SuccessMessage"] = "Plant care activity has been updated successfully!";
                    return RedirectToAction("Details", "Plants", new { id = activity.PlantId });
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

            var plants = await _careService.GetAllPlantsAsync();
            ViewData["PlantId"] = new SelectList(plants, "PlantId", "PlantName", activity.PlantId);
            return View(activity);
        }

        // GET: PlantCare/Delete/5
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

        // POST: PlantCare/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var activity = await _careService.GetCareActivityByIdAsync(id);
                var plantId = activity?.PlantId;
                await _careService.DeleteCareActivityAsync(id);
                TempData["SuccessMessage"] = "Plant care activity has been deleted successfully!";
                
                if (plantId.HasValue)
                {
                    return RedirectToAction("Details", "Plants", new { id = plantId.Value });
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the activity: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}