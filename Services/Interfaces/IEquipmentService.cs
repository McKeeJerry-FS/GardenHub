using GardenHub.Models;

namespace GardenHub.Services.Interfaces
{
    public interface IEquipmentService
    {
        Task<List<Equipment>> GetAllEquipmentsAsync();
        Task<Equipment> GetEquipmentByIdAsync(int id);
        Task CreateEquipmentAsync(Equipment equipment);
        Task UpdateEquipmentAsync(int id, Equipment equipment);
        Task DeleteEquipmentAsync(int id);

        // Helper methods for controller select lists
        Task<List<Garden>> GetAllGardensAsync();
        Task<List<AppUser>> GetAllUsersAsync();
        Task<bool> EquipmentExistsAsync(int id);
    }
}
