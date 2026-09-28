using EventSphere.Repositories;
using EventSphere.Services;
using EventSphere.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EventSphere.Controllers
{
    public class StudentController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IRegistrationRepository _regRepo;
        private readonly ICertificateRepository _certRepo;
        private readonly INotificationRepository _notifRepo;
        private readonly DashboardService _dashboardService;

        public StudentController(IUserRepository userRepo, IRegistrationRepository regRepo, ICertificateRepository certRepo, INotificationRepository notifRepo, DashboardService dashboardService)
        {
            _userRepo = userRepo;
            _regRepo = regRepo;
            _certRepo = certRepo;
            _notifRepo = notifRepo;
            _dashboardService = dashboardService;
        }

        private int GetCurrentUserId() => HttpContext.Session.GetInt32("UserId") ?? 0;
        private string GetCurrentUserName() => HttpContext.Session.GetString("UserName") ?? "Student";

        public IActionResult Dashboard()
        {
            var vm = _dashboardService.GetStudentStats(GetCurrentUserId(), GetCurrentUserName());
            return View(vm);
        }

        public IActionResult MyEvents() => View(_regRepo.GetByUserId(GetCurrentUserId()));

        public IActionResult Certificates() => View(_certRepo.GetByUserId(GetCurrentUserId()));

        public IActionResult ViewCertificate(int id)
        {
            var cert = _certRepo.GetById(id);
            if (cert == null || cert.StudentName != GetCurrentUserName()) return NotFound(); // simplistic auth check
            return View(cert);
        }

        [HttpGet]
        public IActionResult Profile()
        {
            var user = _userRepo.GetById(GetCurrentUserId());
            if (user == null) return RedirectToAction("Logout", "Account");
            return View(new ProfileViewModel { UserId = user.UserId, FullName = user.FullName, Email = user.Email, EnrollmentNumber = user.EnrollmentNumber ?? "", Phone = user.Phone, Department = user.Department, Semester = user.Semester });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            _userRepo.UpdateProfile(new Models.User { UserId = GetCurrentUserId(), Phone = model.Phone, Department = model.Department, Semester = model.Semester });
            TempData["SuccessMessage"] = "Profile updated successfully.";
            return RedirectToAction("Profile");
        }

        [HttpGet]
        public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            int userId = GetCurrentUserId();
            var currentHash = _userRepo.GetPasswordHash(userId);
            if (currentHash == null || !Repositories.UserRepository.VerifyPassword(model.CurrentPassword, currentHash))
            {
                ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                return View(model);
            }
            _userRepo.UpdatePassword(userId, Repositories.UserRepository.HashPassword(model.NewPassword));
            TempData["SuccessMessage"] = "Password changed successfully.";
            return RedirectToAction("Profile");
        }

        public IActionResult Notifications()
        {
            int userId = GetCurrentUserId();
            _notifRepo.MarkAllRead(userId);
            return View(_notifRepo.GetByUserId(userId));
        }
    }
}
