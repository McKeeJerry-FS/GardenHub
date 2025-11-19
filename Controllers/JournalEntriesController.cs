using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GardenHub.Data;
using GardenHub.Models;
using GardenHub.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace GardenHub.Controllers
{
    [Authorize]
    public class JournalEntriesController : Controller
    {
        private readonly IJournalEntriesService _journalEntriesService;
        

        public JournalEntriesController(IJournalEntriesService journalEntriesService)
        {
            _journalEntriesService = journalEntriesService;
        }

        // GET: JournalEntries
        public async Task<IActionResult> Index()
        {
            var journalEntries = await _journalEntriesService.GetAllJournalEntriesAsync();
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

            return View(journalEntry);
        }

        // GET: JournalEntries/Create
        public async Task<IActionResult> Create()  // Remove 'Async' suffix
        {
            ViewData["GardenId"] = new SelectList(await _journalEntriesService.GetAllGardensAsync(), "GardenId", "GardenDescription");
            ViewData["UserId"] = new SelectList(await _journalEntriesService.GetAllUsersAsync(), "Id", "Id");
            return View();
        }

        // POST: JournalEntries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EntryId,GardenId,EntryDate,Content,UserId")] JournalEntry journalEntry)
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

            ModelState.Remove("Garden"); // Remove the navigation property from validation
            ModelState.Remove("User");   // Also remove User navigation property

            if (ModelState.IsValid)
            {
                await _journalEntriesService.CreateJournalEntryAsync(journalEntry);
                return RedirectToAction(nameof(Index));
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
            ViewData["GardenId"] = new SelectList(await _journalEntriesService.GetAllGardensAsync(), "GardenId", "GardenDescription", journalEntry.GardenId);
            ViewData["UserId"] = new SelectList(await _journalEntriesService.GetAllUsersAsync(), "Id", "Id", journalEntry.UserId);
            return View(journalEntry);
        }

        // POST: JournalEntries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EntryId,GardenId,EntryDate,Content,UserId")] JournalEntry journalEntry)
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

            ModelState.Remove("Garden"); // Remove the navigation property from validation
            ModelState.Remove("User");   // Also remove User navigation property

            if (ModelState.IsValid)
            {
                try
                {
                    await _journalEntriesService.UpdateJournalEntryAsync(id, journalEntry);
                }
                catch (KeyNotFoundException)
                {
                    return NotFound();
                }
                catch (ArgumentException)
                {
                    return BadRequest();
                }
                return RedirectToAction(nameof(Index));
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
            var result = await _journalEntriesService.DeleteJournalEntryAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
