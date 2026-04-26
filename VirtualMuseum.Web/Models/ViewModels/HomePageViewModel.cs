namespace VirtualMuseum.Web.Models.ViewModels
{
    public class HomePageViewModel
    {
        public int TotalArtists { get; set; }
        public int TotalArtworks { get; set; }
        public int TotalExhibitions { get; set; }
        public int TotalTours { get; set; }

        public List<HomeArtworkCardViewModel> FeaturedArtworks { get; set; } = new();
        public List<HomeArtistCardViewModel> FeaturedArtists { get; set; } = new();
        public List<HomeExhibitionCardViewModel> ActiveExhibitions { get; set; } = new();
        public List<HomeTourCardViewModel> UpcomingTours { get; set; } = new();
    }
}
