namespace VirtualMuseum.Web.Models.ViewModels
{
    public class ArtworkDetailsViewModel
    {
        public int ArtworkId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int? CreationYear { get; set; }
        public string? ArtworkType { get; set; }
        public string? Theme { get; set; }
        public string? Technique { get; set; }
        public string? Medium { get; set; }
        public string? Dimensions { get; set; }
        public DateOnly? AcquisitionDate { get; set; }
        public int? ArtistId { get; set; }
        public string ArtistFullName { get; set; } = string.Empty;
        public string? ArtistNationality { get; set; }

        public List<string> Movements { get; set; } = new();
        public List<string> ImageUrls { get; set; } = new();
    }
}   
