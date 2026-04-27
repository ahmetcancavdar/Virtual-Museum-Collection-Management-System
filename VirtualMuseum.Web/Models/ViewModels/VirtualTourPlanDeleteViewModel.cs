namespace VirtualMuseum.Web.Models.ViewModels
{
    public class VirtualTourPlanDeleteViewModel
    {
        public int PlanId { get; set; }
        public string ExhibitionTitle { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int EstimatedDurationMinutes { get; set; }
        public bool IsActive { get; set; }
        public int StopCount { get; set; }
    }
}
