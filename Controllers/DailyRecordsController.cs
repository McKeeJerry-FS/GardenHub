using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GardenHub.Data;
using GardenHub.Models;

namespace GardenHub.Controllers
{
    public class DailyRecordsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DailyRecordsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DailyRecords
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.DailyRecords.Include(d => d.Garden).Include(d => d.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: DailyRecords/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dailyRecord = await _context.DailyRecords
                .Include(d => d.Garden)
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.RecordId == id);
            if (dailyRecord == null)
            {
                return NotFound();
            }

            return View(dailyRecord);
        }

        // GET: DailyRecords/Create
        public IActionResult Create()
        {
            ViewData["GardenId"] = new SelectList(_context.Gardens, "GardenId", "GardenDescription");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: DailyRecords/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RecordId,GardenId,CreatedDate,InsideTemperature,OutsideTemperature,InsideHumidity,OutsideHumidity,InsideVPD,OutsideVPD,LightingOn,LightingOff,LightingIntensity,WaterAmount,NutrientAmount,Notes,UserId")] DailyRecord dailyRecord)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dailyRecord);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["GardenId"] = new SelectList(_context.Gardens, "GardenId", "GardenDescription", dailyRecord.GardenId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", dailyRecord.UserId);
            return View(dailyRecord);
        }

        // GET: DailyRecords/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dailyRecord = await _context.DailyRecords.FindAsync(id);
            if (dailyRecord == null)
            {
                return NotFound();
            }
            ViewData["GardenId"] = new SelectList(_context.Gardens, "GardenId", "GardenDescription", dailyRecord.GardenId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", dailyRecord.UserId);
            return View(dailyRecord);
        }

        // POST: DailyRecords/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RecordId,GardenId,CreatedDate,InsideTemperature,OutsideTemperature,InsideHumidity,OutsideHumidity,InsideVPD,OutsideVPD,LightingOn,LightingOff,LightingIntensity,WaterAmount,NutrientAmount,Notes,UserId")] DailyRecord dailyRecord)
        {
            if (id != dailyRecord.RecordId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dailyRecord);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DailyRecordExists(dailyRecord.RecordId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["GardenId"] = new SelectList(_context.Gardens, "GardenId", "GardenDescription", dailyRecord.GardenId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", dailyRecord.UserId);
            return View(dailyRecord);
        }

        // GET: DailyRecords/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dailyRecord = await _context.DailyRecords
                .Include(d => d.Garden)
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.RecordId == id);
            if (dailyRecord == null)
            {
                return NotFound();
            }

            return View(dailyRecord);
        }

        // POST: DailyRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dailyRecord = await _context.DailyRecords.FindAsync(id);
            if (dailyRecord != null)
            {
                _context.DailyRecords.Remove(dailyRecord);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DailyRecordExists(int id)
        {
            return _context.DailyRecords.Any(e => e.RecordId == id);
        }
    }
}
