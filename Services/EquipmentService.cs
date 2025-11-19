using GardenHub.Data;
using GardenHub.Models;
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
    }
}
