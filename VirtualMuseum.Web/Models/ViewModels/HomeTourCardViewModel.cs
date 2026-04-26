namespace VirtualMuseum.Web.Models.ViewModels
{
    public class HomeTourCardViewModel
    {
        public int TourId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ExhibitionTitle { get; set; } = string.Empty;
        public string DateText { get; set; } = string.Empty;
        public string TimeRange { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public int AvailableSeats { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
