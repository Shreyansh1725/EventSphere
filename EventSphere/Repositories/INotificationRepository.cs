using EventSphere.Models;

namespace EventSphere.Repositories
{
    public interface INotificationRepository
    {
        List<Notification> GetByUserId(int userId);
        int GetUnreadCountByUserId(int userId);
        bool MarkAllRead(int userId);
        bool Create(Notification notification);
        bool BroadcastToAllStudents(string title, string message);
        List<Notification> GetAll();
    }
}
