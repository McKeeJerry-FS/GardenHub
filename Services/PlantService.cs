using GardenHub.Data;
using GardenHub.Models;
using GardenHub.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GardenHub.Services
{
    public class PlantService : IPlantService
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService; // Add this

        public PlantService(ApplicationDbContext context, IImageService imageService)
        {
            _context = context;
            _imageService = imageService; // Initialize in constructor
        }

        public async Task CreatePlantAsync(Plant plant)
        {
            if (plant != null)
            {
                // In Create/Edit actions:
                if (plant.ImageFile != null)
                {
                    plant.ImageData = await _imageService.ConvertFileToByteArrayAsync(plant.ImageFile);
                    plant.ImageType = plant.ImageFile.ContentType;
                }

                _context.Plants.Add(plant);
                await _context.SaveChangesAsync();
                return;
            }
            throw new ArgumentNullException(nameof(plant), "Plant cannot be null.");
        }

        public async Task<bool> DeletePlantAsync(int id)
        {
            if (id <= 0)
                return false; // Or throw ArgumentException
            
            var plant = await _context.Plants.FirstOrDefaultAsync(p => p.PlantId == id);
            if (plant == null)
                return false;
            
            _context.Plants.Remove(plant);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Plant>> GetAllPlantsAsync()
        {
            var plants = await _context.Plants
                .AsNoTracking()  // Performance boost for read-only queries
                .Include(p => p.Garden)
                .Include(p => p.User)
                .OrderBy(p => p.PlantName)
                .ToListAsync();
            return plants;
        }


        public async Task<Plant?> GetPlantByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Invalid plant ID.", nameof(id));
            }
            return await _context.Plants
                .AsNoTracking()
                .Include(p => p.Garden)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PlantId == id);

        }

        public async Task UpdatePlantAsync(int id, Plant plant)
        {
            if (id != plant.PlantId)
                throw new ArgumentException("Plant ID mismatch.", nameof(id));
            
            var existingPlant = await _context.Plants.FirstOrDefaultAsync(p => p.PlantId == id);
            if (existingPlant == null)
                throw new KeyNotFoundException($"Plant with ID {id} not found.");
            
            // Store existing image data before update
            var preserveImageData = existingPlant.ImageData;
            var preserveImageType = existingPlant.ImageType;
            
            // Update all properties from the plant parameter
            _context.Entry(existingPlant).CurrentValues.SetValues(plant);
            
            // Handle image update logic
            if (plant.ImageFile != null)
            {
                // New image provided - update it
                existingPlant.ImageData = await _imageService.ConvertFileToByteArrayAsync(plant.ImageFile);
                existingPlant.ImageType = plant.ImageFile.ContentType;
            }
            else
            {
                // No new image - preserve existing image
                existingPlant.ImageData = preserveImageData;
                existingPlant.ImageType = preserveImageType;
            }
            
            await _context.SaveChangesAsync();
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

        public async Task<List<Plant>> GetPlantsByGardenIdAsync(int gardenId)
        {
            if (gardenId <= 0)
                throw new ArgumentException("Invalid garden ID.", nameof(gardenId));

            return await _context.Plants
                .AsNoTracking()
                .Where(p => p.GardenId == gardenId)
                .Include(p => p.Garden)
                .Include(p => p.User)
                .OrderBy(p => p.PlantName)
                .ToListAsync();
        }

        public async Task<List<Plant>> GetPlantsByUserIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("Invalid user ID.", nameof(userId));

            return await _context.Plants
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .Include(p => p.Garden)
                .Include(p => p.User)
                .OrderBy(p => p.PlantName)
                .ToListAsync();
        }

        public async Task<List<Plant>> SearchPlantsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllPlantsAsync(); // Return all plants if the search term is empty
            
            return await _context.Plants
                .AsNoTracking()
                .Where(p => p.PlantName.Contains(searchTerm) || p.PlantDescription.Contains(searchTerm))
                .Include(p => p.Garden)
                .Include(p => p.User)
                .OrderBy(p => p.PlantName)
                .ToListAsync();
        }
    }
}
