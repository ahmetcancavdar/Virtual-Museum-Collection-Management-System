using System.ComponentModel.DataAnnotations;

namespace VirtualMuseum.Web.Models.ViewModels
{
    public class ArtMovementFormViewModel
    {
        public int MovementId { get; set; }

        [Required]
        [StringLength(150)]
        public string MovementName { get; set; } = string.Empty;

        [StringLength(150)]
        public string? RegionOfOrigin { get; set; }

        [StringLength(100)]
        public string? Era { get; set; }

        public string? Description { get; set; }
    }
}
