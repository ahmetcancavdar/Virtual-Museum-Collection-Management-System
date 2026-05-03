using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VirtualMuseum.Web.Data;
using VirtualMuseum.Web.Models;
using VirtualMuseum.Web.Models.ViewModels;

namespace VirtualMuseum.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminExhibitionsController : Controller
    {
        private readonly VirtualMuseumDbContext _context;

        public AdminExhibitionsController(VirtualMuseumDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var exhibitions = await _context.Exhibitions
                .OrderBy(e => e.StartDate)
                .ThenBy(e => e.Title)
                .ToListAsync();

            return View(exhibitions);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new ExhibitionFormViewModel
            {
                IsActive = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExhibitionFormViewModel model)
        {
            if (model.EndDate < model.StartDate)
            {
                ModelState.AddModelError(nameof(model.EndDate), "End date cannot be earlier than start date.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var exhibition = new Exhibition
            {
                Title = model.Title,
                Theme = model.Theme,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                RoomId = model.RoomId,
                RoomName = model.RoomName,
                IsActive = model.IsActive
            };

            _context.Exhibitions.Add(exhibition);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Exhibition created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var exhibition = await _context.Exhibitions.FindAsync(id);

            if (exhibition == null)
            {
                return NotFound();
            }

            var vm = new ExhibitionFormViewModel
            {
                ExhibitionId = exhibition.ExhibitionId,
                Title = exhibition.Title,
                Theme = exhibition.Theme,
                StartDate = exhibition.StartDate,
                EndDate = exhibition.EndDate,
                RoomId = exhibition.RoomId,
                RoomName = exhibition.RoomName,
                IsActive = exhibition.IsActive
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ExhibitionFormViewModel model)
        {
            if (id != model.ExhibitionId)
            {
                return BadRequest();
            }

            if (model.EndDate < model.StartDate)
            {
                ModelState.AddModelError(nameof(model.EndDate), "End date cannot be earlier than start date.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var exhibition = await _context.Exhibitions.FindAsync(id);

            if (exhibition == null)
            {
                return NotFound();
            }

            exhibition.Title = model.Title;
            exhibition.Theme = model.Theme;
            exhibition.StartDate = model.StartDate;
            exhibition.EndDate = model.EndDate;
            exhibition.RoomId = model.RoomId;
            exhibition.RoomName = model.RoomName;
            exhibition.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Exhibition updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var exhibition = await _context.Exhibitions.FindAsync(id);

            if (exhibition == null)
            {
                return NotFound();
            }

            var vm = new ExhibitionDeleteViewModel
            {
                ExhibitionId = exhibition.ExhibitionId,
                Title = exhibition.Title,
                Theme = exhibition.Theme,
                StartDate = exhibition.StartDate,
                EndDate = exhibition.EndDate,
                RoomId = exhibition.RoomId,
                RoomName = exhibition.RoomName,
                IsActive = exhibition.IsActive
            };

            return View(vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var exhibition = await _context.Exhibitions.FindAsync(id);

            if (exhibition == null)
            {
                return NotFound();
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await _context.Database.ExecuteSqlInterpolatedAsync($@"
DELETE FROM [FEATURES]
WHERE [Exhibition_ID] = {id};");

                await _context.Database.ExecuteSqlInterpolatedAsync($@"
DELETE FROM [CURATED_BY]
WHERE [Exhibition_ID] = {id};");

                await _context.Database.ExecuteSqlInterpolatedAsync($@"
DELETE FROM [VISIT]
WHERE [Exhibition_ID] = {id};");

                _context.Exhibitions.Remove(exhibition);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "Exhibition and related records were deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "Delete operation failed: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
