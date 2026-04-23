namespace VirtualMuseum.Web.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalArtists { get; set; }
        public int TotalArtworks { get; set; }
        public int TotalExhibitions { get; set; }
        public int TotalTours { get; set; }

        public int TodayVisitCount { get; set; }
        public int PlannedVisitCount { get; set; }
        public int CompletedVisitCount { get; set; }
        public int CancelledVisitCount { get; set; }

        public int OpenTourCount { get; set; }
        public int UpcomingTourCount { get; set; }
        public int TotalTourBookingCount { get; set; }

        public List<AdminDashboardVisitItemViewModel> RecentVisits { get; set; } = new();
        public List<AdminDashboardTourBookingItemViewModel> RecentTourBookings { get; set; } = new();
    }
}
