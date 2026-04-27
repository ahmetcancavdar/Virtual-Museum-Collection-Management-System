using System.ComponentModel.DataAnnotations;

namespace VirtualMuseum.Web.Models.ViewModels
{
    public class VirtualTourStopFormViewModel
    {
        public int StopId { get; set; }
        public int PlanId { get; set; }

        public string PlanTitle { get; set; } = string.Empty;
        public string ExhibitionTitle { get; set; } = string.Empty;

        [Range(1, 500)]
        public int StepNo { get; set; }

        [Range(1, 10000)]
        public int RoomId { get; set; }

        [Required]
        [StringLength(100)]
        public string RoomName { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string StopTitle { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Notes { get; set; }

        [Range(1, 300)]
        public int? EstimatedMinutes { get; set; }
    }
}
