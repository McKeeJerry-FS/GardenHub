using GardenHub.Data;
using GardenHub.Models;
using GardenHub.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GardenHub.Services
{
    public class JournalEntriesService : IJournalEntriesService
    {
        private readonly ApplicationDbContext _context;

        public JournalEntriesService(ApplicationDbContext context) 
        {
            _context = context;
        }

        public async Task CreateJournalEntryAsync(JournalEntry journalEntry)
        {
            if (journalEntry != null)
            {
                _context.JournalEntries.Add(journalEntry);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> DeleteJournalEntryAsync(int id)
        {
            if (id > 0)
            {
                var journalEntry = await _context.JournalEntries.FirstOrDefaultAsync(je => je.EntryId == id);
                if (journalEntry != null)
                {
                    _context.JournalEntries.Remove(journalEntry);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            return false;
        }

        public async Task<List<JournalEntry>> GetAllJournalEntriesAsync()
        {
            return await _context.JournalEntries
                .AsNoTracking()  // Performance boost for read-only queries
                .Include(j => j.Garden)
                .Include(j => j.User)
                .OrderByDescending(j => j.EntryDate)
                .ToListAsync();
        }

        public async Task<JournalEntry?> GetJournalEntryByIdAsync(int id)
        {
            if (id > 0)
            {
                return await _context.JournalEntries
                    .AsNoTracking()  // For Details view (read-only)
                    .Include(j => j.Garden)
                    .Include(j => j.User)
                    .FirstOrDefaultAsync(je => je.EntryId == id);
            }
            return null;
        }

        public async Task<List<JournalEntry>> GetJournalEntriesByGardenIdAsync(int gardenId)
        {
            return await _context.JournalEntries
                .AsNoTracking()
                .Where(je => je.GardenId == gardenId)
                .Include(j => j.Garden)
                .Include(j => j.User)
                .OrderByDescending(j => j.EntryDate)
                .ToListAsync();
        }

        public async Task UpdateJournalEntryAsync(int id, JournalEntry journalEntry)
        {
            if (id > 0)
            {
                var existingEntry = await _context.JournalEntries.FirstOrDefaultAsync(je => je.EntryId == id);
                if (existingEntry != null)
                {
                    _context.Entry(existingEntry).CurrentValues.SetValues(journalEntry);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // Handle the case where the journal entry does not exist
                    throw new KeyNotFoundException($"JournalEntry with ID {id} not found.");
                }
            }
            else
            {
                throw new ArgumentException("Invalid journal entry ID.");
            }
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
