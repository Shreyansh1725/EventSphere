using EventSphere.Models;

namespace EventSphere.ViewModels
{
    public class EventFilterViewModel
    {
        public List<Event> Events { get; set; } = new();
        public List<EventCategory> Categories { get; set; } = new();
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public string? DateFilter { get; set; }
        public string? StatusFilter { get; set; }
    }
}
