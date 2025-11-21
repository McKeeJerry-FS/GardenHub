using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GardenHub.Models;
using GardenHub.Models.Enums;
using GardenHub.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace GardenHub.Controllers
{
    [Authorize]
    public class JournalEntriesController : Controller
    {
        private readonly IJournalEntriesService _journalEntriesService;
        private readonly IImageService _imageService;
        

        public JournalEntriesController(IJournalEntriesService journalEntriesService, IImageService imageService)
        {
            _journalEntriesService = journalEntriesService;
            _imageService = imageService;
        }

        // GET: JournalEntries
        public async Task<IActionResult> Index()
        {
            var journalEntries = await _journalEntriesService.GetAllJournalEntriesAsync();
            
            // Convert image data for display
            foreach (var entry in journalEntries)
            {
                entry.ImageFile = null;
                ViewData[$"EntryImage_{entry.EntryId}"] = _imageService.ConvertByteArrayToFile(
                    entry.ImageData, 
                    entry.ImageType, 
                    DefaultImage.GardenImage);
            }
            
            return View(journalEntries);
        }

        // GET: JournalEntries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var journalEntry = await _journalEntriesService.GetJournalEntryByIdAsync(id.Value);
            if (journalEntry == null)
            {
                return NotFound();
            }

            // Convert image data for display
            ViewData["EntryImage"] = _imageService.ConvertByteArrayToFile(
                journalEntry.ImageData, 
                journalEntry.ImageType, 
                DefaultImage.GardenImage);

            return View(journalEntry);
        }

        // GET: JournalEntries/Create
        public async Task<IActionResult> Create()
        {
            ViewData["GardenId"] = new SelectList(await _journalEntriesService.GetAllGardensAsync(), "GardenId", "GardenDescription");
            ViewData["UserId"] = new SelectList(await _journalEntriesService.GetAllUsersAsync(), "Id", "Id");
            return View();
        }

        // POST: JournalEntries/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EntryId,GardenId,EntryDate,Content,UserId,ImageFile")] JournalEntry journalEntry)
        {
            // Ensure DateTime is in UTC for PostgreSQL
            if (journalEntry.EntryDate.Kind == DateTimeKind.Unspecified)
            {
                journalEntry.EntryDate = DateTime.SpecifyKind(journalEntry.EntryDate, DateTimeKind.Utc);
            }
            else if (journalEntry.EntryDate.Kind == DateTimeKind.Local)
            {
                journalEntry.EntryDate = journalEntry.EntryDate.ToUniversalTime();
            }

            // Handle image upload
            if (journalEntry.ImageFile != null)
            {
                journalEntry.ImageData = await _imageService.ConvertFileToByteArrayAsync(journalEntry.ImageFile);
                journalEntry.ImageType = journalEntry.ImageFile.ContentType;
            }

            ModelState.Remove("Garden");
            ModelState.Remove("User");
            ModelState.Remove("ImageFile");

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the current logged -in user's ID
                    journalEntry.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    await _journalEntriesService.CreateJournalEntryAsync(journalEntry);
                    TempData["SuccessMessage"] = "Journal entry has been created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"An error occurred while creating the journal entry: {ex.Message}";
                }
            }
            ViewData["GardenId"] = new SelectList(await _journalEntriesService.GetAllGardensAsync(), "GardenId", "GardenDescription", journalEntry.GardenId);
            ViewData["UserId"] = new SelectList(await _journalEntriesService.GetAllUsersAsync(), "Id", "Id", journalEntry.UserId);
            return View(journalEntry);
        }

        // GET: JournalEntries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var journalEntry = await _journalEntriesService.GetJournalEntryByIdAsync(id.Value);
            if (journalEntry == null)
            {
                return NotFound();
            }
            
            // Convert image data for display
            ViewData["CurrentImage"] = _imageService.ConvertByteArrayToFile(
                journalEntry.ImageData, 
                journalEntry.ImageType, 
                DefaultImage.GardenImage);
            
            ViewData["GardenId"] = new SelectList(await _journalEntriesService.GetAllGardensAsync(), "GardenId", "GardenDescription", journalEntry.GardenId);
            ViewData["UserId"] = new SelectList(await _journalEntriesService.GetAllUsersAsync(), "Id", "Id", journalEntry.UserId);
            return View(journalEntry);
        }

        // POST: JournalEntries/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EntryId,GardenId,EntryDate,Content,UserId,ImageFile")] JournalEntry journalEntry)
        {
            if (id != journalEntry.EntryId)
            {
                return NotFound();
            }
            
            // Ensure DateTime is in UTC for PostgreSQL
            if (journalEntry.EntryDate.Kind == DateTimeKind.Unspecified)
            {
                journalEntry.EntryDate = DateTime.SpecifyKind(journalEntry.EntryDate, DateTimeKind.Utc);
            }
            else if (journalEntry.EntryDate.Kind == DateTimeKind.Local)
            {
                journalEntry.EntryDate = journalEntry.EntryDate.ToUniversalTime();
            }

            // Handle image upload
            if (journalEntry.ImageFile != null)
            {
                journalEntry.ImageData = await _imageService.ConvertFileToByteArrayAsync(journalEntry.ImageFile);
                journalEntry.ImageType = journalEntry.ImageFile.ContentType;
            }
            else
            {
                // Keep existing image if no new one uploaded
                var existingEntry = await _journalEntriesService.GetJournalEntryByIdAsync(id);
                if (existingEntry != null)
                {
                    journalEntry.ImageData = existingEntry.ImageData;
                    journalEntry.ImageType = existingEntry.ImageType;
                }
            }

            ModelState.Remove("Garden");
            ModelState.Remove("User");
            ModelState.Remove("ImageFile");

            if (ModelState.IsValid)
            {
                try
                {
                    await _journalEntriesService.UpdateJournalEntryAsync(id, journalEntry);
                    TempData["SuccessMessage"] = "Journal entry has been updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (KeyNotFoundException)
                {
                    TempData["ErrorMessage"] = "The journal entry you are trying to update no longer exists.";
                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException ex)
                {
                    TempData["ErrorMessage"] = $"Invalid data: {ex.Message}";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"An error occurred while updating the journal entry: {ex.Message}";
                }
            }
            ViewData["GardenId"] = new SelectList(await _journalEntriesService.GetAllGardensAsync(), "GardenId", "GardenDescription", journalEntry.GardenId);
            ViewData["UserId"] = new SelectList(await _journalEntriesService.GetAllUsersAsync(), "Id", "Id", journalEntry.UserId);
            return View(journalEntry);
        }

        // GET: JournalEntries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var journalEntry = await _journalEntriesService.GetJournalEntryByIdAsync(id.Value);
            if (journalEntry == null)
            {
                return NotFound();
            }

            return View(journalEntry);
        }

        // POST: JournalEntries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _journalEntriesService.DeleteJournalEntryAsync(id);
                if (!result)
                {
                    TempData["ErrorMessage"] = "The journal entry you are trying to delete no longer exists.";
                }
                else
                {
                    TempData["SuccessMessage"] = "Journal entry has been deleted successfully!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the journal entry: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: JournalEntries/Dashboard
        public async Task<IActionResult> Dashboard(int? gardenId, int days = 30)
        {
            IEnumerable<JournalEntry> entries;
            
            // Filter by garden if specified
            if (gardenId.HasValue)
            {
                entries = await _journalEntriesService.GetJournalEntriesByGardenIdAsync(gardenId.Value);
            }
            else
            {
                entries = await _journalEntriesService.GetAllJournalEntriesAsync();
            }

            // Filter by date range
            var startDate = DateTime.UtcNow.AddDays(-days);
            entries = entries.Where(e => e.EntryDate >= startDate)
                            .OrderByDescending(e => e.EntryDate)
                            .ToList();

            // Convert image data for display
            foreach (var entry in entries)
            {
                entry.ImageFile = null;
                ViewData[$"EntryImage_{entry.EntryId}"] = _imageService.ConvertByteArrayToFile(
                    entry.ImageData, 
                    entry.ImageType, 
                    DefaultImage.GardenImage);
            }

            // Get gardens for filter dropdown
            var gardens = await _journalEntriesService.GetAllGardensAsync();
            ViewData["GardenId"] = new SelectList(gardens, "GardenId", "GardenDescription", gardenId);
            ViewData["Days"] = days;
            
            return View(entries);
        }
    }
}
