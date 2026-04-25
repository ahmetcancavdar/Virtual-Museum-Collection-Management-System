using Microsoft.AspNetCore.Mvc.Rendering;
using VirtualMuseum.Web.Models;

namespace VirtualMuseum.Web.Models.ViewModels
{
    public class ExhibitionFilterViewModel
    {
        public string? SearchTerm { get; set; }
        public string? Theme { get; set; }
        public string? RoomName { get; set; }
        public string? DateStatus { get; set; }

        public List<SelectListItem> ThemeOptions { get; set; } = new();
        public List<SelectListItem> RoomOptions { get; set; } = new();
        public List<SelectListItem> DateStatusOptions { get; set; } = new();

        public List<Exhibition> Results { get; set; } = new();
    }
}
