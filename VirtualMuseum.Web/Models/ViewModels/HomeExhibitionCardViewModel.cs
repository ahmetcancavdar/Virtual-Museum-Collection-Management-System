namespace VirtualMuseum.Web.Models.ViewModels
{
    public class HomeExhibitionCardViewModel
    {
        public int ExhibitionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Theme { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public string DateRangeText { get; set; } = string.Empty;
        public int ArtworkCount { get; set; }
    }
}
