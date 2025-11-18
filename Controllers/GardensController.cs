using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GardenHub.Data;
using GardenHub.Models;
using Microsoft.AspNetCore.Authorization;
using GardenHub.Services.Interfaces;

namespace GardenHub.Controllers
{
    [Authorize]
    public class GardensController : Controller
    {
        private readonly IGardenService _gardenService;

        public GardensController(IGardenService gardenService)
        {
            _gardenService = gardenService;
        }

        // GET: Gardens
        public async Task<IActionResult> Index()
        {
            var gardens = await _gardenService.GetAllGardens();
            return View(gardens);
        }

        // GET: Gardens/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var garden = await _gardenService.GetGardenWithDetailsById(id.Value);
            if (garden == null)
            {
                return NotFound();
            }

            return View(garden);
        }

        // GET: Gardens/Create
        public async Task<IActionResult> Create()
        {
            ViewData["UserId"] = new SelectList(await _gardenService.GetAllUsers(), "Id", "Id");
            return View();
        }

        // POST: Gardens/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GardenId,GardenName,GardenDescription,GardenLocation,Type,GardenGrowMethod,StartDate,EndDate,UserId")] Garden garden)
        {
            if (ModelState.IsValid)
            {
                await _gardenService.CreateGarden(garden);
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(await _gardenService.GetAllUsers(), "Id", "Id", garden.UserId);
            return View(garden);
        }

        // GET: Gardens/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var garden = await _gardenService.GetGardenById(id.Value);
            if (garden == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(await _gardenService.GetAllUsers(), "Id", "Id", garden.UserId);
            return View(garden);
        }

        // POST: Gardens/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GardenId,GardenName,GardenDescription,GardenLocation,Type,GardenGrowMethod,StartDate,EndDate,UserId")] Garden garden)
        {
            if (id != garden.GardenId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _gardenService.UpdateGarden(garden, id);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await GardenExists(garden.GardenId))
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
            ViewData["UserId"] = new SelectList(await _gardenService.GetAllUsers(), "Id", "Id", garden.UserId);
            return View(garden);
        }

        // GET: Gardens/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var garden = await _gardenService.GetGardenById(id.Value);
            if (garden == null)
            {
                return NotFound();
            }

            return View(garden);
        }

        // POST: Gardens/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _gardenService.DeleteGarden(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> GardenExists(int id)
        {
            return await _gardenService.GardenExists(id);
        }
    }
}
