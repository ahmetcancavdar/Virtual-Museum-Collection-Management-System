namespace VirtualMuseum.Web.Models.ViewModels
{
    public class AdminTourBookingItemViewModel
    {
        public int BookingId { get; set; }
        public string VisitorName { get; set; } = string.Empty;
        public string TourTitle { get; set; } = string.Empty;
        public string ExhibitionTitle { get; set; } = string.Empty;
        public DateOnly TourDate { get; set; }
        public string TimeRange { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
