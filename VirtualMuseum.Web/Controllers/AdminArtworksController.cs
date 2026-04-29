using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VirtualMuseum.Web.Data;
using VirtualMuseum.Web.Models;
using VirtualMuseum.Web.Models.ViewModels;

namespace VirtualMuseum.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminArtworksController : Controller
    {
        private readonly VirtualMuseumDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AdminArtworksController(VirtualMuseumDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var artworks = await _context.Artworks
                .Include(a => a.Artist)
                .OrderBy(a => a.Title)
                .ToListAsync();

            return View(artworks);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateArtistsDropDownListAsync();
            return View(new ArtworkFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ArtworkFormViewModel model)
        {
            model.Title = model.Title.Trim();
            model.ArtworkType = string.IsNullOrWhiteSpace(model.ArtworkType) ? null : model.ArtworkType.Trim();
            model.Theme = string.IsNullOrWhiteSpace(model.Theme) ? null : model.Theme.Trim();
            model.Technique = string.IsNullOrWhiteSpace(model.Technique) ? null : model.Technique.Trim();
            model.Medium = string.IsNullOrWhiteSpace(model.Medium) ? null : model.Medium.Trim();
            model.Dimensions = string.IsNullOrWhiteSpace(model.Dimensions) ? null : model.Dimensions.Trim();

            if (!await _context.Artists.AnyAsync(a => a.ArtistId == model.ArtistId))
            {
                ModelState.AddModelError(nameof(model.ArtistId), "Selected artist was not found.");
            }

            var duplicateArtwork = await _context.Artworks.AnyAsync(a =>
                a.ArtistId == model.ArtistId &&
                a.Title == model.Title &&
                a.CreationYear == model.CreationYear);

            if (duplicateArtwork)
            {
                ModelState.AddModelError("", "An artwork with the same title, artist, and year already exists.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateArtistsDropDownListAsync(model.ArtistId);
                return View(model);
            }

            var artwork = new Artwork
            {
                Title = model.Title,
                CreationYear = model.CreationYear,
                ArtworkType = model.ArtworkType,
                Theme = model.Theme,
                Technique = model.Technique,
                Medium = model.Medium,
                Dimensions = model.Dimensions,
                AcquisitionDate = model.AcquisitionDate,
                ArtistId = model.ArtistId
            };

            _context.Artworks.Add(artwork);
            await _context.SaveChangesAsync();

            var manualPaths = ParseImagePaths(model.ImagePathsText);
            var uploadedPaths = await SaveUploadedImagesAsync(artwork.ArtworkId, model.UploadedImages);

            var finalPaths = manualPaths
                .Concat(uploadedPaths)
                .Distinct()
                .ToList();

            if (finalPaths.Any())
            {
                var imageRows = finalPaths.Select(path => new ArtworkImageUrl
                {
                    ArtworkId = artwork.ArtworkId,
                    ImageUrl = path
                });

                _context.ArtworkImageUrls.AddRange(imageRows);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Artwork created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var artwork = await _context.Artworks
                .Include(a => a.ArtworkImageUrls)
                .FirstOrDefaultAsync(a => a.ArtworkId == id);

            if (artwork == null)
            {
                return NotFound();
            }

            var vm = new ArtworkFormViewModel
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
                ImagePathsText = string.Join(Environment.NewLine, artwork.ArtworkImageUrls.Select(i => i.ImageUrl))
            };

            await PopulateArtistsDropDownListAsync(vm.ArtistId);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ArtworkFormViewModel model)
        {
            if (id != model.ArtworkId)
            {
                return BadRequest();
            }

            model.Title = model.Title.Trim();
            model.ArtworkType = string.IsNullOrWhiteSpace(model.ArtworkType) ? null : model.ArtworkType.Trim();
            model.Theme = string.IsNullOrWhiteSpace(model.Theme) ? null : model.Theme.Trim();
            model.Technique = string.IsNullOrWhiteSpace(model.Technique) ? null : model.Technique.Trim();
            model.Medium = string.IsNullOrWhiteSpace(model.Medium) ? null : model.Medium.Trim();
            model.Dimensions = string.IsNullOrWhiteSpace(model.Dimensions) ? null : model.Dimensions.Trim();

            if (!await _context.Artists.AnyAsync(a => a.ArtistId == model.ArtistId))
            {
                ModelState.AddModelError(nameof(model.ArtistId), "Selected artist was not found.");
            }

            var duplicateArtwork = await _context.Artworks.AnyAsync(a =>
                a.ArtworkId != model.ArtworkId &&
                a.ArtistId == model.ArtistId &&
                a.Title == model.Title &&
                a.CreationYear == model.CreationYear);

            if (duplicateArtwork)
            {
                ModelState.AddModelError("", "Another artwork with the same title, artist, and year already exists.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateArtistsDropDownListAsync(model.ArtistId);
                return View(model);
            }

            var artwork = await _context.Artworks
                .FirstOrDefaultAsync(a => a.ArtworkId == id);

            if (artwork == null)
            {
                return NotFound();
            }

            artwork.Title = model.Title;
            artwork.CreationYear = model.CreationYear;
            artwork.ArtworkType = model.ArtworkType;
            artwork.Theme = model.Theme;
            artwork.Technique = model.Technique;
            artwork.Medium = model.Medium;
            artwork.Dimensions = model.Dimensions;
            artwork.AcquisitionDate = model.AcquisitionDate;
            artwork.ArtistId = model.ArtistId;

            var existingImages = await _context.ArtworkImageUrls
                .Where(i => i.ArtworkId == id)
                .ToListAsync();

            _context.ArtworkImageUrls.RemoveRange(existingImages);

            var manualPaths = ParseImagePaths(model.ImagePathsText);
            var uploadedPaths = await SaveUploadedImagesAsync(id, model.UploadedImages);

            var finalPaths = manualPaths
                .Concat(uploadedPaths)
                .Distinct()
                .ToList();

            if (finalPaths.Any())
            {
                var newImageRows = finalPaths.Select(path => new ArtworkImageUrl
                {
                    ArtworkId = id,
                    ImageUrl = path
                });

                _context.ArtworkImageUrls.AddRange(newImageRows);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Artwork updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var artwork = await _context.Artworks
                .Include(a => a.Artist)
                .FirstOrDefaultAsync(a => a.ArtworkId == id);

            if (artwork == null)
            {
                return NotFound();
            }

            var vm = new ArtworkDeleteViewModel
            {
                ArtworkId = artwork.ArtworkId,
                Title = artwork.Title,
                ArtistFullName = artwork.Artist != null
                    ? artwork.Artist.Name + " " + artwork.Artist.Surname
                    : "Unknown",
                ImageCount = await _context.ArtworkImageUrls.CountAsync(i => i.ArtworkId == id)
            };

            return View(vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var artwork = await _context.Artworks.FindAsync(id);

            if (artwork == null)
            {
                return NotFound();
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await _context.Database.ExecuteSqlInterpolatedAsync($@"
DELETE FROM [TAGGED_WITH]
WHERE [Artwork_ID] = {id};");

                await _context.Database.ExecuteSqlInterpolatedAsync($@"
DELETE FROM [BELONGS_TO]
WHERE [Artwork_ID] = {id};");

                await _context.Database.ExecuteSqlInterpolatedAsync($@"
DELETE FROM [FEATURES]
WHERE [Artwork_ID] = {id};");

                await _context.Database.ExecuteSqlInterpolatedAsync($@"
DELETE FROM [ARTWORK_IMAGE_URL]
WHERE [Artwork_ID] = {id};");

                _context.Artworks.Remove(artwork);
                await _context.SaveChangesAsync();

                var artworkFolder = Path.Combine(_environment.WebRootPath, "uploads", "artworks", id.ToString());
                if (Directory.Exists(artworkFolder))
                {
                    Directory.Delete(artworkFolder, true);
                }

                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "Artwork and related records were deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "Delete operation failed: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> ManageMovements(int id)
        {
            var artwork = await _context.Artworks
                .Include(a => a.Artist)
                .FirstOrDefaultAsync(a => a.ArtworkId == id);

            if (artwork == null)
            {
                return NotFound();
            }

            var selectedMovementIds = await GetSelectedArtworkMovementIdsAsync(id);

            var allMovements = await _context.ArtMovements
                .OrderBy(m => m.MovementName)
                .ToListAsync();

            var vm = new ArtworkMovementRelationsViewModel
            {
                ArtworkId = artwork.ArtworkId,
                ArtworkTitle = artwork.Title,
                ArtistFullName = artwork.Artist != null
                    ? artwork.Artist.Name + " " + artwork.Artist.Surname
                    : "Unknown",
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
        public async Task<IActionResult> ManageMovements(ArtworkMovementRelationsViewModel model)
        {
            var artwork = await _context.Artworks
                .FirstOrDefaultAsync(a => a.ArtworkId == model.ArtworkId);

            if (artwork == null)
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
DELETE FROM [BELONGS_TO]
WHERE [Artwork_ID] = {model.ArtworkId};");

                foreach (var movementId in selectedMovementIds)
                {
                    await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO [BELONGS_TO] ([Artwork_ID], [Movement_ID])
VALUES ({model.ArtworkId}, {movementId});");
                }

                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "Artwork movements updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "Movement update failed: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task<List<int>> GetSelectedArtworkMovementIdsAsync(int artworkId)
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

        private async Task PopulateArtistsDropDownListAsync(int? selectedArtistId = null)
        {
            var artists = await _context.Artists
                .OrderBy(a => a.Surname)
                .ThenBy(a => a.Name)
                .Select(a => new
                {
                    a.ArtistId,
                    FullName = a.Name + " " + a.Surname
                })
                .ToListAsync();

            ViewBag.Artists = new SelectList(artists, "ArtistId", "FullName", selectedArtistId);
        }

        private static List<string> ParseImagePaths(string? imagePathsText)
        {
            if (string.IsNullOrWhiteSpace(imagePathsText))
            {
                return new List<string>();
            }

            return imagePathsText
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();
        }

        private async Task<List<string>> SaveUploadedImagesAsync(int artworkId, List<IFormFile>? uploadedImages)
        {
            var savedPaths = new List<string>();

            if (uploadedImages == null || !uploadedImages.Any(f => f.Length > 0))
            {
                return savedPaths;
            }

            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg", ".jpeg", ".png", ".webp"
            };

            var artworkFolder = Path.Combine(_environment.WebRootPath, "uploads", "artworks", artworkId.ToString());
            Directory.CreateDirectory(artworkFolder);

            var existingMain = Directory.GetFiles(artworkFolder, "main.*").Any();
            var existingDetailCount = Directory.GetFiles(artworkFolder, "detail*.*").Length;
            var nextDetailIndex = existingDetailCount + 1;

            foreach (var file in uploadedImages.Where(f => f.Length > 0))
            {
                var extension = Path.GetExtension(file.FileName);

                if (!allowedExtensions.Contains(extension))
                {
                    continue;
                }

                string fileName;

                if (!existingMain)
                {
                    fileName = "main" + extension.ToLowerInvariant();
                    existingMain = true;
                }
                else
                {
                    fileName = $"detail{nextDetailIndex}{extension.ToLowerInvariant()}";
                    nextDetailIndex++;
                }

                var filePath = Path.Combine(artworkFolder, fileName);

                await using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                savedPaths.Add($"/uploads/artworks/{artworkId}/{fileName}");
            }

            return savedPaths;
        }
    }
}
