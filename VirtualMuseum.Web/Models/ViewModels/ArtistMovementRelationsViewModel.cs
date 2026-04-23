namespace VirtualMuseum.Web.Models.ViewModels
{
    public class ArtistMovementRelationsViewModel
    {
        public int ArtistId { get; set; }
        public string ArtistFullName { get; set; } = string.Empty;
        public string? Nationality { get; set; }

        public List<RelationCheckboxItemViewModel> Movements { get; set; } = new();
    }
}
