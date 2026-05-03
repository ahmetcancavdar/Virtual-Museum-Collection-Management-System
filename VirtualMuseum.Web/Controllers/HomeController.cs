using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VirtualMuseum.Web.Data;
using VirtualMuseum.Web.Models.ViewModels;

namespace VirtualMuseum.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly VirtualMuseumDbContext _context;

        public HomeController(VirtualMuseumDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var totalArtists = await _context.Artists.CountAsync();
            var totalArtworks = await _context.Artworks.CountAsync();
            var totalExhibitions = await _context.Exhibitions.CountAsync();
            var totalTours = await _context.Tours.CountAsync();

            var featuredArtworks = await _context.Artworks
                .Include(a => a.Artist)
                .Include(a => a.ArtworkImageUrls)
                .OrderBy(a => a.Title)
                .Take(8)
                .Select(a => new HomeArtworkCardViewModel
                {
                    ArtworkId = a.ArtworkId,
                    Title = a.Title,
                    ArtistName = a.Artist != null
                        ? a.Artist.Name + " " + a.Artist.Surname
                        : "Unknown",
                    Theme = a.Theme,
                    ImageUrl = a.ArtworkImageUrls
                        .OrderBy(i => i.ImageUrl)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault()
                })
                .ToListAsync();

            var featuredArtists = await _context.Artists
                .OrderBy(a => a.Surname)
                .ThenBy(a => a.Name)
                .Select(a => new HomeArtistCardViewModel
                {
                    ArtistId = a.ArtistId,
                    FullName = a.Name + " " + a.Surname,
                    Nationality = a.Nationality,
                    ArtworkCount = _context.Artworks.Count(aw => aw.ArtistId == a.ArtistId)
                })
                .Take(6)
                .ToListAsync();

            var activeExhibitions = await _context.Exhibitions
                .Where(e => e.StartDate <= today && e.EndDate >= today)
                .OrderBy(e => e.EndDate)
                .Take(6)
                .Select(e => new HomeExhibitionCardViewModel
                {
                    ExhibitionId = e.ExhibitionId,
                    Title = e.Title,
                    Theme = e.Theme,
                    RoomName = e.RoomName,
                    DateRangeText = e.StartDate.ToString("yyyy-MM-dd") + " - " + e.EndDate.ToString("yyyy-MM-dd"),
                    ArtworkCount = e.Artworks.Count
                })
                .ToListAsync();

            var upcomingToursRaw = await (
                from t in _context.Tours
                join e in _context.Exhibitions on t.ExhibitionId equals e.ExhibitionId
                where t.TourDate >= today && t.Status == "Open"
                orderby t.TourDate, t.StartTime
                select new
                {
                    t.TourId,
                    t.Title,
                    ExhibitionTitle = e.Title,
                    t.TourDate,
                    t.StartTime,
                    t.EndTime,
                    t.Language,
                    t.Capacity,
                    t.Status
                }
            ).Take(6).ToListAsync();

            var upcomingTourIds = upcomingToursRaw.Select(t => t.TourId).ToList();

            var bookedCounts = await _context.TourBookings
                .Where(tb => upcomingTourIds.Contains(tb.TourId) && tb.Status == "Booked")
                .GroupBy(tb => tb.TourId)
                .Select(g => new { TourId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.TourId, x => x.Count);

            var upcomingTours = upcomingToursRaw
                .Select(t =>
                {
                    var booked = bookedCounts.TryGetValue(t.TourId, out var count) ? count : 0;

                    return new HomeTourCardViewModel
                    {
                        TourId = t.TourId,
                        Title = t.Title,
                        ExhibitionTitle = t.ExhibitionTitle,
                        DateText = t.TourDate.ToString("yyyy-MM-dd"),
                        TimeRange = $"{t.StartTime:HH\\:mm} - {t.EndTime:HH\\:mm}",
                        Language = t.Language,
                        AvailableSeats = t.Capacity - booked,
                        Status = t.Status
                    };
                })
                .ToList();

            var vm = new HomePageViewModel
            {
                TotalArtists = totalArtists,
                TotalArtworks = totalArtworks,
                TotalExhibitions = totalExhibitions,
                TotalTours = totalTours,
                FeaturedArtworks = featuredArtworks,
                FeaturedArtists = featuredArtists,
                ActiveExhibitions = activeExhibitions,
                UpcomingTours = upcomingTours
            };

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
