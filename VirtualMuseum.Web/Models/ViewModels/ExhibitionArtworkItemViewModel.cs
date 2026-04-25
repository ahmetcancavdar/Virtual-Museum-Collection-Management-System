namespace VirtualMuseum.Web.Models.ViewModels
{
    public class ExhibitionArtworkItemViewModel
    {
        public int ArtworkId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ArtistFullName { get; set; } = string.Empty;
        public List<string> Movements { get; set; } = new();
    }
}
