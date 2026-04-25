namespace VirtualMuseum.Web.Models.ViewModels
{
    public class ArtworkDeleteViewModel
    {
        public int ArtworkId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ArtistFullName { get; set; } = string.Empty;

        public int ImageCount { get; set; }
    }
}
