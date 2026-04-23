using Microsoft.AspNetCore.Mvc.Rendering;
using VirtualMuseum.Web.Models;

namespace VirtualMuseum.Web.Models.ViewModels
{
    public class ArtistFilterViewModel
    {
        public string? SearchTerm { get; set; }
        public string? Nationality { get; set; }
        public int? MovementId { get; set; }

        public List<SelectListItem> NationalityOptions { get; set; } = new();
        public List<SelectListItem> MovementOptions { get; set; } = new();

        public List<Artist> Results { get; set; } = new();
    }
}
