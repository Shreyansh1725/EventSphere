using EventSphere.Models;

namespace EventSphere.Repositories
{
    /// <summary>
    /// Interface for user data operations.
    /// Demonstrates: Interface abstraction, Dependency Injection.
    /// </summary>
    public interface IUserRepository
    {
        User? GetByEmail(string email);
        User? GetById(int userId);
        bool EmailExists(string email);
        bool EnrollmentNumberExists(string enrollmentNumber);
        int Create(User user, string password);
        bool UpdateProfile(User user);
        bool UpdatePassword(int userId, string newPasswordHash);
        bool ToggleActive(int userId, bool isActive);
        List<User> GetAllStudents(string? searchTerm = null, string? department = null);
        string? GetPasswordHash(int userId);
    }
}
