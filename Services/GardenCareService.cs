using GardenHub.Data;
using GardenHub.Models;
using GardenHub.Models.Enums;
using GardenHub.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GardenHub.Services
{
    public class GardenCareService : IGardenCareService
    {
        private readonly ApplicationDbContext _context;

        public GardenCareService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateCareActivityAsync(GardenCareActivity activity)
        {
            if (activity is null)
            {
                throw new ArgumentNullException(nameof(activity), "Care activity cannot be null");
            }

            _context.GardenCareActivities.Add(activity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteCareActivityAsync(int activityId)
        {
            var activity = await _context.GardenCareActivities.FindAsync(activityId);
            if (activity != null)
            {
                _context.GardenCareActivities.Remove(activity);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<List<GardenCareActivity>> GetAllCareActivitiesAsync()
        {
            return await _context.GardenCareActivities
                .Include(a => a.Garden)
                .Include(a => a.User)
                .OrderByDescending(a => a.ActivityDate)
                .ToListAsync();
        }

        public async Task<GardenCareActivity?> GetCareActivityByIdAsync(int activityId)
        {
            return await _context.GardenCareActivities
                .Include(a => a.Garden)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.ActivityId == activityId);
        }

        public async Task<List<GardenCareActivity>> GetCareActivitiesByGardenIdAsync(int gardenId)
        {
            return await _context.GardenCareActivities
                .Where(a => a.GardenId == gardenId)
                .Include(a => a.Garden)
                .Include(a => a.User)
                .OrderByDescending(a => a.ActivityDate)
                .ToListAsync();
        }

        public async Task<List<GardenCareActivity>> GetCareActivitiesByUserIdAsync(string userId)
        {
            return await _context.GardenCareActivities
                .Where(a => a.UserId == userId)
                .Include(a => a.Garden)
                .Include(a => a.User)
                .OrderByDescending(a => a.ActivityDate)
                .ToListAsync();
        }

        public async Task<List<GardenCareActivity>> GetCareActivitiesByDateRangeAsync(int gardenId, DateTime startDate, DateTime endDate)
        {
            return await _context.GardenCareActivities
                .Where(a => a.GardenId == gardenId && 
                           a.ActivityDate >= startDate && 
                           a.ActivityDate <= endDate)
                .Include(a => a.Garden)
                .Include(a => a.User)
                .OrderByDescending(a => a.ActivityDate)
                .ToListAsync();
        }

        public async Task<List<GardenCareActivity>> GetCareActivitiesByTypeAsync(int gardenId, CareActivityType activityType)
        {
            return await _context.GardenCareActivities
                .Where(a => a.GardenId == gardenId && a.ActivityType == activityType)
                .Include(a => a.Garden)
                .Include(a => a.User)
                .OrderByDescending(a => a.ActivityDate)
                .ToListAsync();
        }

        public async Task<GardenCareActivity?> GetMostRecentActivityAsync(int gardenId)
        {
            return await _context.GardenCareActivities
                .Where(a => a.GardenId == gardenId)
                .Include(a => a.Garden)
                .Include(a => a.User)
                .OrderByDescending(a => a.ActivityDate)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetTotalActivitiesCountAsync(int gardenId)
        {
            return await _context.GardenCareActivities
                .Where(a => a.GardenId == gardenId)
                .CountAsync();
        }

        public async Task<double> GetAverageActivityDurationAsync(int gardenId)
        {
            var activities = await _context.GardenCareActivities
                .Where(a => a.GardenId == gardenId && a.ActivityDuration.HasValue)
                .ToListAsync();

            if (!activities.Any())
                return 0;

            return activities.Average(a => a.ActivityDuration!.Value);
        }

        public async Task<Garden?> GetGardenWithCareActivitiesAsync(int gardenId)
        {
            return await _context.Gardens
                .Include(g => g.User)
                .Include(g => g.CareActivities)
                    .ThenInclude(ca => ca.User)
                .FirstOrDefaultAsync(g => g.GardenId == gardenId);
        }

        public async Task UpdateCareActivityAsync(int activityId, GardenCareActivity activity)
        {
            var existingActivity = await _context.GardenCareActivities.FindAsync(activityId);
            if (existingActivity != null)
            {
                _context.Entry(existingActivity).CurrentValues.SetValues(activity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Garden>> GetAllGardensAsync()
        {
            return await _context.Gardens.ToListAsync();
        }

        public async Task<List<AppUser>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }
    }
}