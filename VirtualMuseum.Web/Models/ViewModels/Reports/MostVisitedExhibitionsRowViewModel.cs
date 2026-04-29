namespace VirtualMuseum.Web.Models.ViewModels.Reports
{
    public class MostVisitedExhibitionsRowViewModel
    {
        public int ExhibitionId { get; set; }
        public string ExhibitionTitle { get; set; } = string.Empty;

        public int TotalVisits { get; set; }
        public int PlannedVisits { get; set; }
        public int CompletedVisits { get; set; }
        public int CancelledVisits { get; set; }
    }
}
