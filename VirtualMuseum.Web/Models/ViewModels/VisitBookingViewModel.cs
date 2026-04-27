using System.ComponentModel.DataAnnotations;

namespace VirtualMuseum.Web.Models.ViewModels
{
    public class VisitBookingViewModel
    {
        public int ExhibitionId { get; set; }
        public string ExhibitionTitle { get; set; } = string.Empty;
        public string? Theme { get; set; }
        public string RoomName { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateOnly StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateOnly EndDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateOnly VisitDate { get; set; }
    }
}
