using GardenHub.Models;
using GardenHub.Models.Enums;

namespace GardenHub.Services.Interfaces
{
    public interface IGardenCareService
    {
        Task<List<GardenCareActivity>> GetAllCareActivitiesAsync();
        Task<List<GardenCareActivity>> GetCareActivitiesByGardenIdAsync(int gardenId);
        Task<List<GardenCareActivity>> GetCareActivitiesByUserIdAsync(string userId);
        Task<GardenCareActivity?> GetCareActivityByIdAsync(int activityId);
        Task CreateCareActivityAsync(GardenCareActivity activity);
        Task UpdateCareActivityAsync(int activityId, GardenCareActivity activity);
        Task<bool> DeleteCareActivityAsync(int activityId);
        Task<List<GardenCareActivity>> GetCareActivitiesByDateRangeAsync(int gardenId, DateTime startDate, DateTime endDate);
        Task<List<GardenCareActivity>> GetCareActivitiesByTypeAsync(int gardenId, CareActivityType activityType);
        Task<GardenCareActivity?> GetMostRecentActivityAsync(int gardenId);
        Task<int> GetTotalActivitiesCountAsync(int gardenId);
        Task<double> GetAverageActivityDurationAsync(int gardenId);
        Task<Garden?> GetGardenWithCareActivitiesAsync(int gardenId);
        
        // Helper methods
        Task<List<Garden>> GetAllGardensAsync();
        Task<List<AppUser>> GetAllUsersAsync();
    }
}