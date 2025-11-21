using GardenHub.Data;
using GardenHub.Models;
using GardenHub.Models.Enums;
using GardenHub.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GardenHub.Services
{
    public class EquipmentService : IEquipmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;

        public EquipmentService(ApplicationDbContext context, IImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }

        public async Task CreateEquipmentAsync(Equipment equipment)
        {
            if (equipment == null)
                throw new ArgumentNullException(nameof(equipment));

            _context.Equipments.Add(equipment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteEquipmentAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid equipment ID.", nameof(id));
            
            var equipment = await _context.Equipments.FirstOrDefaultAsync(e => e.EquipmentId == id);
            if (equipment == null)
                throw new KeyNotFoundException($"Equipment with ID {id} not found.");
            
            _context.Equipments.Remove(equipment);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Equipment>> GetAllEquipmentsAsync()
        {
            return await _context.Equipments
                .AsNoTracking()
                .Include(e => e.Garden)
                .Include(e => e.User)
                .OrderBy(e => e.EquipmentName)
                .ToListAsync();
        }

        public async Task<Equipment> GetEquipmentByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid equipment ID.", nameof(id));
                
            var equipment = await _context.Equipments
                .Include(e => e.Garden)
                .Include(e => e.User)
                .Include(e => e.MaintenanceRecords)
                    .ThenInclude(mr => mr.RequestedBy)
                .FirstOrDefaultAsync(e => e.EquipmentId == id);
            
            if (equipment == null)
                throw new KeyNotFoundException($"Equipment with ID {id} not found.");
            
            return equipment;
        }

        public async Task UpdateEquipmentAsync(int id, Equipment equipment)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid equipment ID.", nameof(id));
            
            if (equipment == null)
                throw new ArgumentNullException(nameof(equipment));
            
            var existingEquipment = await _context.Equipments.FindAsync(id);
            if (existingEquipment == null)
                throw new KeyNotFoundException($"Equipment with ID {id} not found.");
            
            // Store existing image data before update
            var preserveImageData = existingEquipment.ImageData;
            var preserveImageType = existingEquipment.ImageType;

            // Update all properties from the equipment parameter
            _context.Entry(existingEquipment).CurrentValues.SetValues(equipment);

            // Handle image update logic
            if (equipment.ImageFile != null)
            {
                // New image provided - update it
                existingEquipment.ImageData = await _imageService.ConvertFileToByteArrayAsync(equipment.ImageFile);
                existingEquipment.ImageType = equipment.ImageFile.ContentType;
            }
            else
            {
                // No new image - preserve existing image
                existingEquipment.ImageData = preserveImageData;
                existingEquipment.ImageType = preserveImageType;
            }
            
            await _context.SaveChangesAsync();
        }

        public async Task<bool> EquipmentExistsAsync(int id)
        {
            return await _context.Equipments.AnyAsync(e => e.EquipmentId == id);
        }

        // Helper methods for controller select lists
        public async Task<List<AppUser>> GetAllUsersAsync()
        {
            return await _context.Users
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();
        }

        public async Task<List<Garden>> GetAllGardensAsync()
        {
            return await _context.Gardens
                .OrderBy(g => g.GardenName)
                .ToListAsync();
        }

        // Maintenance methods
        public async Task<MaintenanceRecord> RequestMaintenanceAsync(int equipmentId, string userId, string? notes)
        {
            var equipment = await _context.Equipments.FindAsync(equipmentId);
            if (equipment == null)
                throw new KeyNotFoundException($"Equipment with ID {equipmentId} not found.");

            // Check if there's already an active maintenance request
            var activeRecord = await GetActiveMaintenanceRecordAsync(equipmentId);
            if (activeRecord != null)
                throw new InvalidOperationException("There is already an active maintenance request for this equipment.");

            var maintenanceRecord = new MaintenanceRecord
            {
                EquipmentId = equipmentId,
                Status = MaintenanceStatus.MaintenanceRequested,
                RequestDate = DateTime.UtcNow,
                RequestedByUserId = userId,
                Notes = notes
            };

            equipment.MaintenanceStatus = MaintenanceStatus.MaintenanceRequested;

            _context.MaintenanceRecords.Add(maintenanceRecord);
            await _context.SaveChangesAsync();

            return maintenanceRecord;
        }

        public async Task<MaintenanceRecord> StartMaintenanceAsync(int equipmentId, string userId)
        {
            var equipment = await _context.Equipments.FindAsync(equipmentId);
            if (equipment == null)
                throw new KeyNotFoundException($"Equipment with ID {equipmentId} not found.");

            var activeRecord = await GetActiveMaintenanceRecordAsync(equipmentId);
            if (activeRecord == null)
                throw new InvalidOperationException("No active maintenance request found for this equipment.");

            if (activeRecord.Status != MaintenanceStatus.MaintenanceRequested)
                throw new InvalidOperationException("Equipment must be in 'Maintenance Requested' status to start maintenance.");

            activeRecord.Status = MaintenanceStatus.UnderMaintenance;
            activeRecord.MaintenanceStartDate = DateTime.UtcNow;
            equipment.MaintenanceStatus = MaintenanceStatus.UnderMaintenance;

            await _context.SaveChangesAsync();

            return activeRecord;
        }

        public async Task<MaintenanceRecord> CompleteMaintenanceAsync(int equipmentId, string userId, string? notes)
        {
            var equipment = await _context.Equipments.FindAsync(equipmentId);
            if (equipment == null)
                throw new KeyNotFoundException($"Equipment with ID {equipmentId} not found.");

            var activeRecord = await GetActiveMaintenanceRecordAsync(equipmentId);
            if (activeRecord == null)
                throw new InvalidOperationException("No active maintenance record found for this equipment.");

            if (activeRecord.Status != MaintenanceStatus.UnderMaintenance)
                throw new InvalidOperationException("Equipment must be 'Under Maintenance' to complete maintenance.");

            activeRecord.Status = MaintenanceStatus.Completed;
            activeRecord.MaintenanceEndDate = DateTime.UtcNow;
            
            if (!string.IsNullOrWhiteSpace(notes))
            {
                activeRecord.Notes = string.IsNullOrWhiteSpace(activeRecord.Notes) 
                    ? notes 
                    : $"{activeRecord.Notes}\n\nCompletion Notes: {notes}";
            }

            equipment.MaintenanceStatus = MaintenanceStatus.Operational;
            equipment.LastMaintenanceDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return activeRecord;
        }

        public async Task<List<MaintenanceRecord>> GetMaintenanceHistoryAsync(int equipmentId)
        {
            return await _context.MaintenanceRecords
                .Where(mr => mr.EquipmentId == equipmentId)
                .Include(mr => mr.RequestedBy)
                .OrderByDescending(mr => mr.RequestDate)
                .ToListAsync();
        }

        public async Task<MaintenanceRecord?> GetActiveMaintenanceRecordAsync(int equipmentId)
        {
            return await _context.MaintenanceRecords
                .Where(mr => mr.EquipmentId == equipmentId && 
                            (mr.Status == MaintenanceStatus.MaintenanceRequested || 
                             mr.Status == MaintenanceStatus.UnderMaintenance))
                .Include(mr => mr.RequestedBy)
                .FirstOrDefaultAsync();
        }

        // Equipment status query methods
        public async Task<List<Equipment>> GetEquipmentByStatusAsync(MaintenanceStatus status)
        {
            return await _context.Equipments
                .AsNoTracking()
                .Include(e => e.Garden)
                .Include(e => e.MaintenanceRecords.Where(mr => mr.Status == status))
                .Where(e => e.MaintenanceStatus == status)
                .OrderBy(e => e.EquipmentName)
                .ToListAsync();
        }

        public async Task<List<Equipment>> GetEquipmentByGardenAndStatusAsync(int gardenId, MaintenanceStatus status)
        {
            return await _context.Equipments
                .AsNoTracking()
                .Include(e => e.Garden)
                .Include(e => e.MaintenanceRecords.Where(mr => mr.Status == status))
                .Where(e => e.GardenId == gardenId && e.MaintenanceStatus == status)
                .OrderBy(e => e.EquipmentName)
                .ToListAsync();
        }

        public async Task<List<Equipment>> GetEquipmentByGardenAsync(int gardenId)
        {
            return await _context.Equipments
                .AsNoTracking()
                .Include(e => e.Garden)
                .Include(e => e.MaintenanceRecords)
                .Where(e => e.GardenId == gardenId)
                .OrderBy(e => e.EquipmentName)
                .ToListAsync();
        }
    }
}
