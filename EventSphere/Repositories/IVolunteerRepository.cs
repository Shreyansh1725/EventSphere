using EventSphere.Models;

namespace EventSphere.Repositories
{
    public interface IVolunteerRepository
    {
        List<Volunteer> GetByEventId(int eventId);
        bool Assign(int userId, int eventId);
        bool Remove(int volunteerId);
        bool IsAlreadyVolunteer(int userId, int eventId);
    }
}
