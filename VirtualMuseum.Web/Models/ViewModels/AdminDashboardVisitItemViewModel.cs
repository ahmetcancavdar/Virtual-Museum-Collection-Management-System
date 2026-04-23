namespace VirtualMuseum.Web.Models.ViewModels
{
    public class AdminDashboardVisitItemViewModel
    {
        public string VisitorName { get; set; } = string.Empty;
        public string ExhibitionTitle { get; set; } = string.Empty;
        public DateOnly VisitDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
