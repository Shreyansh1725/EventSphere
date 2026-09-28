using EventSphere.Models;

namespace EventSphere.Repositories
{
    public interface ICertificateRepository
    {
        List<Certificate> GetByUserId(int userId);
        Certificate? GetById(int certificateId);
        int GetCountByUserId(int userId);
        bool CreateIfNotExists(int registrationId);
    }
}
