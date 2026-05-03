using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VirtualMuseum.Web.Data;
using VirtualMuseum.Web.Models.ViewModels;

namespace VirtualMuseum.Web.Controllers
{
    public class ExhibitionsController : Controller
    {
        private readonly VirtualMuseumDbContext _context;

        public ExhibitionsController(VirtualMuseumDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchTerm, string? theme, string? roomName, string? dateStatus)
        {
            var query = _context.Exhibitions.AsQueryable();
            var today = DateOnly.FromDateTime(DateTime.Today);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(e => e.Title.Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(theme))
            {
                query = query.Where(e => e.Theme == theme);
            }

            if (!string.IsNullOrWhiteSpace(roomName))
            {
                query = query.Where(e => e.RoomName == roomName);
            }

            if (!string.IsNullOrWhiteSpace(dateStatus))
            {
                query = dateStatus switch
                {
                    "current" => query.Where(e => e.StartDate <= today && e.EndDate >= today),
                    "upcoming" => query.Where(e => e.StartDate > today),
                    "past" => query.Where(e => e.EndDate < today),
                    _ => query
                };
            }

            var themes = await _context.Exhibitions
                .Where(e => e.Theme != null && e.Theme != "")
                .Select(e => e.Theme!)
                .Distinct()
                .OrderBy(t => t)
                .Select(t => new SelectListItem
                {
                    Value = t,
                    Text = t
                })
                .ToListAsync();

            var rooms = await _context.Exhibitions
                .Where(e => e.RoomName != null && e.RoomName != "")
                .Select(e => e.RoomName)
                .Distinct()
                .OrderBy(r => r)
                .Select(r => new SelectListItem
                {
                    Value = r,
                    Text = r
                })
                .ToListAsync();

            var vm = new ExhibitionFilterViewModel
            {
                SearchTerm = searchTerm,
                Theme = theme,
                RoomName = roomName,
                DateStatus = dateStatus,
                ThemeOptions = themes,
                RoomOptions = rooms,
                DateStatusOptions = new List<SelectListItem>
                {
                    new SelectListItem { Value = "current", Text = "Current" },
                    new SelectListItem { Value = "upcoming", Text = "Upcoming" },
                    new SelectListItem { Value = "past", Text = "Past" }
                },
                Results = await query
                    .OrderBy(e => e.StartDate)
                    .ThenBy(e => e.Title)
                    .ToListAsync()
            };

            return View(vm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var exhibition = await _context.Exhibitions
                .Include(e => e.Artworks)
                    .ThenInclude(a => a.Artist)
                .FirstOrDefaultAsync(e => e.ExhibitionId == id);

            if (exhibition == null)
            {
                return NotFound();
            }

            var curatorIds = await GetSelectedIdsAsync(
                "SELECT [User_ID] FROM [CURATED_BY] WHERE [Exhibition_ID] = @ExhibitionId;",
                id);

            var curators = await _context.Users
                .Where(u => curatorIds.Contains(u.UserId))
                .OrderBy(u => u.Name)
                .ThenBy(u => u.Surname)
                .Select(u => u.Name + " " + u.Surname)
                .ToListAsync();

            var toursRaw = await (
                from t in _context.Tours
                join u in _context.Users on t.GuideUserId equals u.UserId
                where t.ExhibitionId == id
                orderby t.TourDate, t.StartTime
                select new
                {
                    t.TourId,
                    t.Title,
                    t.TourDate,
                    t.StartTime,
                    t.EndTime,
                    t.Language,
                    t.Capacity,
                    t.Status,
                    GuideName = u.Name + " " + u.Surname
                }
            ).ToListAsync();

            var tourIds = toursRaw.Select(t => t.TourId).ToList();

            var bookedCounts = await _context.TourBookings
                .Where(tb => tourIds.Contains(tb.TourId) && tb.Status == "Booked")
                .GroupBy(tb => tb.TourId)
                .Select(g => new { TourId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.TourId, x => x.Count);

            var artworkItems = new List<ExhibitionArtworkItemViewModel>();

            foreach (var artwork in exhibition.Artworks.OrderBy(a => a.Title))
            {
                var artworkMovementIds = await GetArtworkMovementIdsAsync(artwork.ArtworkId);

                var artworkMovements = await _context.ArtMovements
                    .Where(m => artworkMovementIds.Contains(m.MovementId))
                    .OrderBy(m => m.MovementName)
                    .Select(m => string.IsNullOrWhiteSpace(m.Era)
                        ? m.MovementName
                        : m.MovementName + " (" + m.Era + ")")
                    .ToListAsync();

                artworkItems.Add(new ExhibitionArtworkItemViewModel
                {
                    ArtworkId = artwork.ArtworkId,
                    Title = artwork.Title,
                    ArtistFullName = artwork.Artist != null
                        ? artwork.Artist.Name + " " + artwork.Artist.Surname
                        : "Unknown",
                    Movements = artworkMovements
                });
            }

            var virtualPlan = await _context.VirtualTourPlans
                .FirstOrDefaultAsync(p => p.ExhibitionId == id && p.IsActive);

            List<VirtualTourStopPublicItemViewModel> virtualStops = new();

            if (virtualPlan != null)
            {
                virtualStops = await _context.VirtualTourStops
                    .Where(s => s.PlanId == virtualPlan.PlanId)
                    .OrderBy(s => s.StepNo)
                    .Select(s => new VirtualTourStopPublicItemViewModel
                    {
                        StepNo = s.StepNo,
                        RoomId = s.RoomId,
                        RoomName = s.RoomName,
                        StopTitle = s.StopTitle,
                        Notes = s.Notes,
                        EstimatedMinutes = s.EstimatedMinutes
                    })
                    .ToListAsync();
            }

            var vm = new ExhibitionDetailsViewModel
            {
                ExhibitionId = exhibition.ExhibitionId,
                Title = exhibition.Title,
                Theme = exhibition.Theme,
                RoomId = exhibition.RoomId,
                RoomName = exhibition.RoomName,
                IsActive = exhibition.IsActive,
                StartDateText = exhibition.StartDate.ToString("yyyy-MM-dd"),
                EndDateText = exhibition.EndDate.ToString("yyyy-MM-dd"),
                Curators = curators,
                Artworks = artworkItems,
                Tours = toursRaw.Select(t =>
                {
                    var booked = bookedCounts.TryGetValue(t.TourId, out var count) ? count : 0;

                    return new TourPublicItemViewModel
                    {
                        TourId = t.TourId,
                        Title = t.Title,
                        TourDate = t.TourDate,
                        TimeRange = $"{t.StartTime:HH\\:mm} - {t.EndTime:HH\\:mm}",
                        Language = t.Language,
                        GuideName = t.GuideName,
                        Capacity = t.Capacity,
                        BookedCount = booked,
                        AvailableSeats = t.Capacity - booked,
                        Status = t.Status
                    };
                }).ToList(),
                VirtualTourPlanTitle = virtualPlan?.Title,
                VirtualTourPlanDescription = virtualPlan?.Description,
                VirtualTourEstimatedMinutes = virtualPlan?.EstimatedDurationMinutes,
                VirtualTourStops = virtualStops
            };

            return View(vm);
        }

        private async Task<List<int>> GetSelectedIdsAsync(string sql, int exhibitionId)
        {
            var result = new List<int>();

            var connection = _context.Database.GetDbConnection();
            var shouldClose = connection.State != ConnectionState.Open;

            if (shouldClose)
            {
                await connection.OpenAsync();
            }

            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = sql;

                var parameter = command.CreateParameter();
                parameter.ParameterName = "@ExhibitionId";
                parameter.Value = exhibitionId;
                command.Parameters.Add(parameter);

                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    result.Add(reader.GetInt32(0));
                }
            }
            finally
            {
                if (shouldClose)
                {
                    await connection.CloseAsync();
                }
            }

            return result;
        }

        private async Task<List<int>> GetArtworkMovementIdsAsync(int artworkId)
        {
            var result = new List<int>();

            var connection = _context.Database.GetDbConnection();
            var shouldClose = connection.State != ConnectionState.Open;

            if (shouldClose)
            {
                await connection.OpenAsync();
            }

            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = @"
SELECT [Movement_ID]
FROM [BELONGS_TO]
WHERE [Artwork_ID] = @ArtworkId;";

                var parameter = command.CreateParameter();
                parameter.ParameterName = "@ArtworkId";
                parameter.Value = artworkId;
                command.Parameters.Add(parameter);

                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    result.Add(reader.GetInt32(0));
                }
            }
            finally
            {
                if (shouldClose)
                {
                    await connection.CloseAsync();
                }
            }

            return result;
        }
    }
}
