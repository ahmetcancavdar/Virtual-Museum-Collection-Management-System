using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VirtualMuseum.Web.Data;
using VirtualMuseum.Web.Models.ViewModels;

namespace VirtualMuseum.Web.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class AdminVisitsController : Controller
    {
        private readonly VirtualMuseumDbContext _context;

        public AdminVisitsController(VirtualMuseumDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var visits = await (
                from v in _context.Visits
                join u in _context.Users on v.UserId equals u.UserId
                join e in _context.Exhibitions on v.ExhibitionId equals e.ExhibitionId
                orderby v.VisitDate descending, e.Title
                select new VisitManagementItemViewModel
                {
                    UserId = u.UserId,
                    VisitorName = u.Name + " " + u.Surname,
                    ExhibitionId = e.ExhibitionId,
                    ExhibitionTitle = e.Title,
                    RoomName = e.RoomName,
                    VisitDate = v.VisitDate,
                    Status = v.Status
                }
            ).ToListAsync();

            return View(visits);
        }

        [HttpGet]
        public async Task<IActionResult> EditStatus(int userId, int exhibitionId, DateOnly visitDate)
        {
            var item = await (
                from v in _context.Visits
                join u in _context.Users on v.UserId equals u.UserId
                join e in _context.Exhibitions on v.ExhibitionId equals e.ExhibitionId
                where v.UserId == userId && v.ExhibitionId == exhibitionId && v.VisitDate == visitDate
                select new VisitStatusEditViewModel
                {
                    UserId = v.UserId,
                    ExhibitionId = v.ExhibitionId,
                    VisitDate = v.VisitDate,
                    VisitorName = u.Name + " " + u.Surname,
                    ExhibitionTitle = e.Title,
                    Status = v.Status
                }
            ).FirstOrDefaultAsync();

            if (item == null)
            {
                return NotFound();
            }

            ViewBag.StatusOptions = new List<string> { "Planned", "Completed", "Cancelled" };
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStatus(VisitStatusEditViewModel model)
        {
            var validStatuses = new[] { "Planned", "Completed", "Cancelled" };

            if (!validStatuses.Contains(model.Status))
            {
                ModelState.AddModelError(nameof(model.Status), "Invalid status.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.StatusOptions = new List<string> { "Planned", "Completed", "Cancelled" };
                return View(model);
            }

            var visit = await _context.Visits.FirstOrDefaultAsync(v =>
                v.UserId == model.UserId &&
                v.ExhibitionId == model.ExhibitionId &&
                v.VisitDate == model.VisitDate);

            if (visit == null)
            {
                return NotFound();
            }

            visit.Status = model.Status;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Visit status updated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
