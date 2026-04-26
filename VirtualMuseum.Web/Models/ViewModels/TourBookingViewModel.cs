using System.ComponentModel.DataAnnotations;

namespace VirtualMuseum.Web.Models.ViewModels
{
    public class TourBookingViewModel
    {
        public int TourId { get; set; }
        public string TourTitle { get; set; } = string.Empty;
        public string ExhibitionTitle { get; set; } = string.Empty;
        public DateOnly TourDate { get; set; }
        public string TimeRange { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string GuideName { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int BookedCount { get; set; }
        public int AvailableSeats { get; set; }
        public string Status { get; set; } = string.Empty;

        [Required]
        public bool ConfirmBooking { get; set; }
    }
}
