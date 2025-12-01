using GardenHub.Data;
using GardenHub.Models;
using GardenHub.Models.Enums;
using GardenHub.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GardenHub.Services
{
    public class ReminderService : IReminderService
    {
        private readonly ApplicationDbContext _context;

        public ReminderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Reminder> CreateReminderAsync(Reminder reminder)
        {
            if (reminder is null)
            {
                throw new ArgumentNullException(nameof(reminder), "Reminder cannot be null");
            }

            reminder.CreatedDate = DateTime.UtcNow;
            _context.Reminders.Add(reminder);
            await _context.SaveChangesAsync();
            return reminder;
        }

        public async Task<bool> DeleteReminderAsync(int reminderId)
        {
            var reminder = await _context.Reminders.FindAsync(reminderId);
            if (reminder != null)
            {
                _context.Reminders.Remove(reminder);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<List<Reminder>> GetActiveRemindersAsync(string userId)
        {
            return await _context.Reminders
                .Where(r => r.UserId == userId && !r.IsCompleted)
                .Include(r => r.Garden)
                .Include(r => r.DailyRecord)
                .Include(r => r.GardenCareActivity)
                .OrderBy(r => r.ReminderDateTime)
                .ToListAsync();
        }

        public async Task<List<Reminder>> GetAllRemindersAsync()
        {
            return await _context.Reminders
                .Include(r => r.User)
                .Include(r => r.Garden)
                .Include(r => r.DailyRecord)
                .Include(r => r.GardenCareActivity)
                .OrderByDescending(r => r.ReminderDateTime)
                .ToListAsync();
        }

        public async Task<List<Reminder>> GetCompletedRemindersAsync(string userId)
        {
            return await _context.Reminders
                .Where(r => r.UserId == userId && r.IsCompleted)
                .Include(r => r.Garden)
                .Include(r => r.DailyRecord)
                .Include(r => r.GardenCareActivity)
                .OrderByDescending(r => r.CompletedDate)
                .ToListAsync();
        }

        public async Task<List<Reminder>> GetOverdueRemindersAsync(string userId)
        {
            var now = DateTime.UtcNow;
            return await _context.Reminders
                .Where(r => r.UserId == userId && !r.IsCompleted && r.ReminderDateTime < now)
                .Include(r => r.Garden)
                .Include(r => r.DailyRecord)
                .Include(r => r.GardenCareActivity)
                .OrderBy(r => r.ReminderDateTime)
                .ToListAsync();
        }

        public async Task<Reminder?> GetReminderByIdAsync(int reminderId)
        {
            return await _context.Reminders
                .Include(r => r.User)
                .Include(r => r.Garden)
                .Include(r => r.DailyRecord)
                .Include(r => r.GardenCareActivity)
                .FirstOrDefaultAsync(r => r.ReminderId == reminderId);
        }

        public async Task<List<Reminder>> GetRemindersByDateRangeAsync(string userId, DateTime startDate, DateTime endDate)
        {
            return await _context.Reminders
                .Where(r => r.UserId == userId && r.ReminderDateTime >= startDate && r.ReminderDateTime <= endDate)
                .Include(r => r.Garden)
                .Include(r => r.DailyRecord)
                .Include(r => r.GardenCareActivity)
                .OrderBy(r => r.ReminderDateTime)
                .ToListAsync();
        }

        public async Task<List<Reminder>> GetRemindersByGardenIdAsync(int gardenId)
        {
            return await _context.Reminders
                .Where(r => r.GardenId == gardenId)
                .Include(r => r.Garden)
                .Include(r => r.User)
                .OrderBy(r => r.ReminderDateTime)
                .ToListAsync();
        }

        public async Task<List<Reminder>> GetRemindersByPriorityAsync(string userId, ReminderPriority priority)
        {
            return await _context.Reminders
                .Where(r => r.UserId == userId && r.Priority == priority && !r.IsCompleted)
                .Include(r => r.Garden)
                .Include(r => r.DailyRecord)
                .Include(r => r.GardenCareActivity)
                .OrderBy(r => r.ReminderDateTime)
                .ToListAsync();
        }

        public async Task<List<Reminder>> GetRemindersByTypeAsync(string userId, ReminderType type)
        {
            return await _context.Reminders
                .Where(r => r.UserId == userId && r.ReminderType == type && !r.IsCompleted)
                .Include(r => r.Garden)
                .Include(r => r.DailyRecord)
                .Include(r => r.GardenCareActivity)
                .OrderBy(r => r.ReminderDateTime)
                .ToListAsync();
        }

        public async Task<List<Reminder>> GetRemindersByUserIdAsync(string userId)
        {
            return await _context.Reminders
                .Where(r => r.UserId == userId)
                .Include(r => r.Garden)
                .Include(r => r.DailyRecord)
                .Include(r => r.GardenCareActivity)
                .OrderBy(r => r.ReminderDateTime)
                .ToListAsync();
        }

        public async Task<List<Reminder>> GetUpcomingRemindersAsync(string userId, int days = 7)
        {
            var now = DateTime.UtcNow;
            var endDate = now.AddDays(days);
            
            return await _context.Reminders
                .Where(r => r.UserId == userId && !r.IsCompleted && r.ReminderDateTime >= now && r.ReminderDateTime <= endDate)
                .Include(r => r.Garden)
                .Include(r => r.DailyRecord)
                .Include(r => r.GardenCareActivity)
                .OrderBy(r => r.ReminderDateTime)
                .ToListAsync();
        }

        public async Task<bool> MarkAsCompletedAsync(int reminderId)
        {
            var reminder = await _context.Reminders.FindAsync(reminderId);
            if (reminder != null)
            {
                reminder.IsCompleted = true;
                reminder.CompletedDate = DateTime.UtcNow;
                reminder.LastModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Create next occurrence if recurring
                if (reminder.IsRecurring && reminder.RecurrencePattern.HasValue)
                {
                    await CreateNextRecurrenceAsync(reminderId);
                }

                return true;
            }
            return false;
        }

        public async Task<bool> MarkAsIncompleteAsync(int reminderId)
        {
            var reminder = await _context.Reminders.FindAsync(reminderId);
            if (reminder != null)
            {
                reminder.IsCompleted = false;
                reminder.CompletedDate = null;
                reminder.LastModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<Reminder?> SnoozeReminderAsync(int reminderId, int hours)
        {
            var reminder = await _context.Reminders.FindAsync(reminderId);
            if (reminder != null)
            {
                reminder.ReminderDateTime = reminder.ReminderDateTime.AddHours(hours);
                reminder.LastModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return reminder;
            }
            return null;
        }

        public async Task<Reminder?> UpdateReminderAsync(int reminderId, Reminder reminder)
        {
            var existingReminder = await _context.Reminders.FindAsync(reminderId);
            if (existingReminder != null)
            {
                existingReminder.Title = reminder.Title;
                existingReminder.Description = reminder.Description;
                existingReminder.ReminderDateTime = reminder.ReminderDateTime;
                existingReminder.ReminderType = reminder.ReminderType;
                existingReminder.Priority = reminder.Priority;
                existingReminder.IsRecurring = reminder.IsRecurring;
                existingReminder.RecurrencePattern = reminder.RecurrencePattern;
                existingReminder.RecurrenceInterval = reminder.RecurrenceInterval;
                existingReminder.GardenId = reminder.GardenId;
                existingReminder.DailyRecordId = reminder.DailyRecordId;
                existingReminder.GardenCareActivityId = reminder.GardenCareActivityId;
                existingReminder.LastModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return existingReminder;
            }
            return null;
        }

        public async Task<Reminder?> CreateNextRecurrenceAsync(int reminderId)
        {
            var originalReminder = await _context.Reminders.FindAsync(reminderId);
            if (originalReminder == null || !originalReminder.IsRecurring || !originalReminder.RecurrencePattern.HasValue)
            {
                return null;
            }

            var interval = originalReminder.RecurrenceInterval ?? 1;
            DateTime nextReminderDate = originalReminder.ReminderDateTime;

            // Calculate next occurrence based on pattern
            nextReminderDate = originalReminder.RecurrencePattern.Value switch
            {
                RecurrencePattern.Daily => nextReminderDate.AddDays(interval),
                RecurrencePattern.Weekly => nextReminderDate.AddDays(7 * interval),
                RecurrencePattern.BiWeekly => nextReminderDate.AddDays(14 * interval),
                RecurrencePattern.Monthly => nextReminderDate.AddMonths(interval),
                RecurrencePattern.Quarterly => nextReminderDate.AddMonths(3 * interval),
                RecurrencePattern.Yearly => nextReminderDate.AddYears(interval),
                _ => nextReminderDate.AddDays(interval)
            };

            // Create new reminder
            var newReminder = new Reminder
            {
                Title = originalReminder.Title,
                Description = originalReminder.Description,
                ReminderDateTime = nextReminderDate,
                ReminderType = originalReminder.ReminderType,
                Priority = originalReminder.Priority,
                IsRecurring = originalReminder.IsRecurring,
                RecurrencePattern = originalReminder.RecurrencePattern,
                RecurrenceInterval = originalReminder.RecurrenceInterval,
                GardenId = originalReminder.GardenId,
                DailyRecordId = originalReminder.DailyRecordId,
                GardenCareActivityId = originalReminder.GardenCareActivityId,
                UserId = originalReminder.UserId,
                CreatedDate = DateTime.UtcNow
            };

            _context.Reminders.Add(newReminder);
            await _context.SaveChangesAsync();
            return newReminder;
        }

        // Helper methods
        public async Task<List<Garden>> GetAllGardensAsync()
        {
            return await _context.Gardens.ToListAsync();
        }

        public async Task<List<DailyRecord>> GetAllDailyRecordsAsync()
        {
            return await _context.DailyRecords.Include(d => d.Garden).ToListAsync();
        }

        public async Task<List<GardenCareActivity>> GetAllGardenCareActivitiesAsync()
        {
            return await _context.GardenCareActivities.Include(g => g.Garden).ToListAsync();
        }
    }
}