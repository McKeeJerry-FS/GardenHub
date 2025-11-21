using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GardenHub.Models;
using GardenHub.Services.Interfaces;
using GardenHub.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace GardenHub.Controllers
{
    [Authorize]
    public class DailyRecordsController : Controller
    {
        private readonly IDailyRecordService _dailyRecordService;

        public DailyRecordsController(IDailyRecordService dailyRecordService)
        {
            _dailyRecordService = dailyRecordService;
        }

        // GET: DailyRecords
        public async Task<IActionResult> Index(PlantCondition? condition)
        {
            IEnumerable<DailyRecord> records;
            if (condition.HasValue)
            {
                records = await _dailyRecordService.GetDailyRecordsByConditionAsync(condition.Value);
            }
            else
            {
                records = await _dailyRecordService.GetAllDailyRecordsAsync();
            }

            // Build SelectList of PlantCondition enum values for the view
            var conditionItems = Enum.GetValues(typeof(PlantCondition))
                                     .Cast<PlantCondition>()
                                     .Select(c => new { Id = (int)c, Name = c.ToString() })
                                     .ToList();

            ViewData["Condition"] = new SelectList(conditionItems, "Id", "Name", condition.HasValue ? (int)condition.Value : (int?)null);

            return View(records);
        }

        // GET: DailyRecords/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dailyRecord = await _dailyRecordService.GetDailyRecordByIdAsync(id.Value);
            if (dailyRecord == null)
            {
                return NotFound();
            }

            return View(dailyRecord);
        }

        // GET: DailyRecords/Create
        public async Task<IActionResult> Create()
        {
            var gardens = await _dailyRecordService.GetAllGardensAsync();
            var users = await _dailyRecordService.GetAllUsersAsync();
            ViewData["GardenId"] = new SelectList(gardens, "GardenId", "GardenDescription");
            ViewData["UserId"] = new SelectList(users, "Id", "Id");
            return View();
        }

        // POST: DailyRecords/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RecordId,GardenId,CreatedDate,InsideTemperature,OutsideTemperature,InsideHumidity,OutsideHumidity,InsideVPD,OutsideVPD,LightingOn,LightingOff,LightingIntensity,WaterAmount,NutrientAmount,Notes,UserId")] DailyRecord dailyRecord)
        {
            // Ensure DateTime is in UTC for PostgreSQL
            if (dailyRecord.CreatedDate.Kind == DateTimeKind.Unspecified)
            {
                dailyRecord.CreatedDate = DateTime.SpecifyKind(dailyRecord.CreatedDate, DateTimeKind.Utc);
            }
            else if (dailyRecord.CreatedDate.Kind == DateTimeKind.Local)
            {
                dailyRecord.CreatedDate = dailyRecord.CreatedDate.ToUniversalTime();
            }

            ModelState.Remove("Garden"); // Remove the navigation property from validation
            ModelState.Remove("User");   // Also remove User navigation property
            
            if (ModelState.IsValid)
            {
                try
                {
                    // Get the current logged-in user's ID
                    dailyRecord.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    await _dailyRecordService.CreateDailyRecordAsync(dailyRecord);
                    TempData["SuccessMessage"] = "Daily record has been created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"An error occurred while creating the daily record: {ex.Message}";
                }
            }

            var gardens = await _dailyRecordService.GetAllGardensAsync();
            var users = await _dailyRecordService.GetAllUsersAsync();
            ViewData["GardenId"] = new SelectList(gardens, "GardenId", "GardenDescription", dailyRecord.GardenId);
            ViewData["UserId"] = new SelectList(users, "Id", "Id", dailyRecord.UserId);
            return View(dailyRecord);
        }

        // GET: DailyRecords/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dailyRecord = await _dailyRecordService.GetDailyRecordByIdAsync(id.Value);
            if (dailyRecord == null)
            {
                return NotFound();
            }

            var gardens = await _dailyRecordService.GetAllGardensAsync();
            var users = await _dailyRecordService.GetAllUsersAsync();
            ViewData["GardenId"] = new SelectList(gardens, "GardenId", "GardenDescription", dailyRecord.GardenId);
            ViewData["UserId"] = new SelectList(users, "Id", "Id", dailyRecord.UserId);
            return View(dailyRecord);
        }

        // POST: DailyRecords/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RecordId,GardenId,CreatedDate,InsideTemperature,OutsideTemperature,InsideHumidity,OutsideHumidity,InsideVPD,OutsideVPD,LightingOn,LightingOff,LightingIntensity,WaterAmount,NutrientAmount,Notes,UserId")] DailyRecord dailyRecord)
        {
            if (id != dailyRecord.RecordId)
            {
                return NotFound();
            }

            // Ensure DateTime is in UTC for PostgreSQL
            if (dailyRecord.CreatedDate.Kind == DateTimeKind.Unspecified)
            {
                dailyRecord.CreatedDate = DateTime.SpecifyKind(dailyRecord.CreatedDate, DateTimeKind.Utc);
            }
            else if (dailyRecord.CreatedDate.Kind == DateTimeKind.Local)
            {
                dailyRecord.CreatedDate = dailyRecord.CreatedDate.ToUniversalTime();
            }

            ModelState.Remove("Garden"); // Remove the navigation property from validation
            ModelState.Remove("User");   // Also remove User navigation property

            if (ModelState.IsValid)
            {
                try
                {
                    var updated = await _dailyRecordService.UpdateDailyRecordAsync(id, dailyRecord);
                    if (updated == null)
                    {
                        TempData["ErrorMessage"] = "The daily record you are trying to update no longer exists.";
                        return RedirectToAction(nameof(Index));
                    }
                    TempData["SuccessMessage"] = "Daily record has been updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"An error occurred while updating the daily record: {ex.Message}";
                }
            }

            var gardens = await _dailyRecordService.GetAllGardensAsync();
            var users = await _dailyRecordService.GetAllUsersAsync();
            ViewData["GardenId"] = new SelectList(gardens, "GardenId", "GardenDescription", dailyRecord.GardenId);
            ViewData["UserId"] = new SelectList(users, "Id", "Id", dailyRecord.UserId);
            return View(dailyRecord);
        }

        // GET: DailyRecords/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dailyRecord = await _dailyRecordService.GetDailyRecordByIdAsync(id.Value);
            if (dailyRecord == null)
            {
                return NotFound();
            }

            return View(dailyRecord);
        }

        // POST: DailyRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var deleted = await _dailyRecordService.DeleteDailyRecordAsync(id);
                if (!deleted)
                {
                    TempData["ErrorMessage"] = "The daily record you are trying to delete no longer exists.";
                }
                else
                {
                    TempData["SuccessMessage"] = "Daily record has been deleted successfully!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the daily record: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: DailyRecords/Dashboard
        public async Task<IActionResult> Dashboard(int? gardenId, int days = 30)
        {
            IEnumerable<DailyRecord> records;
            
            // Filter by garden if specified
            if (gardenId.HasValue)
            {
                records = await _dailyRecordService.GetDailyRecordsByGardenIdAsync(gardenId.Value);
            }
            else
            {
                records = await _dailyRecordService.GetAllDailyRecordsAsync();
            }

            // Filter by date range
            var startDate = DateTime.UtcNow.AddDays(-days);
            records = records.Where(r => r.CreatedDate >= startDate)
                            .OrderBy(r => r.CreatedDate)
                            .ToList();

            // Get gardens for filter dropdown
            var gardens = await _dailyRecordService.GetAllGardensAsync();
            ViewData["GardenId"] = new SelectList(gardens, "GardenId", "GardenDescription", gardenId);
            ViewData["Days"] = days;
            
            return View(records);
        }

        private async Task<bool> DailyRecordExists(int id)
        {
            var record = await _dailyRecordService.GetDailyRecordByIdAsync(id);
            return record != null;
        }
    }
}
