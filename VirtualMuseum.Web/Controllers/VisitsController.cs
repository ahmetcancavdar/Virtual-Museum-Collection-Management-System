using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VirtualMuseum.Web.Data;
using VirtualMuseum.Web.Models;
using VirtualMuseum.Web.Models.ViewModels;

namespace VirtualMuseum.Web.Controllers
{
    [Authorize(Roles = "Visitor")]
    public class VisitsController : Controller
    {
        private readonly VirtualMuseumDbContext _context;

        public VisitsController(VirtualMuseumDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Book(int exhibitionId)
        {
            var exhibition = await _context.Exhibitions
                .FirstOrDefaultAsync(e => e.ExhibitionId == exhibitionId);

            if (exhibition == null)
            {
                return NotFound();
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            var defaultVisitDate = exhibition.StartDate > today ? exhibition.StartDate : today;

            if (defaultVisitDate > exhibition.EndDate)
            {
                defaultVisitDate = exhibition.EndDate;
            }

            var vm = new VisitBookingViewModel
            {
                ExhibitionId = exhibition.ExhibitionId,
                ExhibitionTitle = exhibition.Title,
                Theme = exhibition.Theme,
                RoomName = exhibition.RoomName,
                StartDate = exhibition.StartDate,
                EndDate = exhibition.EndDate,
                VisitDate = defaultVisitDate
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(VisitBookingViewModel model)
        {
            var exhibition = await _context.Exhibitions
                .FirstOrDefaultAsync(e => e.ExhibitionId == model.ExhibitionId);

            if (exhibition == null)
            {
                return NotFound();
            }

            if (model.VisitDate < exhibition.StartDate || model.VisitDate > exhibition.EndDate)
            {
                ModelState.AddModelError(nameof(model.VisitDate), "Visit date must be within the exhibition date range.");
            }

            var userId = GetCurrentUserId();

            var alreadyExists = await _context.Visits.AnyAsync(v =>
                v.UserId == userId &&
                v.ExhibitionId == model.ExhibitionId &&
                v.VisitDate == model.VisitDate);

            if (alreadyExists)
            {
                ModelState.AddModelError("", "You already have a visit booking for this exhibition on the selected date.");
            }

            if (!ModelState.IsValid)
            {
                model.ExhibitionTitle = exhibition.Title;
                model.Theme = exhibition.Theme;
                model.RoomName = exhibition.RoomName;
                model.StartDate = exhibition.StartDate;
                model.EndDate = exhibition.EndDate;
                return View(model);
            }

            var visit = new Visit
            {
                UserId = userId,
                ExhibitionId = model.ExhibitionId,
                VisitDate = model.VisitDate,
                Status = "Planned"
            };

            _context.Visits.Add(visit);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Visit booked successfully.";
            return RedirectToAction(nameof(MyVisits));
        }

        [HttpGet]
        public async Task<IActionResult> MyVisits()
        {
            var userId = GetCurrentUserId();

            var visits = await (
                from v in _context.Visits
                join e in _context.Exhibitions on v.ExhibitionId equals e.ExhibitionId
                where v.UserId == userId
                orderby v.VisitDate descending, e.Title
                select new MyVisitItemViewModel
                {
                    ExhibitionId = e.ExhibitionId,
                    ExhibitionTitle = e.Title,
                    RoomName = e.RoomName,
                    VisitDate = v.VisitDate,
                    Status = v.Status
                }
            ).ToListAsync();

            return View(visits);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int exhibitionId, DateOnly visitDate)
        {
            var userId = GetCurrentUserId();

            var visit = await _context.Visits.FirstOrDefaultAsync(v =>
                v.UserId == userId &&
                v.ExhibitionId == exhibitionId &&
                v.VisitDate == visitDate);

            if (visit == null)
            {
                return NotFound();
            }

            visit.Status = "Cancelled";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Visit cancelled successfully.";
            return RedirectToAction(nameof(MyVisits));
        }

        private int GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userIdValue))
            {
                throw new InvalidOperationException("Authenticated user id could not be found.");
            }

            return int.Parse(userIdValue);
        }
    }
}
