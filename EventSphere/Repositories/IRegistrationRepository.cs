using EventSphere.Models;

namespace EventSphere.Repositories
{
    public interface IRegistrationRepository
    {
        List<Registration> GetByUserId(int userId);
        List<Registration> GetAll(int? eventId = null, string? status = null, string? searchTerm = null);
        Registration? GetById(int registrationId);
        bool IsAlreadyRegistered(int userId, int eventId);
        int Create(Registration registration);
        bool UpdateStatus(int registrationId, string status);
        int GetRegistrationCountByUser(int userId);
        int GetAttendedCountByUser(int userId);
    }
}
