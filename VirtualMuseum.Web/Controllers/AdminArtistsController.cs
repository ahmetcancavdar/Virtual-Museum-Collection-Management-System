using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VirtualMuseum.Web.Data;
using VirtualMuseum.Web.Models;
using VirtualMuseum.Web.Models.ViewModels;


namespace VirtualMuseum.Web.Controllers

{
    [Authorize(Roles = "Admin")]
    public class AdminArtistsController : Controller
    {
        private readonly VirtualMuseumDbContext _context;

        public AdminArtistsController(VirtualMuseumDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var artists = await _context.Artists
                .OrderBy(a => a.Surname)
                .ThenBy(a => a.Name)
                .ToListAsync();

            return View(artists);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new ArtistFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ArtistFormViewModel model)
        {
            model.Name = model.Name.Trim();
            model.Surname = model.Surname.Trim();
            model.Nationality = string.IsNullOrWhiteSpace(model.Nationality) ? null : model.Nationality.Trim();

            var duplicateArtist = await _context.Artists.AnyAsync(a =>
                a.Name == model.Name &&
                a.Surname == model.Surname &&
                a.BirthDate == model.BirthDate);

            if (duplicateArtist)
            {
                ModelState.AddModelError("", "An artist with the same name and birth date already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var artist = new Artist
            {
                Name = model.Name,
                Surname = model.Surname,
                BirthDate = model.BirthDate,
                DeathDate = model.DeathDate,
                Nationality = model.Nationality
            };

            _context.Artists.Add(artist);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Artist created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var artist = await _context.Artists.FindAsync(id);

            if (artist == null)
            {
                return NotFound();
            }

            var vm = new ArtistFormViewModel
            {
                ArtistId = artist.ArtistId,
                Name = artist.Name,
                Surname = artist.Surname,
                BirthDate = artist.BirthDate,
                DeathDate = artist.DeathDate,
                Nationality = artist.Nationality
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ArtistFormViewModel model)
        {
            if (id != model.ArtistId)
            {
                return BadRequest();
            }

            model.Name = model.Name.Trim();
            model.Surname = model.Surname.Trim();
            model.Nationality = string.IsNullOrWhiteSpace(model.Nationality) ? null : model.Nationality.Trim();

            var duplicateArtist = await _context.Artists.AnyAsync(a =>
                a.ArtistId != model.ArtistId &&
                a.Name == model.Name &&
                a.Surname == model.Surname &&
                a.BirthDate == model.BirthDate);

            if (duplicateArtist)
            {
                ModelState.AddModelError("", "Another artist with the same name and birth date already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var artist = await _context.Artists.FindAsync(id);

            if (artist == null)
            {
                return NotFound();
            }

            artist.Name = model.Name;
            artist.Surname = model.Surname;
            artist.BirthDate = model.BirthDate;
            artist.DeathDate = model.DeathDate;
            artist.Nationality = model.Nationality;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Artist updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var artist = await _context.Artists.FindAsync(id);

            if (artist == null)
            {
                return NotFound();
            }

            var linkedArtworkCount = await _context.Artworks.CountAsync(a => a.ArtistId == id);
            var linkedMovementCount = (await GetSelectedMovementIdsAsync(id)).Count;

            var vm = new ArtistDeleteViewModel
            {
                ArtistId = artist.ArtistId,
                Name = artist.Name,
                Surname = artist.Surname,
                BirthDate = artist.BirthDate,
                DeathDate = artist.DeathDate,
                Nationality = artist.Nationality,
                LinkedArtworkCount = linkedArtworkCount,
                LinkedMovementCount = linkedMovementCount
            };

            return View(vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var artist = await _context.Artists.FindAsync(id);

            if (artist == null)
            {
                return NotFound();
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await _context.Database.ExecuteSqlInterpolatedAsync($@"
DELETE FROM [TAGGED_WITH]
WHERE [Artwork_ID] IN (
    SELECT [Artwork_ID]
    FROM [ARTWORK]
    WHERE [Artist_ID] = {id}
);");

                await _context.Database.ExecuteSqlInterpolatedAsync($@"
DELETE FROM [BELONGS_TO]
WHERE [Artwork_ID] IN (
    SELECT [Artwork_ID]
    FROM [ARTWORK]
    WHERE [Artist_ID] = {id}
);");

                await _context.Database.ExecuteSqlInterpolatedAsync($@"
DELETE FROM [FEATURES]
WHERE [Artwork_ID] IN (
    SELECT [Artwork_ID]
    FROM [ARTWORK]
    WHERE [Artist_ID] = {id}
);");

                await _context.Database.ExecuteSqlInterpolatedAsync($@"
DELETE FROM [ARTWORK_IMAGE_URL]
WHERE [Artwork_ID] IN (
    SELECT [Artwork_ID]
    FROM [ARTWORK]
    WHERE [Artist_ID] = {id}
);");

                await _context.Database.ExecuteSqlInterpolatedAsync($@"
DELETE FROM [INFLUENCED_BY]
WHERE [Artist_ID] = {id};");

                await _context.Database.ExecuteSqlInterpolatedAsync($@"
DELETE FROM [ARTWORK]
WHERE [Artist_ID] = {id};");

                _context.Artists.Remove(artist);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "Artist and related records were deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "Delete failed: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> ManageMovements(int id)
        {
            var artist = await _context.Artists
                .FirstOrDefaultAsync(a => a.ArtistId == id);

            if (artist == null)
            {
                return NotFound();
            }

            var selectedMovementIds = await GetSelectedMovementIdsAsync(id);

            var allMovements = await _context.ArtMovements
                .OrderBy(m => m.MovementName)
                .ToListAsync();

            var vm = new ArtistMovementRelationsViewModel
            {
                ArtistId = artist.ArtistId,
                ArtistFullName = artist.Name + " " + artist.Surname,
                Nationality = artist.Nationality,
                Movements = allMovements.Select(m => new RelationCheckboxItemViewModel
                {
                    Id = m.MovementId,
                    Label = string.IsNullOrWhiteSpace(m.Era)
                        ? m.MovementName
                        : $"{m.MovementName} ({m.Era})",
                    Selected = selectedMovementIds.Contains(m.MovementId)
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageMovements(ArtistMovementRelationsViewModel model)
        {
            var artist = await _context.Artists
                .FirstOrDefaultAsync(a => a.ArtistId == model.ArtistId);

            if (artist == null)
            {
                return NotFound();
            }

            var selectedMovementIds = (model.Movements ?? new List<RelationCheckboxItemViewModel>())
                .Where(x => x.Selected)
                .Select(x => x.Id)
                .Distinct()
                .ToList();

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await _context.Database.ExecuteSqlInterpolatedAsync($@"
DELETE FROM [INFLUENCED_BY]
WHERE [Artist_ID] = {model.ArtistId};");

                foreach (var movementId in selectedMovementIds)
                {
                    await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO [INFLUENCED_BY] ([Artist_ID], [Movement_ID])
VALUES ({model.ArtistId}, {movementId});");
                }

                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "Artist movements updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "Movement update failed: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task<List<int>> GetSelectedMovementIdsAsync(int artistId)
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
