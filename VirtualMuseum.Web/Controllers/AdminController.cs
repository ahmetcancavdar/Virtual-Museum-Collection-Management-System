using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VirtualMuseum.Web.Data;
using VirtualMuseum.Web.Models.ViewModels;

namespace VirtualMuseum.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly VirtualMuseumDbContext _context;

        public AdminController(VirtualMuseumDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var vm = new AdminDashboardViewModel
            {
                TotalArtists = await _context.Artists.CountAsync(),
                TotalArtworks = await _context.Artworks.CountAsync(),
                TotalExhibitions = await _context.Exhibitions.CountAsync(),
                TotalTours = await _context.Tours.CountAsync(),

                TodayVisitCount = await _context.Visits.CountAsync(v => v.VisitDate == today),
                PlannedVisitCount = await _context.Visits.CountAsync(v => v.Status == "Planned"),
                CompletedVisitCount = await _context.Visits.CountAsync(v => v.Status == "Completed"),
                CancelledVisitCount = await _context.Visits.CountAsync(v => v.Status == "Cancelled"),

                OpenTourCount = await _context.Tours.CountAsync(t => t.Status == "Open"),
                UpcomingTourCount = await _context.Tours.CountAsync(t => t.TourDate >= today),
                TotalTourBookingCount = await _context.TourBookings.CountAsync(),

                RecentVisits = await (
                    from v in _context.Visits
                    join u in _context.Users on v.UserId equals u.UserId
                    join e in _context.Exhibitions on v.ExhibitionId equals e.ExhibitionId
                    orderby v.VisitDate descending, e.Title
                    select new AdminDashboardVisitItemViewModel
                    {
                        VisitorName = u.Name + " " + u.Surname,
                        ExhibitionTitle = e.Title,
                        VisitDate = v.VisitDate,
                        Status = v.Status
                    }
                ).Take(8).ToListAsync(),

                RecentTourBookings = await (
                    from tb in _context.TourBookings
                    join u in _context.Users on tb.UserId equals u.UserId
                    join t in _context.Tours on tb.TourId equals t.TourId
                    join e in _context.Exhibitions on t.ExhibitionId equals e.ExhibitionId
                    orderby tb.BookingDate descending, t.TourDate descending
                    select new AdminDashboardTourBookingItemViewModel
                    {
                        VisitorName = u.Name + " " + u.Surname,
                        TourTitle = t.Title,
                        ExhibitionTitle = e.Title,
                        TourDate = t.TourDate,
                        Status = tb.Status
                    }
                ).Take(8).ToListAsync()
            };

            return View(vm);
        }
    }
}
