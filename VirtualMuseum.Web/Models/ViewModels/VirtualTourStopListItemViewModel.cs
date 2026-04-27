namespace VirtualMuseum.Web.Models.ViewModels
{
    public class VirtualTourStopListItemViewModel
    {
        public int StopId { get; set; }
        public int PlanId { get; set; }
        public int StepNo { get; set; }
        public int RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public string StopTitle { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public int? EstimatedMinutes { get; set; }
    }
}
