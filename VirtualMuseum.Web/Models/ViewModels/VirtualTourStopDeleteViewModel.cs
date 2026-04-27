namespace VirtualMuseum.Web.Models.ViewModels
{
    public class VirtualTourStopDeleteViewModel
    {
        public int StopId { get; set; }
        public int PlanId { get; set; }
        public string PlanTitle { get; set; } = string.Empty;
        public string ExhibitionTitle { get; set; } = string.Empty;
        public int StepNo { get; set; }
        public int RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public string StopTitle { get; set; } = string.Empty;
        public int? EstimatedMinutes { get; set; }
    }
}
