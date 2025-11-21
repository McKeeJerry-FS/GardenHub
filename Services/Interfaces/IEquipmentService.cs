using GardenHub.Models;
using GardenHub.Models.Enums;

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

        // Maintenance methods
        Task<MaintenanceRecord> RequestMaintenanceAsync(int equipmentId, string userId, string? notes);
        Task<MaintenanceRecord> StartMaintenanceAsync(int equipmentId, string userId);
        Task<MaintenanceRecord> CompleteMaintenanceAsync(int equipmentId, string userId, string? notes);
        Task<List<MaintenanceRecord>> GetMaintenanceHistoryAsync(int equipmentId);
        Task<MaintenanceRecord?> GetActiveMaintenanceRecordAsync(int equipmentId);

        // Equipment status queries
        Task<List<Equipment>> GetEquipmentByStatusAsync(MaintenanceStatus status);
        Task<List<Equipment>> GetEquipmentByGardenAndStatusAsync(int gardenId, MaintenanceStatus status);
        Task<List<Equipment>> GetEquipmentByGardenAsync(int gardenId);
    }
}
