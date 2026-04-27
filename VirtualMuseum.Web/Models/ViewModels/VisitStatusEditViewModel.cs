using System.ComponentModel.DataAnnotations;

namespace VirtualMuseum.Web.Models.ViewModels
{
    public class VisitStatusEditViewModel
    {
        public int UserId { get; set; }
        public int ExhibitionId { get; set; }

        [DataType(DataType.Date)]
        public DateOnly VisitDate { get; set; }

        public string VisitorName { get; set; } = string.Empty;
        public string ExhibitionTitle { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "Planned";
    }
}
