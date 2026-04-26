namespace VirtualMuseum.Web.Models.ViewModels
{
    public class MyTourBookingItemViewModel
    {
        public int BookingId { get; set; }
        public int TourId { get; set; }
        public string TourTitle { get; set; } = string.Empty;
        public string ExhibitionTitle { get; set; } = string.Empty;
        public DateOnly TourDate { get; set; }
        public string TimeRange { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
