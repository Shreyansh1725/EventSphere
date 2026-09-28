using EventSphere.Models;
using EventSphere.ViewModels;

namespace EventSphere.Repositories
{
    public interface IAttendanceRepository
    {
        List<AttendanceRecord> GetByEventId(int eventId);
        bool MarkAttendance(int registrationId, string status, string markedBy);
        int GetAttendedCountByUser(int userId);
        int GetTotalAttendance();
    }
}
