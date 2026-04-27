using System.ComponentModel.DataAnnotations;

namespace VirtualMuseum.Web.Models.ViewModels
{
    public class VirtualTourPlanFormViewModel
    {
        public int PlanId { get; set; }

        [Required]
        public int ExhibitionId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(1, 500)]
        public int EstimatedDurationMinutes { get; set; }

        public bool IsActive { get; set; }
    }
}
