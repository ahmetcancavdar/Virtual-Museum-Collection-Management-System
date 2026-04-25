namespace VirtualMuseum.Web.Models.ViewModels
{
    public class ArtMovementDeleteViewModel
    {
        public int MovementId { get; set; }
        public string MovementName { get; set; } = string.Empty;
        public string? RegionOfOrigin { get; set; }
        public string? Era { get; set; }
        public string? Description { get; set; }

        public int LinkedArtistCount { get; set; }
        public int LinkedArtworkCount { get; set; }
    }
}
