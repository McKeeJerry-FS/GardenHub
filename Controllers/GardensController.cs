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
using GardenHub.Models.Enums;

namespace GardenHub.Controllers
{
    [Authorize]
    public class GardensController : Controller
    {
        private readonly IGardenService _gardenService;
        private readonly IImageService _imageService;

        public GardensController(IGardenService gardenService, IImageService imageService)
        {
            _gardenService = gardenService;
            _imageService = imageService;
        }

        // GET: Gardens
        public async Task<IActionResult> Index()
        {
            var gardens = await _gardenService.GetAllGardens();
            
            // Convert image data for display
            foreach (var garden in gardens)
            {
                garden.ImageFile = null; // Clear for display
                ViewData[$"GardenImage_{garden.GardenId}"] = _imageService.ConvertByteArrayToFile(
                    garden.ImageData, 
                    garden.ImageType, 
                    DefaultImage.GardenImage);
            }
            
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

            // Convert image data for display
            ViewData["GardenImage"] = _imageService.ConvertByteArrayToFile(
                garden.ImageData, 
                garden.ImageType, 
                DefaultImage.GardenImage);

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
        public async Task<IActionResult> Create([Bind("GardenId,GardenName,GardenDescription,GardenLocation,Type,GardenGrowMethod,StartDate,EndDate,UserId,ImageFile")] Garden garden)
        {
            // Handle image upload
            if (garden.ImageFile != null)
            {
                garden.ImageData = await _imageService.ConvertFileToByteArrayAsync(garden.ImageFile);
                garden.ImageType = garden.ImageFile.ContentType;
            }
            
            ModelState.Remove("ImageFile");
            
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
            
            // Convert image data for display
            ViewData["CurrentImage"] = _imageService.ConvertByteArrayToFile(
                garden.ImageData, 
                garden.ImageType, 
                DefaultImage.GardenImage);
            
            ViewData["UserId"] = new SelectList(await _gardenService.GetAllUsers(), "Id", "Id", garden.UserId);
            return View(garden);
        }

        // POST: Gardens/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GardenId,GardenName,GardenDescription,GardenLocation,Type,GardenGrowMethod,StartDate,EndDate,UserId,ImageFile")] Garden garden)
        {
            if (id != garden.GardenId)
            {
                return NotFound();
            }

            // Handle image upload
            if (garden.ImageFile != null)
            {
                garden.ImageData = await _imageService.ConvertFileToByteArrayAsync(garden.ImageFile);
                garden.ImageType = garden.ImageFile.ContentType;
            }
            else
            {
                // Keep existing image if no new one uploaded
                var existingGarden = await _gardenService.GetGardenById(id);
                if (existingGarden != null)
                {
                    garden.ImageData = existingGarden.ImageData;
                    garden.ImageType = existingGarden.ImageType;
                }
            }
            
            ModelState.Remove("ImageFile");

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
