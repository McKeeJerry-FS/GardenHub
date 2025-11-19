using GardenHub.Models;
using GardenHub.Models.Enums;

namespace GardenHub.Services.Interfaces
{
    public interface IDailyRecordService
    {
        Task<List<DailyRecord>> GetAllDailyRecordsAsync();
        Task<List<DailyRecord>> GetDailyRecordsByGardenIdAsync(int gardenId);
        Task<List<DailyRecord>> GetDailyRecordsByUserIdAsync(string userId);
        Task<DailyRecord?> GetDailyRecordByIdAsync(int recordId);
        Task<DailyRecord> CreateDailyRecordAsync(DailyRecord dailyRecord);
        Task<DailyRecord?> UpdateDailyRecordAsync(int recordId, DailyRecord updatedRecord);
        Task<bool> DeleteDailyRecordAsync(int recordId);
        Task<List<DailyRecord>> GetDailyRecordsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<DailyRecord>> GetDailyRecordsByConditionAsync(PlantCondition condition);

        // Helper methods for controller select lists
        Task<List<Garden>> GetAllGardensAsync();
        Task<List<AppUser>> GetAllUsersAsync();
    }
}
