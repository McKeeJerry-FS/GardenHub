using GardenHub.Models;
using GardenHub.Models.Enums;

namespace GardenHub.Services.Interfaces
{
    public interface IPlantService
    {
        Task<Plant?> GetPlantByIdAsync(int id);
        Task<List<Plant>> GetAllPlantsAsync();
        Task CreatePlantAsync(Plant plant);
        Task UpdatePlantAsync(int id, Plant plant);
        Task<bool> DeletePlantAsync(int id);

        // Helper methods for controller select lists
        Task<List<Garden>> GetAllGardensAsync();
        Task<List<AppUser>> GetAllUsersAsync();
        
        // ⚠️ Add these new methods:
        Task<List<Plant>> GetPlantsByGardenIdAsync(int gardenId);
        Task<List<Plant>> GetPlantsByUserIdAsync(string userId);
        Task<List<Plant>> SearchPlantsAsync(string searchTerm);
    }
}
