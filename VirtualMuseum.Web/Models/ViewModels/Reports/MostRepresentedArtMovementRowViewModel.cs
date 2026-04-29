namespace VirtualMuseum.Web.Models.ViewModels.Reports
{
    public class MostRepresentedArtMovementRowViewModel
    {
        public int MovementId { get; set; }
        public string MovementName { get; set; } = string.Empty;
        public string Era { get; set; } = string.Empty;
        public int ArtworkCount { get; set; }
    }
}
