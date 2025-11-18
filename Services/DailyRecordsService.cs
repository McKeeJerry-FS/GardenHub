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
            if (dailyRecord is not null) 
            { 
                _context.DailyRecords.Add(dailyRecord);
                _context.SaveChangesAsync();
                return Task.FromResult(dailyRecord);
            } else {                 
                throw new ArgumentNullException(nameof(dailyRecord), "DailyRecord cannot be null"); 
            }

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

        public async Task<DailyRecord?> GetDailyRecordByIdAsync(int? recordId)
        {
            if (recordId != null)
            {
                var record = await _context.DailyRecords.FindAsync(recordId);
                return record;
            }
            return null;
        }

        public async Task<DailyRecord?> GetDailyRecordByIdAsync(int recordId)
        {
            if (recordId >= 0)
            {
                var record = await _context.DailyRecords.FindAsync(recordId);
                return record;
            } else
            {
                return null;
            }
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
            if (recordId >= 0 && updatedRecord is not null)
            {
                var existingRecord = _context.DailyRecords.Find(recordId);
                if (existingRecord != null)
                {
                    _context.Entry(existingRecord).CurrentValues.SetValues(updatedRecord);
                    _context.SaveChangesAsync();
                    return Task.FromResult<DailyRecord?>(existingRecord);
                }
                return Task.FromResult<DailyRecord?>(null);
            }
            else
            {
                throw new ArgumentNullException(nameof(updatedRecord), "Updated record cannot be null");
            }
        }
    }
}
