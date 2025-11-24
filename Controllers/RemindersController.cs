using GardenHub.Models;
using GardenHub.Models.Enums;
using GardenHub.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace GardenHub.Controllers
{
    [Authorize]
    public class RemindersController : Controller
    {
        private readonly IReminderService _reminderService;

        public RemindersController(IReminderService reminderService)
        {
            _reminderService = reminderService;
        }

        // GET: Reminders
        public async Task<IActionResult> Index(string filter = "active")
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            List<Reminder> reminders;

            switch (filter.ToLower())
            {
                case "completed":
                    reminders = await _reminderService.GetCompletedRemindersAsync(userId!);
                    break;
                case "overdue":
                    reminders = await _reminderService.GetOverdueRemindersAsync(userId!);
                    break;
                case "upcoming":
                    reminders = await _reminderService.GetUpcomingRemindersAsync(userId!, 7);
                    break;
                default:
                    reminders = await _reminderService.GetActiveRemindersAsync(userId!);
                    break;
            }

            ViewData["CurrentFilter"] = filter;
            return View(reminders);
        }

        // GET: Reminders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var reminder = await _reminderService.GetReminderByIdAsync(id.Value);
            if (reminder == null) return NotFound();

            return View(reminder);
        }

        // GET: Reminders/Create
        public async Task<IActionResult> Create(int? gardenId, int? recordId, int? activityId)
        {
            await PopulateDropdowns(gardenId, recordId, activityId);
            
            var model = new Reminder
            {
                ReminderDateTime = DateTime.Now.AddDays(1),
                GardenId = gardenId,
                DailyRecordId = recordId,
                GardenCareActivityId = activityId
            };

            return View(model);
        }

        // POST: Reminders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,ReminderDateTime,ReminderType,Priority,IsRecurring,RecurrencePattern,RecurrenceInterval,GardenId,DailyRecordId,GardenCareActivityId")] Reminder reminder)
        {
            // Ensure DateTime is in UTC
            if (reminder.ReminderDateTime.Kind == DateTimeKind.Unspecified)
            {
                reminder.ReminderDateTime = DateTime.SpecifyKind(reminder.ReminderDateTime, DateTimeKind.Utc);
            }
            else if (reminder.ReminderDateTime.Kind == DateTimeKind.Local)
            {
                reminder.ReminderDateTime = reminder.ReminderDateTime.ToUniversalTime();
            }

            ModelState.Remove("User");
            ModelState.Remove("Garden");
            ModelState.Remove("DailyRecord");
            ModelState.Remove("GardenCareActivity");

            if (ModelState.IsValid)
            {
                try
                {
                    reminder.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                    await _reminderService.CreateReminderAsync(reminder);
                    TempData["SuccessMessage"] = "Reminder created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error creating reminder: {ex.Message}";
                }
            }

            await PopulateDropdowns(reminder.GardenId, reminder.DailyRecordId, reminder.GardenCareActivityId);
            return View(reminder);
        }

        // GET: Reminders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var reminder = await _reminderService.GetReminderByIdAsync(id.Value);
            if (reminder == null) return NotFound();

            await PopulateDropdowns(reminder.GardenId, reminder.DailyRecordId, reminder.GardenCareActivityId);
            return View(reminder);
        }

        // POST: Reminders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReminderId,Title,Description,ReminderDateTime,ReminderType,Priority,IsRecurring,RecurrencePattern,RecurrenceInterval,GardenId,DailyRecordId,GardenCareActivityId,UserId")] Reminder reminder)
        {
            if (id != reminder.ReminderId) return NotFound();

            // Ensure DateTime is in UTC
            if (reminder.ReminderDateTime.Kind == DateTimeKind.Unspecified)
            {
                reminder.ReminderDateTime = DateTime.SpecifyKind(reminder.ReminderDateTime, DateTimeKind.Utc);
            }
            else if (reminder.ReminderDateTime.Kind == DateTimeKind.Local)
            {
                reminder.ReminderDateTime = reminder.ReminderDateTime.ToUniversalTime();
            }

            ModelState.Remove("User");
            ModelState.Remove("Garden");
            ModelState.Remove("DailyRecord");
            ModelState.Remove("GardenCareActivity");

            if (ModelState.IsValid)
            {
                try
                {
                    var updated = await _reminderService.UpdateReminderAsync(id, reminder);
                    if (updated == null)
                    {
                        TempData["ErrorMessage"] = "Reminder no longer exists.";
                        return RedirectToAction(nameof(Index));
                    }
                    TempData["SuccessMessage"] = "Reminder updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error updating reminder: {ex.Message}";
                }
            }

            await PopulateDropdowns(reminder.GardenId, reminder.DailyRecordId, reminder.GardenCareActivityId);
            return View(reminder);
        }

        // GET: Reminders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var reminder = await _reminderService.GetReminderByIdAsync(id.Value);
            if (reminder == null) return NotFound();

            return View(reminder);
        }

        // POST: Reminders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _reminderService.DeleteReminderAsync(id);
                TempData["SuccessMessage"] = "Reminder deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting reminder: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Reminders/Complete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            try
            {
                await _reminderService.MarkAsCompletedAsync(id);
                TempData["SuccessMessage"] = "Reminder marked as completed!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error completing reminder: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Reminders/Snooze/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Snooze(int id, int hours = 1)
        {
            try
            {
                await _reminderService.SnoozeReminderAsync(id, hours);
                TempData["SuccessMessage"] = $"Reminder snoozed for {hours} hour(s)!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error snoozing reminder: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Reminders/QuickCreate (for Dashboard modal)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickCreate(string Title, string? Description, string ReminderDate, string ReminderTime, 
            ReminderType ReminderType, ReminderPriority Priority = ReminderPriority.Normal, 
            bool IsRecurring = false, RecurrencePattern? RecurrencePattern = null, int? RecurrenceInterval = null, int? GardenId = null)
        {
            try
            {
                // Combine date and time
                var dateTime = DateTime.Parse($"{ReminderDate} {ReminderTime}");
                
                // Ensure DateTime is in UTC
                if (dateTime.Kind == DateTimeKind.Unspecified)
                {
                    dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                }
                else if (dateTime.Kind == DateTimeKind.Local)
                {
                    dateTime = dateTime.ToUniversalTime();
                }

                var reminder = new Reminder
                {
                    Title = Title,
                    Description = Description,
                    ReminderDateTime = dateTime,
                    ReminderType = ReminderType,
                    Priority = Priority,
                    IsRecurring = IsRecurring,
                    RecurrencePattern = IsRecurring ? RecurrencePattern : null,
                    RecurrenceInterval = IsRecurring ? RecurrenceInterval : null,
                    GardenId = GardenId,
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!
                };

                await _reminderService.CreateReminderAsync(reminder);
                TempData["SuccessMessage"] = "Reminder created successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating reminder: {ex.Message}";
            }

            return RedirectToAction("Dashboard", "Home");
        }

        private async Task PopulateDropdowns(int? gardenId = null, int? recordId = null, int? activityId = null)
        {
            var gardens = await _reminderService.GetAllGardensAsync();
            var records = await _reminderService.GetAllDailyRecordsAsync();
            var activities = await _reminderService.GetAllGardenCareActivitiesAsync();

            ViewData["GardenId"] = new SelectList(gardens, "GardenId", "GardenDescription", gardenId);
            ViewData["DailyRecordId"] = new SelectList(records, "RecordId", "CreatedDate", recordId);
            ViewData["GardenCareActivityId"] = new SelectList(activities, "ActivityId", "ActivityType", activityId);
        }
    }
}