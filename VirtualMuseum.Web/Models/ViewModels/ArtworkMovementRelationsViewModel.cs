namespace VirtualMuseum.Web.Models.ViewModels
{
    public class ArtworkMovementRelationsViewModel
    {
        public int ArtworkId { get; set; }
        public string ArtworkTitle { get; set; } = string.Empty;
        public string ArtistFullName { get; set; } = string.Empty;

        public List<RelationCheckboxItemViewModel> Movements { get; set; } = new();
    }
}
