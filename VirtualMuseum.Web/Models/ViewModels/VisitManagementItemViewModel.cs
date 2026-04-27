namespace VirtualMuseum.Web.Models.ViewModels
{
    public class VisitManagementItemViewModel
    {
        public int UserId { get; set; }
        public string VisitorName { get; set; } = string.Empty;
        public int ExhibitionId { get; set; }
        public string ExhibitionTitle { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public DateOnly VisitDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
