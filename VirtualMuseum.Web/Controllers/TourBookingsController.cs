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
    public class TourBookingsController : Controller
    {
        private readonly VirtualMuseumDbContext _context;

        public TourBookingsController(VirtualMuseumDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Book(int tourId)
        {
            var item = await (
                from t in _context.Tours
                join e in _context.Exhibitions on t.ExhibitionId equals e.ExhibitionId
                join u in _context.Users on t.GuideUserId equals u.UserId
                where t.TourId == tourId
                select new
                {
                    Tour = t,
                    ExhibitionTitle = e.Title,
                    GuideName = u.Name + " " + u.Surname
                }
            ).FirstOrDefaultAsync();

            if (item == null)
            {
                return NotFound();
            }

            var bookedCount = await _context.TourBookings.CountAsync(tb =>
                tb.TourId == tourId && tb.Status == "Booked");

            var vm = new TourBookingViewModel
            {
                TourId = item.Tour.TourId,
                TourTitle = item.Tour.Title,
                ExhibitionTitle = item.ExhibitionTitle,
                TourDate = item.Tour.TourDate,
                TimeRange = $"{item.Tour.StartTime:HH\\:mm} - {item.Tour.EndTime:HH\\:mm}",
                Language = item.Tour.Language,
                GuideName = item.GuideName,
                Capacity = item.Tour.Capacity,
                BookedCount = bookedCount,
                AvailableSeats = item.Tour.Capacity - bookedCount,
                Status = item.Tour.Status
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(TourBookingViewModel model)
        {
            var tour = await _context.Tours.FindAsync(model.TourId);

            if (tour == null)
            {
                return NotFound();
            }

            var bookedCount = await _context.TourBookings.CountAsync(tb =>
                tb.TourId == model.TourId && tb.Status == "Booked");

            var userId = GetCurrentUserId();

            var alreadyBooked = await _context.TourBookings.AnyAsync(tb =>
                tb.TourId == model.TourId &&
                tb.UserId == userId &&
                tb.Status == "Booked");

            if (!model.ConfirmBooking)
            {
                ModelState.AddModelError(nameof(model.ConfirmBooking), "You must confirm the booking.");
            }

            if (tour.Status != "Open")
            {
                ModelState.AddModelError("", "This tour is not open for booking.");
            }

            if (bookedCount >= tour.Capacity)
            {
                ModelState.AddModelError("", "This tour is already full.");
            }

            if (alreadyBooked)
            {
                ModelState.AddModelError("", "You already booked this tour.");
            }

            if (!ModelState.IsValid)
            {
                var item = await (
                    from t in _context.Tours
                    join e in _context.Exhibitions on t.ExhibitionId equals e.ExhibitionId
                    join u in _context.Users on t.GuideUserId equals u.UserId
                    where t.TourId == model.TourId
                    select new
                    {
                        Tour = t,
                        ExhibitionTitle = e.Title,
                        GuideName = u.Name + " " + u.Surname
                    }
                ).FirstAsync();

                model.TourTitle = item.Tour.Title;
                model.ExhibitionTitle = item.ExhibitionTitle;
                model.TourDate = item.Tour.TourDate;
                model.TimeRange = $"{item.Tour.StartTime:HH\\:mm} - {item.Tour.EndTime:HH\\:mm}";
                model.Language = item.Tour.Language;
                model.GuideName = item.GuideName;
                model.Capacity = item.Tour.Capacity;
                model.BookedCount = bookedCount;
                model.AvailableSeats = item.Tour.Capacity - bookedCount;
                model.Status = item.Tour.Status;

                return View(model);
            }

            var booking = new TourBooking
            {
                UserId = userId,
                TourId = model.TourId,
                BookingDate = DateOnly.FromDateTime(DateTime.Today),
                Status = "Booked"
            };

            _context.TourBookings.Add(booking);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Tour booked successfully.";
            return RedirectToAction(nameof(MyBookings));
        }

        [HttpGet]
        public async Task<IActionResult> MyBookings()
        {
            var userId = GetCurrentUserId();

            var items = await (
                from tb in _context.TourBookings
                join t in _context.Tours on tb.TourId equals t.TourId
                join e in _context.Exhibitions on t.ExhibitionId equals e.ExhibitionId
                where tb.UserId == userId
                orderby t.TourDate descending, t.StartTime
                select new MyTourBookingItemViewModel
                {
                    BookingId = tb.BookingId,
                    TourId = t.TourId,
                    TourTitle = t.Title,
                    ExhibitionTitle = e.Title,
                    TourDate = t.TourDate,
                    TimeRange = $"{t.StartTime:HH\\:mm} - {t.EndTime:HH\\:mm}",
                    Language = t.Language,
                    Status = tb.Status
                }
            ).ToListAsync();

            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int bookingId)
        {
            var userId = GetCurrentUserId();

            var booking = await _context.TourBookings.FirstOrDefaultAsync(tb =>
                tb.BookingId == bookingId && tb.UserId == userId);

            if (booking == null)
            {
                return NotFound();
            }

            booking.Status = "Cancelled";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Tour booking cancelled successfully.";
            return RedirectToAction(nameof(MyBookings));
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
