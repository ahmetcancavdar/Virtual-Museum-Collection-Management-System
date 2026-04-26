namespace VirtualMuseum.Web.Models.ViewModels
{
    public class HomeArtistCardViewModel
    {
        public int ArtistId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Nationality { get; set; }
        public int ArtworkCount { get; set; }
    }
}
