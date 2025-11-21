using GardenHub.Models;
using GardenHub.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


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

            // Get active maintenance record
            ViewData["ActiveMaintenance"] = await _equipmentService.GetActiveMaintenanceRecordAsync(id.Value);
            
            // Get maintenance history
            ViewData["MaintenanceHistory"] = await _equipmentService.GetMaintenanceHistoryAsync(id.Value);

            return View(equipment);
        }

        // POST: Equipments/RequestMaintenance/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestMaintenance(int id, string? notes)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "User not authenticated.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                await _equipmentService.RequestMaintenanceAsync(id, userId, notes);
                TempData["SuccessMessage"] = "Maintenance request has been submitted successfully!";
            }
            catch (InvalidOperationException ex)
            {
                TempData["WarningMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Equipments/StartMaintenance/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartMaintenance(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "User not authenticated.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                await _equipmentService.StartMaintenanceAsync(id, userId);
                TempData["InfoMessage"] = "Maintenance has been started. Equipment is now under maintenance.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["WarningMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Equipments/CompleteMaintenance/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteMaintenance(int id, string? completionNotes)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "User not authenticated.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                await _equipmentService.CompleteMaintenanceAsync(id, userId, completionNotes);
                TempData["SuccessMessage"] = "Maintenance has been completed successfully! Equipment is now operational.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["WarningMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id });
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
        public async Task<IActionResult> Create([Bind("EquipmentId,EquipmentName,EquipmentDescription,EquipmentType,PurchaseDate,PurchasePrice,LastMaintenanceDate,ImageFile,GardenId")] Equipment equipment)
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
            else
            {
                // Set LastMaintenanceDate to current date for new equipment
                equipment.LastMaintenanceDate = DateTime.UtcNow;
            }

            // Ensure new equipment is set to Operational status
            equipment.MaintenanceStatus = GardenHub.Models.Enums.MaintenanceStatus.Operational;

            ModelState.Remove("Garden");
            ModelState.Remove("User");
            ModelState.Remove("ImageFile");

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the current logged-in user's ID
                    equipment.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    await _equipmentService.CreateEquipmentAsync(equipment);
                    TempData["SuccessMessage"] = $"Equipment '{equipment.EquipmentName}' has been created successfully and is now operational!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"An error occurred while creating the equipment: {ex.Message}";
                }
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
        public async Task<IActionResult> Edit(int id, [Bind("EquipmentId,EquipmentName,EquipmentDescription,EquipmentType,PurchaseDate,PurchasePrice,LastMaintenanceDate,ImageFile,GardenId")] Equipment equipment)
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
                    TempData["SuccessMessage"] = $"Equipment '{equipment.EquipmentName}' has been updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _equipmentService.EquipmentExistsAsync(equipment.EquipmentId))
                    {
                        TempData["ErrorMessage"] = "The equipment you are trying to update no longer exists.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "A concurrency error occurred. Please try again.";
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"An error occurred while updating the equipment: {ex.Message}";
                }
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
            try
            {
                var equipment = await _equipmentService.GetEquipmentByIdAsync(id);
                var equipmentName = equipment?.EquipmentName ?? "Equipment";
                
                if (equipment != null)
                {
                    await _equipmentService.DeleteEquipmentAsync(id);
                    TempData["SuccessMessage"] = $"Equipment '{equipmentName}' has been deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "The equipment you are trying to delete no longer exists.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the equipment: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
