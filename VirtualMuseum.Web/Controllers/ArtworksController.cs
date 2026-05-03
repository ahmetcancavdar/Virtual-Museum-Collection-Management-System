using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VirtualMuseum.Web.Data;
using VirtualMuseum.Web.Models.ViewModels;

namespace VirtualMuseum.Web.Controllers
{
    public class ArtworksController : Controller
    {
        private readonly VirtualMuseumDbContext _context;

        public ArtworksController(VirtualMuseumDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            string? searchTerm,
            int? artistId,
            string? theme,
            string? period,
            int? movementId,
            string? technique,
            string? medium)
        {
            var query = _context.Artworks
                .Include(a => a.Artist)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(a => a.Title.Contains(term));
            }

            if (artistId.HasValue)
            {
                query = query.Where(a => a.ArtistId == artistId.Value);
            }

            if (!string.IsNullOrWhiteSpace(theme))
            {
                query = query.Where(a => a.Theme == theme);
            }

            if (!string.IsNullOrWhiteSpace(technique))
            {
                query = query.Where(a => a.Technique == technique);
            }

            if (!string.IsNullOrWhiteSpace(medium))
            {
                query = query.Where(a => a.Medium == medium);
            }

            if (!string.IsNullOrWhiteSpace(period))
            {
                query = period switch
                {
                    "before1600" => query.Where(a => a.CreationYear.HasValue && a.CreationYear.Value < 1600),
                    "1600to1799" => query.Where(a => a.CreationYear.HasValue && a.CreationYear.Value >= 1600 && a.CreationYear.Value <= 1799),
                    "1800to1899" => query.Where(a => a.CreationYear.HasValue && a.CreationYear.Value >= 1800 && a.CreationYear.Value <= 1899),
                    "1900to1949" => query.Where(a => a.CreationYear.HasValue && a.CreationYear.Value >= 1900 && a.CreationYear.Value <= 1949),
                    "1950plus" => query.Where(a => a.CreationYear.HasValue && a.CreationYear.Value >= 1950),
                    _ => query
                };
            }

            if (movementId.HasValue)
            {
                var artworkIds = await GetArtworkIdsByMovementAsync(movementId.Value);
                query = query.Where(a => artworkIds.Contains(a.ArtworkId));
            }

            var artists = await _context.Artists
                .OrderBy(a => a.Surname)
                .ThenBy(a => a.Name)
                .Select(a => new SelectListItem
                {
                    Value = a.ArtistId.ToString(),
                    Text = a.Name + " " + a.Surname
                })
                .ToListAsync();

            var themes = await _context.Artworks
                .Where(a => a.Theme != null && a.Theme != "")
                .Select(a => a.Theme!)
                .Distinct()
                .OrderBy(t => t)
                .Select(t => new SelectListItem
                {
                    Value = t,
                    Text = t
                })
                .ToListAsync();

            var movements = await _context.ArtMovements
                .OrderBy(m => m.MovementName)
                .Select(m => new SelectListItem
                {
                    Value = m.MovementId.ToString(),
                    Text = string.IsNullOrWhiteSpace(m.Era)
                        ? m.MovementName
                        : m.MovementName + " (" + m.Era + ")"
                })
                .ToListAsync();

            var techniques = await _context.Artworks
                .Where(a => a.Technique != null && a.Technique != "")
                .Select(a => a.Technique!)
                .Distinct()
                .OrderBy(x => x)
                .Select(x => new SelectListItem
                {
                    Value = x,
                    Text = x
                })
                .ToListAsync();

            var media = await _context.Artworks
                .Where(a => a.Medium != null && a.Medium != "")
                .Select(a => a.Medium!)
                .Distinct()
                .OrderBy(x => x)
                .Select(x => new SelectListItem
                {
                    Value = x,
                    Text = x
                })
                .ToListAsync();

            var vm = new ArtworkFilterViewModel
            {
                SearchTerm = searchTerm,
                ArtistId = artistId,
                Theme = theme,
                Period = period,
                MovementId = movementId,
                Technique = technique,
                Medium = medium,
                ArtistOptions = artists,
                ThemeOptions = themes,
                MovementOptions = movements,
                TechniqueOptions = techniques,
                MediumOptions = media,
                PeriodOptions = new List<SelectListItem>
                {
                    new SelectListItem { Value = "before1600", Text = "Before 1600" },
                    new SelectListItem { Value = "1600to1799", Text = "1600 - 1799" },
                    new SelectListItem { Value = "1800to1899", Text = "1800 - 1899" },
                    new SelectListItem { Value = "1900to1949", Text = "1900 - 1949" },
                    new SelectListItem { Value = "1950plus", Text = "1950 and later" }
                },
                Results = await query
                    .OrderBy(a => a.Title)
                    .ToListAsync()
            };

            return View(vm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var artwork = await _context.Artworks
                .Include(a => a.Artist)
                .Include(a => a.ArtworkImageUrls)
                .FirstOrDefaultAsync(a => a.ArtworkId == id);

            if (artwork == null)
            {
                return NotFound();
            }

            var movementIds = await GetArtworkMovementIdsAsync(id);

            var movements = await _context.ArtMovements
                .Where(m => movementIds.Contains(m.MovementId))
                .OrderBy(m => m.MovementName)
                .Select(m => string.IsNullOrWhiteSpace(m.Era)
                    ? m.MovementName
                    : m.MovementName + " (" + m.Era + ")")
                .ToListAsync();

            var vm = new ArtworkDetailsViewModel
            {
                ArtworkId = artwork.ArtworkId,
                Title = artwork.Title,
                CreationYear = artwork.CreationYear,
                ArtworkType = artwork.ArtworkType,
                Theme = artwork.Theme,
                Technique = artwork.Technique,
                Medium = artwork.Medium,
                Dimensions = artwork.Dimensions,
                AcquisitionDate = artwork.AcquisitionDate,
                ArtistId = artwork.ArtistId,
                ArtistFullName = artwork.Artist != null
                    ? artwork.Artist.Name + " " + artwork.Artist.Surname
                    : "Unknown",
                ArtistNationality = artwork.Artist?.Nationality,
                Movements = movements,
                ImageUrls = artwork.ArtworkImageUrls
                    .Select(x => x.ImageUrl)
                    .ToList()
            };

            return View(vm);
        }

        private async Task<List<int>> GetArtworkIdsByMovementAsync(int movementId)
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
SELECT [Artwork_ID]
FROM [BELONGS_TO]
WHERE [Movement_ID] = @MovementId;";

                var parameter = command.CreateParameter();
                parameter.ParameterName = "@MovementId";
                parameter.Value = movementId;
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
