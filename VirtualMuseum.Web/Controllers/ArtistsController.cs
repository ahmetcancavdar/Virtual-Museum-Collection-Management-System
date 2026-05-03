using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VirtualMuseum.Web.Data;
using VirtualMuseum.Web.Models.ViewModels;

namespace VirtualMuseum.Web.Controllers
{
    public class ArtistsController : Controller
    {
        private readonly VirtualMuseumDbContext _context;

        public ArtistsController(VirtualMuseumDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchTerm, string? nationality, int? movementId)
        {
            var query = _context.Artists.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(a =>
                    a.Name.Contains(term) ||
                    a.Surname.Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(nationality))
            {
                query = query.Where(a => a.Nationality == nationality);
            }

            if (movementId.HasValue)
            {
                var artistIds = await GetArtistIdsByMovementAsync(movementId.Value);
                query = query.Where(a => artistIds.Contains(a.ArtistId));
            }

            var nationalityOptions = await _context.Artists
                .Where(a => a.Nationality != null && a.Nationality != "")
                .Select(a => a.Nationality!)
                .Distinct()
                .OrderBy(n => n)
                .Select(n => new SelectListItem
                {
                    Value = n,
                    Text = n
                })
                .ToListAsync();

            var movementOptions = await _context.ArtMovements
                .OrderBy(m => m.MovementName)
                .Select(m => new SelectListItem
                {
                    Value = m.MovementId.ToString(),
                    Text = string.IsNullOrWhiteSpace(m.Era)
                        ? m.MovementName
                        : m.MovementName + " (" + m.Era + ")"
                })
                .ToListAsync();

            var vm = new ArtistFilterViewModel
            {
                SearchTerm = searchTerm,
                Nationality = nationality,
                MovementId = movementId,
                NationalityOptions = nationalityOptions,
                MovementOptions = movementOptions,
                Results = await query
                    .OrderBy(a => a.Surname)
                    .ThenBy(a => a.Name)
                    .ToListAsync()
            };

            return View(vm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var artist = await _context.Artists
                .FirstOrDefaultAsync(a => a.ArtistId == id);

            if (artist == null)
            {
                return NotFound();
            }

            var artworks = await _context.Artworks
                .Where(a => a.ArtistId == id)
                .OrderBy(a => a.CreationYear)
                .ThenBy(a => a.Title)
                .Select(a => new ArtistArtworkItemViewModel
                {
                    ArtworkId = a.ArtworkId,
                    Title = a.Title,
                    CreationYear = a.CreationYear,
                    Theme = a.Theme,
                    ArtworkType = a.ArtworkType
                })
                .ToListAsync();

            var movementIds = await GetArtistMovementIdsAsync(id);

            var movements = await _context.ArtMovements
                .Where(m => movementIds.Contains(m.MovementId))
                .OrderBy(m => m.MovementName)
                .Select(m => string.IsNullOrWhiteSpace(m.Era)
                    ? m.MovementName
                    : m.MovementName + " (" + m.Era + ")")
                .ToListAsync();

            var vm = new ArtistDetailsViewModel
            {
                ArtistId = artist.ArtistId,
                FullName = artist.Name + " " + artist.Surname,
                BirthDate = artist.BirthDate,
                DeathDate = artist.DeathDate,
                Nationality = artist.Nationality,
                Movements = movements,
                Artworks = artworks
            };

            return View(vm);
        }

        private async Task<List<int>> GetArtistIdsByMovementAsync(int movementId)
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
SELECT [Artist_ID]
FROM [INFLUENCED_BY]
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

        private async Task<List<int>> GetArtistMovementIdsAsync(int artistId)
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
FROM [INFLUENCED_BY]
WHERE [Artist_ID] = @ArtistId;";

                var parameter = command.CreateParameter();
                parameter.ParameterName = "@ArtistId";
                parameter.Value = artistId;
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
