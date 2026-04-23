namespace VirtualMuseum.Web.Models.ViewModels
{
    public class ArtistDetailsViewModel
    {
        public int ArtistId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateOnly? BirthDate { get; set; }
        public DateOnly? DeathDate { get; set; }
        public string? Nationality { get; set; }

        public List<ArtistArtworkItemViewModel> Artworks { get; set; } = new();
        public List<string> Movements { get; set; } = new();
    }
}
