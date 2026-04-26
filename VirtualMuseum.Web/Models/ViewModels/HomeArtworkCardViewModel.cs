namespace VirtualMuseum.Web.Models.ViewModels
{
    public class HomeArtworkCardViewModel
    {
        public int ArtworkId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ArtistName { get; set; } = string.Empty;
        public string? Theme { get; set; }
        public string? ImageUrl { get; set; }
    }
}
