using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VirtualMuseum.Web.Data;
using VirtualMuseum.Web.Models;
using VirtualMuseum.Web.Models.ViewModels;

namespace VirtualMuseum.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminArtMovementsController : Controller
    {
        private readonly VirtualMuseumDbContext _context;

        public AdminArtMovementsController(VirtualMuseumDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var movements = await _context.ArtMovements
                .OrderBy(m => m.MovementName)
                .ToListAsync();

            return View(movements);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new ArtMovementFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ArtMovementFormViewModel model)
        {
            model.MovementName = model.MovementName.Trim();
            model.RegionOfOrigin = string.IsNullOrWhiteSpace(model.RegionOfOrigin) ? null : model.RegionOfOrigin.Trim();
            model.Era = string.IsNullOrWhiteSpace(model.Era) ? null : model.Era.Trim();
            model.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();

            var duplicateExists = await _context.ArtMovements.AnyAsync(m =>
                m.MovementName == model.MovementName);

            if (duplicateExists)
            {
                ModelState.AddModelError("", "A movement with the same name already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var movement = new ArtMovement
            {
                MovementName = model.MovementName,
                RegionOfOrigin = model.RegionOfOrigin,
                Era = model.Era,
                Description = model.Description
            };

            _context.ArtMovements.Add(movement);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Art movement created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var movement = await _context.ArtMovements.FindAsync(id);

            if (movement == null)
            {
                return NotFound();
            }

            var vm = new ArtMovementFormViewModel
            {
                MovementId = movement.MovementId,
                MovementName = movement.MovementName,
                RegionOfOrigin = movement.RegionOfOrigin,
                Era = movement.Era,
                Description = movement.Description
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ArtMovementFormViewModel model)
        {
            if (id != model.MovementId)
            {
                return BadRequest();
            }

            model.MovementName = model.MovementName.Trim();
            model.RegionOfOrigin = string.IsNullOrWhiteSpace(model.RegionOfOrigin) ? null : model.RegionOfOrigin.Trim();
            model.Era = string.IsNullOrWhiteSpace(model.Era) ? null : model.Era.Trim();
            model.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();

            var duplicateExists = await _context.ArtMovements.AnyAsync(m =>
                m.MovementId != model.MovementId &&
                m.MovementName == model.MovementName);

            if (duplicateExists)
            {
                ModelState.AddModelError("", "Another movement with the same name already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var movement = await _context.ArtMovements.FindAsync(id);

            if (movement == null)
            {
                return NotFound();
            }

            movement.MovementName = model.MovementName;
            movement.RegionOfOrigin = model.RegionOfOrigin;
            movement.Era = model.Era;
            movement.Description = model.Description;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Art movement updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var movement = await _context.ArtMovements.FindAsync(id);

            if (movement == null)
            {
                return NotFound();
            }

            var linkedArtistCount = await CountLinkedArtistsAsync(id);
            var linkedArtworkCount = await CountLinkedArtworksAsync(id);

            var vm = new ArtMovementDeleteViewModel
            {
                MovementId = movement.MovementId,
                MovementName = movement.MovementName,
                RegionOfOrigin = movement.RegionOfOrigin,
                Era = movement.Era,
                Description = movement.Description,
                LinkedArtistCount = linkedArtistCount,
                LinkedArtworkCount = linkedArtworkCount
            };

            return View(vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movement = await _context.ArtMovements.FindAsync(id);

            if (movement == null)
            {
                return NotFound();
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await _context.Database.ExecuteSqlInterpolatedAsync($@"
DELETE FROM [INFLUENCED_BY]
WHERE [Movement_ID] = {id};");

                await _context.Database.ExecuteSqlInterpolatedAsync($@"
DELETE FROM [BELONGS_TO]
WHERE [Movement_ID] = {id};");

                _context.ArtMovements.Remove(movement);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "Art movement and related links were deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "Delete failed: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task<int> CountLinkedArtistsAsync(int movementId)
        {
            return await _context.Database.SqlQueryRaw<int>($@"
SELECT COUNT(*)
FROM [INFLUENCED_BY]
WHERE [Movement_ID] = {movementId}")
                .FirstAsync();
        }

        private async Task<int> CountLinkedArtworksAsync(int movementId)
        {
            return await _context.Database.SqlQueryRaw<int>($@"
SELECT COUNT(*)
FROM [BELONGS_TO]
WHERE [Movement_ID] = {movementId}")
                .FirstAsync();
        }
    }
}
