namespace VirtualMuseum.Web.Models.ViewModels.Reports
{
    public class MostPopularToursRowViewModel
    {
        public int TourId { get; set; }
        public string TourTitle { get; set; } = string.Empty;
        public string ExhibitionTitle { get; set; } = string.Empty;
        public DateTime TourDate { get; set; }
        public int Capacity { get; set; }

        public int TotalBookings { get; set; }
        public int ActiveBookings { get; set; }
        public int CancelledBookings { get; set; }
        public int RemainingCapacity { get; set; }
    }
}
