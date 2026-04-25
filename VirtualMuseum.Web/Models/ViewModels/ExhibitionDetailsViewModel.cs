namespace VirtualMuseum.Web.Models.ViewModels
{
    public class ExhibitionDetailsViewModel
    {
        public int ExhibitionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Theme { get; set; }
        public int RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        public string? StartDateText { get; set; }
        public string? EndDateText { get; set; }

        public List<ExhibitionArtworkItemViewModel> Artworks { get; set; } = new();
        public List<string> Curators { get; set; } = new();
        public List<TourPublicItemViewModel> Tours { get; set; } = new();

        public string? VirtualTourPlanTitle { get; set; }
        public string? VirtualTourPlanDescription { get; set; }
        public int? VirtualTourEstimatedMinutes { get; set; }
        public List<VirtualTourStopPublicItemViewModel> VirtualTourStops { get; set; } = new();
    }
}
