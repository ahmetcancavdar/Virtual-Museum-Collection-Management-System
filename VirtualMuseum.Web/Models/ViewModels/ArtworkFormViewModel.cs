using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace VirtualMuseum.Web.Models.ViewModels
{
    public class ArtworkFormViewModel
    {
        public int ArtworkId { get; set; }

        [Required]
        [StringLength(300)]
        public string Title { get; set; } = string.Empty;

        public int? CreationYear { get; set; }

        [StringLength(100)]
        public string? ArtworkType { get; set; }

        [StringLength(200)]
        public string? Theme { get; set; }

        [StringLength(200)]
        public string? Technique { get; set; }

        [StringLength(200)]
        public string? Medium { get; set; }

        [StringLength(100)]
        public string? Dimensions { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? AcquisitionDate { get; set; }

        [Required]
        public int ArtistId { get; set; }

        public string? ImagePathsText { get; set; }

        public List<IFormFile>? UploadedImages { get; set; }
    }
}
