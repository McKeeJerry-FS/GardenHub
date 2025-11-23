using GardenHub.Models;
using GardenHub.Models.Enums;

namespace GardenHub.Services.Interfaces
{
    public interface IPlantCareService
    {
        Task<List<PlantCareActivity>> GetAllCareActivitiesAsync();
        Task<List<PlantCareActivity>> GetCareActivitiesByPlantIdAsync(int plantId);
        Task<List<PlantCareActivity>> GetCareActivitiesByUserIdAsync(string userId);
        Task<PlantCareActivity?> GetCareActivityByIdAsync(int activityId);
        Task CreateCareActivityAsync(PlantCareActivity activity);
        Task UpdateCareActivityAsync(int activityId, PlantCareActivity activity);
        Task<bool> DeleteCareActivityAsync(int activityId);
        Task<List<PlantCareActivity>> GetCareActivitiesByDateRangeAsync(int plantId, DateTime startDate, DateTime endDate);
        Task<List<PlantCareActivity>> GetCareActivitiesByTypeAsync(int plantId, PlantCareActivityType activityType);
        Task<PlantCareActivity?> GetMostRecentActivityAsync(int plantId);
        Task<int> GetTotalActivitiesCountAsync(int plantId);
        Task<double> GetAverageActivityDurationAsync(int plantId);
        Task<Plant?> GetPlantWithCareActivitiesAsync(int plantId);
        
        // Helper methods
        Task<List<Plant>> GetAllPlantsAsync();
        Task<List<AppUser>> GetAllUsersAsync();
    }
}