using GardenHub.Services.Interfaces;
using GardenHub.Data;
using GardenHub.Models;

namespace GardenHub.Services
{
    public class DailyRecordsService : IDailyRecordService
    {
        private readonly ApplicationDbContext _context;
        public DailyRecordsService(ApplicationDbContext context)
        {
            _context = context;
        }
    }
}
