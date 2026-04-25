using System.ComponentModel.DataAnnotations;

namespace VirtualMuseum.Web.Models.ViewModels
{
    public class ExhibitionFormViewModel
    {
        public int ExhibitionId { get; set; }

        [Required]
        [StringLength(300)]
        public string Title { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Theme { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateOnly StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateOnly EndDate { get; set; }

        [Required]
        public int RoomId { get; set; }

        [Required]
        [StringLength(100)]
        public string RoomName { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}
