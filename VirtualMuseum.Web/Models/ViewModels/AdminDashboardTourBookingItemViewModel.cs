namespace VirtualMuseum.Web.Models.ViewModels
{
    public class AdminDashboardTourBookingItemViewModel
    {
        public string VisitorName { get; set; } = string.Empty;
        public string TourTitle { get; set; } = string.Empty;
        public string ExhibitionTitle { get; set; } = string.Empty;
        public DateOnly TourDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
