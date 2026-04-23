namespace VirtualMuseum.Web.Models.ViewModels
{
    public class ArtistArtworkItemViewModel
    {
        public int ArtworkId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int? CreationYear { get; set; }
        public string? Theme { get; set; }
        public string? ArtworkType { get; set; }
    }
}
