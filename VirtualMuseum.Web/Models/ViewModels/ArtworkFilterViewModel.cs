using Microsoft.AspNetCore.Mvc.Rendering;
using VirtualMuseum.Web.Models;

namespace VirtualMuseum.Web.Models.ViewModels
{
    public class ArtworkFilterViewModel
    {
        public string? SearchTerm { get; set; }
        public int? ArtistId { get; set; }
        public string? Theme { get; set; }
        public string? Period { get; set; }
        public int? MovementId { get; set; }

        public string? Technique { get; set; }
        public string? Medium { get; set; }

        public List<SelectListItem> ArtistOptions { get; set; } = new();
        public List<SelectListItem> ThemeOptions { get; set; } = new();
        public List<SelectListItem> PeriodOptions { get; set; } = new();
        public List<SelectListItem> MovementOptions { get; set; } = new();
        public List<SelectListItem> TechniqueOptions { get; set; } = new();
        public List<SelectListItem> MediumOptions { get; set; } = new();

        public List<Artwork> Results { get; set; } = new();
    }
}
