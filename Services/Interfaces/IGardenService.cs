using GardenHub.Models;

namespace GardenHub.Services.Interfaces
{
    public interface IGardenService
    {
        Task<List<Garden?>> GetAllGardens();
        Task<Garden?> GetGardenById(int id);
        Task<Garden?> GetGardenWithDetailsById(int id); // Fixed: Task<Garden?> instead of Task<Garden>?
        Task CreateGarden(Garden garden);
        Task UpdateGarden(Garden garden, int id);
        Task<bool> DeleteGarden(int id);
        Task<IEnumerable<AppUser>> GetAllUsers();
        Task<bool> GardenExists(int id);
    }
}
