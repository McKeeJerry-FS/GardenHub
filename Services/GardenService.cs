using GardenHub.Data;
using GardenHub.Models;
using GardenHub.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace GardenHub.Services
{
    public class GardenService : IGardenService
    {
        private readonly ApplicationDbContext _context;

        public GardenService(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task CreateGarden(Garden garden)
        {
            garden.StartDate = DateTime.SpecifyKind(garden.StartDate, DateTimeKind.Utc);
            garden.EndDate = DateTime.SpecifyKind(garden.EndDate, DateTimeKind.Utc);
            _context.Add(garden);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteGarden(int id)
        {
            var garden = await _context.Gardens.FindAsync(id);
            if (garden != null)
            {
                _context.Gardens.Remove(garden);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<List<Garden?>> GetAllGardens()
        {
            var gardens = await _context.Gardens.Include(g => g.User).ToListAsync();
            return gardens.Cast<Garden?>().ToList();
        }

        public Task<Garden?> GetGardenById(int id)
        {
            var garden = _context.Gardens.Include(g => g.User).FirstOrDefaultAsync(g => g.GardenId == id);  
            return garden;
        }

        public async Task UpdateGarden(Garden garden, int id)
        {
            var existingGarden = await _context.Gardens.FindAsync(id);
            if (existingGarden != null)
            {
                // Update properties of the tracked entity
                existingGarden.GardenName = garden.GardenName;
                existingGarden.GardenDescription = garden.GardenDescription;
                existingGarden.GardenLocation = garden.GardenLocation;
                existingGarden.Type = garden.Type;
                existingGarden.GardenGrowMethod = garden.GardenGrowMethod;
                existingGarden.StartDate = DateTime.SpecifyKind(garden.StartDate, DateTimeKind.Utc);
                existingGarden.EndDate = DateTime.SpecifyKind(garden.EndDate, DateTimeKind.Utc);
                existingGarden.UserId = garden.UserId;
                
                // No need to call Update() - EF will detect changes automatically
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<AppUser>> GetAllUsers()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<Garden?> GetGardenWithDetailsById(int id)
        {
            var garden = await _context.Gardens
                .Include(g => g.User)
                .Include(g => g.DailyRecords)
                .Include(g => g.JournalEntries)
                .Include(g => g.Equipments)
                .Include(g => g.Plants)
                .FirstOrDefaultAsync(g => g.GardenId == id);
            return garden;
        }

        public Task<bool> GardenExists(int id)
        {
            return _context.Gardens.AnyAsync(e => e.GardenId == id);
        }
    }
}
