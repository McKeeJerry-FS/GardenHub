using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GardenHub.Models;
using Microsoft.AspNetCore.Authorization;
using GardenHub.Services.Interfaces;


namespace GardenHub.Controllers
{
    [Authorize]
    public class EquipmentsController : Controller
    {
        
        private readonly IImageService _imageService;
        private readonly IEquipmentService _equipmentService;


        public EquipmentsController(IEquipmentService equipmentService, IImageService imageService)
        {
            
            _equipmentService = equipmentService;
            _imageService = imageService;
        }

        // GET: Equipments
        public async Task<IActionResult> Index()
        {
            var equipments = await _equipmentService.GetAllEquipmentsAsync();
            return View(equipments);
        }

        // GET: Equipments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var equipment = await _equipmentService.GetEquipmentByIdAsync(id.Value);
            if (equipment == null)
            {
                return NotFound();
            }

            return View(equipment);
        }

        // GET: Equipments/Create
        public async Task<IActionResult> Create()
        {
            var gardens = await _equipmentService.GetAllGardensAsync();
            var users = await _equipmentService.GetAllUsersAsync();
            ViewData["GardenId"] = new SelectList(gardens, "GardenId", "GardenDescription");
            ViewData["UserId"] = new SelectList(users, "Id", "Id");
            return View();
        }

        // POST: Equipments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EquipmentId,EquipmentName,EquipmentDescription,EquipmentType,PurchaseDate,PurchasePrice,LastMaintenanceDate,ImageFile,GardenId,UserId")] Equipment equipment)
        {
            // Process image upload
            if (equipment.ImageFile != null)
            {
                equipment.ImageData = await _imageService.ConvertFileToByteArrayAsync(equipment.ImageFile);
                equipment.ImageType = equipment.ImageFile.ContentType;
            }

            // Ensure DateTime is in UTC for PostgreSQL
            equipment.PurchaseDate = equipment.PurchaseDate.Kind switch
            {
                DateTimeKind.Unspecified => DateTime.SpecifyKind(equipment.PurchaseDate, DateTimeKind.Utc),
                DateTimeKind.Local => equipment.PurchaseDate.ToUniversalTime(),
                _ => equipment.PurchaseDate
            };

            // Handle LastMaintenanceDate if set
            if (equipment.LastMaintenanceDate != default)
            {
                equipment.LastMaintenanceDate = equipment.LastMaintenanceDate.Kind switch
                {
                    DateTimeKind.Unspecified => DateTime.SpecifyKind(equipment.LastMaintenanceDate, DateTimeKind.Utc),
                    DateTimeKind.Local => equipment.LastMaintenanceDate.ToUniversalTime(),
                    _ => equipment.LastMaintenanceDate
                };
            }

            ModelState.Remove("Garden");
            ModelState.Remove("User");
            ModelState.Remove("ImageFile");

            if (ModelState.IsValid)
            {
                await _equipmentService.CreateEquipmentAsync(equipment);
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["GardenId"] = new SelectList(await _equipmentService.GetAllGardensAsync(), "GardenId", "GardenDescription", equipment.GardenId);
            ViewData["UserId"] = new SelectList(await _equipmentService.GetAllUsersAsync(), "Id", "Id", equipment.UserId);
            return View(equipment);
        }

        // GET: Equipments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var equipment = await _equipmentService.GetEquipmentByIdAsync(id.Value);
            if (equipment == null)
            {
                return NotFound();
            }
            ViewData["GardenId"] = new SelectList(await _equipmentService.GetAllGardensAsync(), "GardenId", "GardenDescription", equipment.GardenId);
            ViewData["UserId"] = new SelectList(await _equipmentService.GetAllUsersAsync(), "Id", "Id", equipment.UserId);
            return View(equipment);
        }

        // POST: Equipments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EquipmentId,EquipmentName,EquipmentDescription,EquipmentType,PurchaseDate,PurchasePrice,LastMaintenanceDate,ImageFile,GardenId,UserId")] Equipment equipment)
        {
            if (id != equipment.EquipmentId)
            {
                return NotFound();
            }

            // Ensure DateTime is in UTC for PostgreSQL
            equipment.PurchaseDate = equipment.PurchaseDate.Kind switch
            {
                DateTimeKind.Unspecified => DateTime.SpecifyKind(equipment.PurchaseDate, DateTimeKind.Utc),
                DateTimeKind.Local => equipment.PurchaseDate.ToUniversalTime(),
                _ => equipment.PurchaseDate
            };

            // Handle LastMaintenanceDate if set
            if (equipment.LastMaintenanceDate != default)
            {
                equipment.LastMaintenanceDate = equipment.LastMaintenanceDate.Kind switch
                {
                    DateTimeKind.Unspecified => DateTime.SpecifyKind(equipment.LastMaintenanceDate, DateTimeKind.Utc),
                    DateTimeKind.Local => equipment.LastMaintenanceDate.ToUniversalTime(),
                    _ => equipment.LastMaintenanceDate
                };
            }

            ModelState.Remove("Garden");
            ModelState.Remove("User");
            ModelState.Remove("ImageFile");

            if (ModelState.IsValid)
            {
                try
                {
                    await _equipmentService.UpdateEquipmentAsync(id, equipment);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _equipmentService.EquipmentExistsAsync(equipment.EquipmentId))
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
            
            ViewData["GardenId"] = new SelectList(await _equipmentService.GetAllGardensAsync(), "GardenId", "GardenDescription", equipment.GardenId);
            ViewData["UserId"] = new SelectList(await _equipmentService.GetAllUsersAsync(), "Id", "Id", equipment.UserId);
            return View(equipment);
        }

        // GET: Equipments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var equipment = await _equipmentService.GetEquipmentByIdAsync(id.Value);

            if (equipment == null)
            {
                return NotFound();
            }

            return View(equipment);
        }

        // POST: Equipments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var equipment = await _equipmentService.GetEquipmentByIdAsync(id);
            if (equipment != null)
            {
                await _equipmentService.DeleteEquipmentAsync(id);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
