using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VirtualMuseum.Web.Data;
using VirtualMuseum.Web.Models;
using VirtualMuseum.Web.Models.ViewModels;

namespace VirtualMuseum.Web.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class AdminToursController : Controller
    {
        private readonly VirtualMuseumDbContext _context;

        public AdminToursController(VirtualMuseumDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var items = await (
                from t in _context.Tours
                join e in _context.Exhibitions on t.ExhibitionId equals e.ExhibitionId
                join u in _context.Users on t.GuideUserId equals u.UserId
                orderby t.TourDate, t.StartTime
                select new TourListItemViewModel
                {
                    TourId = t.TourId,
                    Title = t.Title,
                    ExhibitionTitle = e.Title,
                    TourDate = t.TourDate,
                    StartTime = t.StartTime,
                    EndTime = t.EndTime,
                    Capacity = t.Capacity,
                    Language = t.Language,
                    GuideName = u.Name + " " + u.Surname,
                    Status = t.Status
                }
            ).ToListAsync();

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateDropDownsAsync();
            return View(new TourFormViewModel
            {
                TourDate = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(11, 0),
                EndTime = new TimeOnly(12, 0),
                Capacity = 20,
                Language = "English",
                Status = "Open"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TourFormViewModel model)
        {
            await ValidateTourModelAsync(model);

            if (!ModelState.IsValid)
            {
                await PopulateDropDownsAsync(model.ExhibitionId, model.GuideUserId, model.Status);
                return View(model);
            }

            var tour = new Tour
            {
                ExhibitionId = model.ExhibitionId,
                Title = model.Title,
                Description = model.Description,
                TourDate = model.TourDate,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                Capacity = model.Capacity,
                Language = model.Language,
                GuideUserId = model.GuideUserId,
                Status = model.Status
            };

            _context.Tours.Add(tour);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Tour created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var tour = await _context.Tours.FindAsync(id);

            if (tour == null)
            {
                return NotFound();
            }

            var vm = new TourFormViewModel
            {
                TourId = tour.TourId,
                ExhibitionId = tour.ExhibitionId,
                Title = tour.Title,
                Description = tour.Description,
                TourDate = tour.TourDate,
                StartTime = tour.StartTime,
                EndTime = tour.EndTime,
                Capacity = tour.Capacity,
                Language = tour.Language,
                GuideUserId = tour.GuideUserId,
                Status = tour.Status
            };

            await PopulateDropDownsAsync(vm.ExhibitionId, vm.GuideUserId, vm.Status);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TourFormViewModel model)
        {
            if (id != model.TourId)
            {
                return BadRequest();
            }

            await ValidateTourModelAsync(model);

            if (!ModelState.IsValid)
            {
                await PopulateDropDownsAsync(model.ExhibitionId, model.GuideUserId, model.Status);
                return View(model);
            }

            var tour = await _context.Tours.FindAsync(id);

            if (tour == null)
            {
                return NotFound();
            }

            tour.ExhibitionId = model.ExhibitionId;
            tour.Title = model.Title;
            tour.Description = model.Description;
            tour.TourDate = model.TourDate;
            tour.StartTime = model.StartTime;
            tour.EndTime = model.EndTime;
            tour.Capacity = model.Capacity;
            tour.Language = model.Language;
            tour.GuideUserId = model.GuideUserId;
            tour.Status = model.Status;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Tour updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await (
                from t in _context.Tours
                join e in _context.Exhibitions on t.ExhibitionId equals e.ExhibitionId
                join u in _context.Users on t.GuideUserId equals u.UserId
                where t.TourId == id
                select new TourDeleteViewModel
                {
                    TourId = t.TourId,
                    Title = t.Title,
                    ExhibitionTitle = e.Title,
                    TourDate = t.TourDate,
                    StartTime = t.StartTime,
                    EndTime = t.EndTime,
                    GuideName = u.Name + " " + u.Surname,
                    BookingCount = _context.TourBookings.Count(tb => tb.TourId == t.TourId),
                    Status = t.Status
                }
            ).FirstOrDefaultAsync();

            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tour = await _context.Tours.FindAsync(id);

            if (tour == null)
            {
                return NotFound();
            }

            var bookings = await _context.TourBookings
                .Where(tb => tb.TourId == id)
                .ToListAsync();

            _context.TourBookings.RemoveRange(bookings);
            _context.Tours.Remove(tour);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Tour and related bookings were deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task ValidateTourModelAsync(TourFormViewModel model)
        {
            if (model.EndTime <= model.StartTime)
            {
                ModelState.AddModelError(nameof(model.EndTime), "End time must be later than start time.");
            }

            var validStatuses = new[] { "Open", "Full", "Cancelled", "Completed" };
            if (!validStatuses.Contains(model.Status))
            {
                ModelState.AddModelError(nameof(model.Status), "Invalid status.");
            }

            if (!await _context.Exhibitions.AnyAsync(e => e.ExhibitionId == model.ExhibitionId))
            {
                ModelState.AddModelError(nameof(model.ExhibitionId), "Selected exhibition was not found.");
            }

            if (!await _context.Staff.AnyAsync(s => s.UserId == model.GuideUserId))
            {
                ModelState.AddModelError(nameof(model.GuideUserId), "Selected guide must be a staff user.");
            }
        }

        private async Task PopulateDropDownsAsync(int? selectedExhibitionId = null, int? selectedGuideUserId = null, string? selectedStatus = null)
        {
            var exhibitions = await _context.Exhibitions
                .OrderBy(e => e.Title)
                .Select(e => new SelectListItem
                {
                    Value = e.ExhibitionId.ToString(),
                    Text = e.Title
                })
                .ToListAsync();

            var guides = await (
                from s in _context.Staff
                join u in _context.Users on s.UserId equals u.UserId
                orderby u.Name, u.Surname
                select new SelectListItem
                {
                    Value = u.UserId.ToString(),
                    Text = u.Name + " " + u.Surname
                }
            ).ToListAsync();

            var statuses = new List<SelectListItem>
            {
                new SelectListItem { Value = "Open", Text = "Open" },
                new SelectListItem { Value = "Full", Text = "Full" },
                new SelectListItem { Value = "Cancelled", Text = "Cancelled" },
                new SelectListItem { Value = "Completed", Text = "Completed" }
            };

            ViewBag.Exhibitions = new SelectList(exhibitions, "Value", "Text", selectedExhibitionId?.ToString());
            ViewBag.Guides = new SelectList(guides, "Value", "Text", selectedGuideUserId?.ToString());
            ViewBag.Statuses = new SelectList(statuses, "Value", "Text", selectedStatus);
        }
    }
}
