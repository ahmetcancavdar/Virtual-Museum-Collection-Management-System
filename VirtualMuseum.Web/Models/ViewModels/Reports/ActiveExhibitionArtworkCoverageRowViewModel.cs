namespace VirtualMuseum.Web.Models.ViewModels.Reports
{
    public class ActiveExhibitionArtworkCoverageRowViewModel
    {
        public int ExhibitionId { get; set; }
        public string ExhibitionTitle { get; set; } = string.Empty;
        public int RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public int ArtworkId { get; set; }
        public string ArtworkTitle { get; set; } = string.Empty;
        public string ArtworkType { get; set; } = string.Empty;
        public string Theme { get; set; } = string.Empty;
        public string ArtistName { get; set; } = string.Empty;
    }
}
