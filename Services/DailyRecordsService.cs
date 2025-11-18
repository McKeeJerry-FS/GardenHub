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

        public Task<DailyRecord> CreateDailyRecordAsync(DailyRecord dailyRecord)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteDailyRecordAsync(int recordId)
        {
            throw new NotImplementedException();
        }

        public async Task<DailyRecord?> GetDailyRecordByIdAsync(int? recordId)
        {
            if (recordId != null)
            {
                var record = await _context.DailyRecords.FindAsync(recordId);
                return record;
            }
            return null;
        }

        public Task<List<DailyRecord>> GetDailyRecordsByConditionAsync(PlantCondition condition)
        {
            throw new NotImplementedException();
        }

        public Task<List<DailyRecord>> GetDailyRecordsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var records = _context.DailyRecords
                                  .Where(r => r.CreatedDate >= startDate && r.CreatedDate <= endDate)
                                  .ToListAsync();
            return records;
        }

        public async Task<List<DailyRecord>> GetDailyRecordsByGardenIdAsync(int gardenId)
        {
            var records = await _context.DailyRecords
                                        .Where(r => r.GardenId == gardenId)
                                        .ToListAsync();
            return records;
        }

        public async Task<List<DailyRecord>> GetDailyRecordsByUserIdAsync(string userId)
        {
            var records = await _context.DailyRecords
                                        .Where(r => r.UserId == userId)
                                        .ToListAsync();
            return records;
        }

        public Task<DailyRecord?> UpdateDailyRecordAsync(int recordId, DailyRecord updatedRecord)
        {
            throw new NotImplementedException();
        }
    }
}
