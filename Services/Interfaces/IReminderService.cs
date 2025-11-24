using GardenHub.Models;
using GardenHub.Models.Enums;

namespace GardenHub.Services.Interfaces
{
    public interface IReminderService
    {
        // Basic CRUD operations
        Task<List<Reminder>> GetAllRemindersAsync();
        Task<List<Reminder>> GetRemindersByUserIdAsync(string userId);
        Task<Reminder?> GetReminderByIdAsync(int reminderId);
        Task<Reminder> CreateReminderAsync(Reminder reminder);
        Task<Reminder?> UpdateReminderAsync(int reminderId, Reminder reminder);
        Task<bool> DeleteReminderAsync(int reminderId);

        // Filtered queries
        Task<List<Reminder>> GetActiveRemindersAsync(string userId);
        Task<List<Reminder>> GetCompletedRemindersAsync(string userId);
        Task<List<Reminder>> GetOverdueRemindersAsync(string userId);
        Task<List<Reminder>> GetUpcomingRemindersAsync(string userId, int days = 7);
        Task<List<Reminder>> GetRemindersByDateRangeAsync(string userId, DateTime startDate, DateTime endDate);
        Task<List<Reminder>> GetRemindersByTypeAsync(string userId, ReminderType type);
        Task<List<Reminder>> GetRemindersByPriorityAsync(string userId, ReminderPriority priority);
        Task<List<Reminder>> GetRemindersByGardenIdAsync(int gardenId);

        // Reminder actions
        Task<bool> MarkAsCompletedAsync(int reminderId);
        Task<bool> MarkAsIncompleteAsync(int reminderId);
        Task<Reminder?> SnoozeReminderAsync(int reminderId, int hours);

        // Recurring reminders
        Task<Reminder?> CreateNextRecurrenceAsync(int reminderId);
        
        // Helper methods
        Task<List<Garden>> GetAllGardensAsync();
        Task<List<DailyRecord>> GetAllDailyRecordsAsync();
        Task<List<GardenCareActivity>> GetAllGardenCareActivitiesAsync();
    }
}