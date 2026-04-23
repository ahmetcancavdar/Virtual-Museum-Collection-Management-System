namespace VirtualMuseum.Web.Models.ViewModels
{
    public class ArtistDeleteViewModel
    {
        public int ArtistId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public DateOnly? BirthDate { get; set; }
        public DateOnly? DeathDate { get; set; }
        public string? Nationality { get; set; }

        public int LinkedArtworkCount { get; set; }
        public int LinkedMovementCount { get; set; }
    }
}
