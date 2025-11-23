using GardenHub.Data;
using GardenHub.Models;
using GardenHub.Models.Enums;
using GardenHub.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GardenHub.Services
{
    public class PlantCareService : IPlantCareService
    {
        private readonly ApplicationDbContext _context;

        public PlantCareService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateCareActivityAsync(PlantCareActivity activity)
        {
            if (activity is null)
            {
                throw new ArgumentNullException(nameof(activity), "Care activity cannot be null");
            }

            _context.PlantCareActivities.Add(activity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteCareActivityAsync(int activityId)
        {
            var activity = await _context.PlantCareActivities.FindAsync(activityId);
            if (activity != null)
            {
                _context.PlantCareActivities.Remove(activity);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<List<PlantCareActivity>> GetAllCareActivitiesAsync()
        {
            return await _context.PlantCareActivities
                .Include(a => a.Plant)
                .Include(a => a.User)
                .OrderByDescending(a => a.ActivityDate)
                .ToListAsync();
        }

        public async Task<PlantCareActivity?> GetCareActivityByIdAsync(int activityId)
        {
            return await _context.PlantCareActivities
                .Include(a => a.Plant)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.ActivityId == activityId);
        }

        public async Task<List<PlantCareActivity>> GetCareActivitiesByPlantIdAsync(int plantId)
        {
            return await _context.PlantCareActivities
                .Where(a => a.PlantId == plantId)
                .Include(a => a.Plant)
                .Include(a => a.User)
                .OrderByDescending(a => a.ActivityDate)
                .ToListAsync();
        }

        public async Task<List<PlantCareActivity>> GetCareActivitiesByUserIdAsync(string userId)
        {
            return await _context.PlantCareActivities
                .Where(a => a.UserId == userId)
                .Include(a => a.Plant)
                .Include(a => a.User)
                .OrderByDescending(a => a.ActivityDate)
                .ToListAsync();
        }

        public async Task<List<PlantCareActivity>> GetCareActivitiesByDateRangeAsync(int plantId, DateTime startDate, DateTime endDate)
        {
            return await _context.PlantCareActivities
                .Where(a => a.PlantId == plantId && 
                           a.ActivityDate >= startDate && 
                           a.ActivityDate <= endDate)
                .Include(a => a.Plant)
                .Include(a => a.User)
                .OrderByDescending(a => a.ActivityDate)
                .ToListAsync();
        }

        public async Task<List<PlantCareActivity>> GetCareActivitiesByTypeAsync(int plantId, PlantCareActivityType activityType)
        {
            return await _context.PlantCareActivities
                .Where(a => a.PlantId == plantId && a.ActivityType == activityType)
                .Include(a => a.Plant)
                .Include(a => a.User)
                .OrderByDescending(a => a.ActivityDate)
                .ToListAsync();
        }

        public async Task<PlantCareActivity?> GetMostRecentActivityAsync(int plantId)
        {
            return await _context.PlantCareActivities
                .Where(a => a.PlantId == plantId)
                .Include(a => a.Plant)
                .Include(a => a.User)
                .OrderByDescending(a => a.ActivityDate)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetTotalActivitiesCountAsync(int plantId)
        {
            return await _context.PlantCareActivities
                .Where(a => a.PlantId == plantId)
                .CountAsync();
        }

        public async Task<double> GetAverageActivityDurationAsync(int plantId)
        {
            var activities = await _context.PlantCareActivities
                .Where(a => a.PlantId == plantId && a.ActivityDuration.HasValue)
                .ToListAsync();

            if (!activities.Any())
                return 0;

            return activities.Average(a => a.ActivityDuration!.Value);
        }

        public async Task<Plant?> GetPlantWithCareActivitiesAsync(int plantId)
        {
            return await _context.Plants
                .Include(p => p.User)
                .Include(p => p.Garden)
                .Include(p => p.CareActivities)
                    .ThenInclude(ca => ca.User)
                .FirstOrDefaultAsync(p => p.PlantId == plantId);
        }

        public async Task UpdateCareActivityAsync(int activityId, PlantCareActivity activity)
        {
            var existingActivity = await _context.PlantCareActivities.FindAsync(activityId);
            if (existingActivity != null)
            {
                _context.Entry(existingActivity).CurrentValues.SetValues(activity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Plant>> GetAllPlantsAsync()
        {
            return await _context.Plants
                .Include(p => p.Garden)
                .ToListAsync();
        }

        public async Task<List<AppUser>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }
    }
}