using GardenHub.Services.Interfaces;
using GardenHub.Data;
using GardenHub.Models;
using GardenHub.Models.Enums;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace GardenHub.Services
{
    public class DailyRecordsService : IDailyRecordService
    {
        private readonly ApplicationDbContext _context;
        public DailyRecordsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DailyRecord> CreateDailyRecordAsync(DailyRecord dailyRecord)
        {
            if (dailyRecord is null)
            {
                throw new ArgumentNullException(nameof(dailyRecord), "DailyRecord cannot be null");
            }

            _context.DailyRecords.Add(dailyRecord);
            await _context.SaveChangesAsync();
            return dailyRecord;
        }

        public async Task<bool> DeleteDailyRecordAsync(int recordId)
        {
            if (recordId < 0)
            {
                return false;
            }

            var record = await _context.DailyRecords.FindAsync(recordId);
            if (record != null)
            {
                _context.DailyRecords.Remove(record);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<DailyRecord?> GetDailyRecordByIdAsync(int recordId)
        {
            if (recordId < 0) return null;
            return await _context.DailyRecords
                                 .Include(r => r.Garden)
                                 .Include(r => r.User)
                                 .FirstOrDefaultAsync(r => r.RecordId == recordId);
        }

        public async Task<List<DailyRecord>> GetDailyRecordsByConditionAsync(PlantCondition condition)
        {
            // Simple heuristics to map a PlantCondition to a query over DailyRecord fields.
            // These heuristics can be refined later; they intentionally avoid changing the domain model.
            IQueryable<DailyRecord> query = _context.DailyRecords
                                                     .Include(r => r.Garden)
                                                     .Include(r => r.User);

            switch (condition)
            {
                case PlantCondition.Healthy:
                    // Healthy: moderate temperature, humidity and mid-range watering
                    query = query.Where(r => r.InsideTemperature >= 18 && r.InsideTemperature <= 26
                                             && r.InsideHumidity >= 40 && r.InsideHumidity <= 70
                                             && r.WaterAmount >= 30 && r.WaterAmount <= 70);
                    break;
                case PlantCondition.Wilting:
                    // Wilting: high temperature or low humidity
                    query = query.Where(r => r.InsideTemperature > 30 || r.InsideHumidity < 30);
                    break;
                case PlantCondition.Overwatered:
                    // Overwatered: very high water amount
                    query = query.Where(r => r.WaterAmount >= 80);
                    break;
                case PlantCondition.Underwatered:
                    // Underwatered: very low water amount
                    query = query.Where(r => r.WaterAmount <= 20);
                    break;
                case PlantCondition.PestInfested:
                    // PestInfested: notes mentioning pests (case-insensitive)
                    query = query.Where(r => r.Notes != null && EF.Functions.ILike(r.Notes, "%pest%"));
                    break;
                case PlantCondition.Diseased:
                    // Diseased: notes mentioning disease or low nutrient amount
                    query = query.Where(r => (r.Notes != null && EF.Functions.ILike(r.Notes, "%disease%"))
                                             || r.NutrientAmount <= 10);
                    break;
                default:
                    break;
            }

            return await query.ToListAsync();
        }

        public async Task<List<DailyRecord>> GetDailyRecordsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var records = await _context.DailyRecords
                                  .Where(r => r.CreatedDate >= startDate && r.CreatedDate <= endDate)
                                  .Include(r => r.Garden)
                                  .Include(r => r.User)
                                  .ToListAsync();
            return records;
        }

        public async Task<List<DailyRecord>> GetDailyRecordsByGardenIdAsync(int gardenId)
        {
            var records = await _context.DailyRecords
                                        .Where(r => r.GardenId == gardenId)
                                        .Include(r => r.Garden)
                                        .Include(r => r.User)
                                        .ToListAsync();
            return records;
        }

        public async Task<List<DailyRecord>> GetDailyRecordsByUserIdAsync(string userId)
        {
            var records = await _context.DailyRecords
                                        .Where(r => r.UserId == userId)
                                        .Include(r => r.Garden)
                                        .Include(r => r.User)
                                        .ToListAsync();
            return records;
        }

        public async Task<List<DailyRecord>> GetAllDailyRecordsAsync()
        {
            return await _context.DailyRecords
                                 .Include(r => r.Garden)
                                 .Include(r => r.User)
                                 .ToListAsync();
        }

        public async Task<DailyRecord?> UpdateDailyRecordAsync(int recordId, DailyRecord updatedRecord)
        {
            if (recordId < 0 || updatedRecord is null)
            {
                throw new ArgumentNullException(nameof(updatedRecord), "Updated record cannot be null");
            }

            var existingRecord = await _context.DailyRecords.FindAsync(recordId);
            if (existingRecord != null)
            {
                _context.Entry(existingRecord).CurrentValues.SetValues(updatedRecord);
                await _context.SaveChangesAsync();
                return existingRecord;
            }
            return null;
        }

        // Helper methods for controller select lists
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
