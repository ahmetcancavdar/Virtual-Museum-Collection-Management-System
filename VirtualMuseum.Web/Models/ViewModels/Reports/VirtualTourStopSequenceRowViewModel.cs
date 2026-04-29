namespace VirtualMuseum.Web.Models.ViewModels.Reports
{
    public class VirtualTourStopSequenceRowViewModel
    {
        public int ExhibitionId { get; set; }
        public string ExhibitionTitle { get; set; } = string.Empty;

        public int PlanId { get; set; }
        public string PlanTitle { get; set; } = string.Empty;

        public int StepNo { get; set; }
        public int RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;

        public string StopTitle { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        public int? EstimatedMinutes { get; set; }
    }
}
