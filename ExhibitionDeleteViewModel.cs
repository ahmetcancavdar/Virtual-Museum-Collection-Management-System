namespace VirtualMuseum.Web.Models.ViewModels
{
    public class ExhibitionDeleteViewModel
    {
        public int ExhibitionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Theme { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
