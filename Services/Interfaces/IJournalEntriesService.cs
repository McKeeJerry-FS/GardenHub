using GardenHub.Models;

namespace GardenHub.Services.Interfaces
{
    public interface IJournalEntriesService
    {   
        Task<List<JournalEntry>> GetAllJournalEntriesAsync();
        Task<JournalEntry?> GetJournalEntryByIdAsync(int id);
        Task CreateJournalEntryAsync(JournalEntry journalEntry);
        Task UpdateJournalEntryAsync(int id, JournalEntry journalEntry);
        Task<bool> DeleteJournalEntryAsync(int id);
        Task<List<JournalEntry>> GetJournalEntriesByGardenIdAsync(int gardenId);

        // Helper methods for controller select lists
        Task<List<Garden>> GetAllGardensAsync();
        Task<List<AppUser>> GetAllUsersAsync();
    }
}
