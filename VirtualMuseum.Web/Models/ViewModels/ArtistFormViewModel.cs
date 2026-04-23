using System.ComponentModel.DataAnnotations;

namespace VirtualMuseum.Web.Models.ViewModels
{
    public class ArtistFormViewModel
    {
        public int ArtistId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Surname { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateOnly? BirthDate { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? DeathDate { get; set; }

        [StringLength(100)]
        public string? Nationality { get; set; }
    }
}
