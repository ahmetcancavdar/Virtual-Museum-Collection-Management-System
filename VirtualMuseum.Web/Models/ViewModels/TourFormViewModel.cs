using System.ComponentModel.DataAnnotations;

namespace VirtualMuseum.Web.Models.ViewModels
{
    public class TourFormViewModel
    {
        public int TourId { get; set; }

        [Required]
        public int ExhibitionId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateOnly TourDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeOnly StartTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeOnly EndTime { get; set; }

        [Range(1, 500)]
        public int Capacity { get; set; }

        [Required]
        [StringLength(50)]
        public string Language { get; set; } = "English";

        [Required]
        public int GuideUserId { get; set; }

        [Required]
        public string Status { get; set; } = "Open";
    }
}
