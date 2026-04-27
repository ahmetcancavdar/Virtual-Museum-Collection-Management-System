namespace VirtualMuseum.Web.Models.ViewModels
{
    public class TourListItemViewModel
    {
        public int TourId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ExhibitionTitle { get; set; } = string.Empty;
        public DateOnly TourDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int Capacity { get; set; }
        public string Language { get; set; } = string.Empty;
        public string GuideName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
