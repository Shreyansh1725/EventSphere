using EventSphere.Models;

namespace EventSphere.Repositories
{
    /// <summary>
    /// Interface for event data operations.
    /// Demonstrates: Interface, ADO.NET CRUD abstraction.
    /// </summary>
    public interface IEventRepository
    {
        List<Event> GetAll(string? search = null, int? categoryId = null, string? status = null, string? dateFilter = null);
        Event? GetById(int eventId);
        int Create(Event evt);
        bool Update(Event evt);
        bool Delete(int eventId);
        bool UpdateStatus(int eventId, string status);
        bool DecrementSeats(int eventId);
        bool IncrementSeats(int eventId);
        List<EventCategory> GetCategories();
        List<Event> GetUpcoming(int count = 5);
        List<Event> GetByCoordinatorId(int coordinatorId);
    }
}
