using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VirtualMuseum.Web.Data;
using VirtualMuseum.Web.Models.ViewModels;

namespace VirtualMuseum.Web.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class AdminTourBookingsController : Controller
    {
        private readonly VirtualMuseumDbContext _context;

        public AdminTourBookingsController(VirtualMuseumDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var items = await (
                from tb in _context.TourBookings
                join u in _context.Users on tb.UserId equals u.UserId
                join t in _context.Tours on tb.TourId equals t.TourId
                join e in _context.Exhibitions on t.ExhibitionId equals e.ExhibitionId
                orderby t.TourDate descending, t.StartTime
                select new AdminTourBookingItemViewModel
                {
                    BookingId = tb.BookingId,
                    VisitorName = u.Name + " " + u.Surname,
                    TourTitle = t.Title,
                    ExhibitionTitle = e.Title,
                    TourDate = t.TourDate,
                    TimeRange = $"{t.StartTime:HH\\:mm} - {t.EndTime:HH\\:mm}",
                    Status = tb.Status
                }
            ).ToListAsync();

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> EditStatus(int bookingId)
        {
            var item = await (
                from tb in _context.TourBookings
                join u in _context.Users on tb.UserId equals u.UserId
                join t in _context.Tours on tb.TourId equals t.TourId
                join e in _context.Exhibitions on t.ExhibitionId equals e.ExhibitionId
                where tb.BookingId == bookingId
                select new AdminTourBookingStatusEditViewModel
                {
                    BookingId = tb.BookingId,
                    VisitorName = u.Name + " " + u.Surname,
                    TourTitle = t.Title,
                    ExhibitionTitle = e.Title,
                    TourDate = t.TourDate,
                    TimeRange = $"{t.StartTime:HH\\:mm} - {t.EndTime:HH\\:mm}",
                    Status = tb.Status
                }
            ).FirstOrDefaultAsync();

            if (item == null)
            {
                return NotFound();
            }

            ViewBag.StatusOptions = new List<string> { "Booked", "Cancelled", "Completed", "NoShow" };
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStatus(AdminTourBookingStatusEditViewModel model)
        {
            var validStatuses = new[] { "Booked", "Cancelled", "Completed", "NoShow" };

            if (!validStatuses.Contains(model.Status))
            {
                ModelState.AddModelError(nameof(model.Status), "Invalid status.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.StatusOptions = new List<string> { "Booked", "Cancelled", "Completed", "NoShow" };
                return View(model);
            }

            var booking = await _context.TourBookings.FindAsync(model.BookingId);

            if (booking == null)
            {
                return NotFound();
            }

            booking.Status = model.Status;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Tour booking status updated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
