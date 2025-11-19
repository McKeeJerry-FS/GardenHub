using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GardenHub.Models;
using GardenHub.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace GardenHub.Controllers
{
    [Authorize]
    public class PlantsController : Controller
    {
        private readonly IPlantService _plantService;

        public PlantsController(IPlantService plantService)
        {
            _plantService = plantService;
        }

        // GET: Plants
        public async Task<IActionResult> Index()
        {
            var plants = await _plantService.GetAllPlantsAsync();
            return View(plants);
        }

        // GET: Plants/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plant = await _plantService.GetPlantByIdAsync(id.Value);

            if (plant == null)
            {
                return NotFound();
            }

            return View(plant);
        }

        // GET: Plants/Create
        public async Task<IActionResult> Create()
        {
            var gardens = await _plantService.GetAllGardensAsync();
            var users = await _plantService.GetAllUsersAsync();
            
            ViewData["GardenId"] = new SelectList(gardens, "GardenId", "GardenDescription");
            ViewData["UserId"] = new SelectList(users, "Id", "Id");
            return View();
        }

        // POST: Plants/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PlantId,PlantName,PlantDescription,PlantType,LightingRequirement,GrowMethod,DatePlanted,PlantCondition,ImageFile,GardenId,UserId")] Plant plant)
        {
            // Ensure DateTime is in UTC for PostgreSQL
            if (plant.DatePlanted.Kind == DateTimeKind.Unspecified)
            {
                plant.DatePlanted = DateTime.SpecifyKind(plant.DatePlanted, DateTimeKind.Utc);
            }
            else if (plant.DatePlanted.Kind == DateTimeKind.Local)
            {
                plant.DatePlanted = plant.DatePlanted.ToUniversalTime();
            }

            ModelState.Remove("Garden");
            ModelState.Remove("User");
            ModelState.Remove("ImageFile");

            if (ModelState.IsValid)
            {
                await _plantService.CreatePlantAsync(plant);
                return RedirectToAction(nameof(Index));
            }
            
            var gardens = await _plantService.GetAllGardensAsync();
            var users = await _plantService.GetAllUsersAsync();
            
            ViewData["GardenId"] = new SelectList(gardens, "GardenId", "GardenDescription", plant.GardenId);
            ViewData["UserId"] = new SelectList(users, "Id", "Id", plant.UserId);
            return View(plant);
        }

        // GET: Plants/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plant = await _plantService.GetPlantByIdAsync(id.Value);
            if (plant == null)
            {
                return NotFound();
            }
            
            var gardens = await _plantService.GetAllGardensAsync();
            var users = await _plantService.GetAllUsersAsync();
            
            ViewData["GardenId"] = new SelectList(gardens, "GardenId", "GardenDescription", plant.GardenId);
            ViewData["UserId"] = new SelectList(users, "Id", "Id", plant.UserId);
            return View(plant);
        }

        // POST: Plants/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PlantId,PlantName,PlantDescription,PlantType,LightingRequirement,GrowMethod,DatePlanted,PlantCondition,ImageFile,GardenId,UserId")] Plant plant)
        {
            if (id != plant.PlantId)
            {
                return NotFound();
            }

            // Ensure DateTime is in UTC for PostgreSQL
            if (plant.DatePlanted.Kind == DateTimeKind.Unspecified)
            {
                plant.DatePlanted = DateTime.SpecifyKind(plant.DatePlanted, DateTimeKind.Utc);
            }
            else if (plant.DatePlanted.Kind == DateTimeKind.Local)
            {
                plant.DatePlanted = plant.DatePlanted.ToUniversalTime();
            }

            ModelState.Remove("Garden");
            ModelState.Remove("User");
            ModelState.Remove("ImageFile");

            if (ModelState.IsValid)
            {
                try
                {
                    await _plantService.UpdatePlantAsync(id, plant);
                }
                catch (DbUpdateConcurrencyException)
                {
                    var existingPlant = await _plantService.GetPlantByIdAsync(plant.PlantId);
                    if (existingPlant == null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (KeyNotFoundException)
                {
                    return NotFound();
                }
                return RedirectToAction(nameof(Index));
            }
            
            var gardens = await _plantService.GetAllGardensAsync();
            var users = await _plantService.GetAllUsersAsync();
            
            ViewData["GardenId"] = new SelectList(gardens, "GardenId", "GardenDescription", plant.GardenId);
            ViewData["UserId"] = new SelectList(users, "Id", "Id", plant.UserId);
            return View(plant);
        }

        // GET: Plants/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plant = await _plantService.GetPlantByIdAsync(id.Value);
            if (plant == null)
            {
                return NotFound();
            }

            return View(plant);
        }

        // POST: Plants/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleted = await _plantService.DeletePlantAsync(id);
            if (!deleted)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
