using EventSphere.Models;
using EventSphere.Repositories;
using EventSphere.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EventSphere.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IUserRepository userRepo, ILogger<AccountController> logger)
        {
            _userRepo = userRepo;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (HttpContext.Session.GetInt32("UserId").HasValue)
            {
                var role = HttpContext.Session.GetString("Role");
                if (role == "Admin") return RedirectToAction("Dashboard", "Admin");
                if (role == "Coordinator") return RedirectToAction("Dashboard", "Coordinator");
                return RedirectToAction("Dashboard", "Student");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var user = _userRepo.GetByEmail(model.Email);
                if (user == null || !Repositories.UserRepository.VerifyPassword(model.Password, user.PasswordHash))
                {
                    ModelState.AddModelError("", "Invalid email or password.");
                    return View(model);
                }

                if (!user.IsActive)
                {
                    ModelState.AddModelError("", "Your account has been deactivated. Contact admin.");
                    return View(model);
                }

                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("UserName", user.FullName);
                HttpContext.Session.SetString("Role", user.Role);
                HttpContext.Session.SetString("Email", user.Email);

                if (model.RememberMe)
                {
                    var cookieOptions = new CookieOptions { Expires = DateTimeOffset.Now.AddDays(30), HttpOnly = true, IsEssential = true };
                    Response.Cookies.Append("ES_RememberEmail", user.Email, cookieOptions);
                }

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                if (user.Role == "Admin") return RedirectToAction("Dashboard", "Admin");
                if (user.Role == "Coordinator") return RedirectToAction("Dashboard", "Coordinator");
                return RedirectToAction("Dashboard", "Student");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred during login. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            var rememberedEmail = Request.Cookies["ES_RememberEmail"];
            return View(new RegisterViewModel { Email = rememberedEmail ?? "" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                if (_userRepo.EmailExists(model.Email))
                {
                    ModelState.AddModelError("Email", "This email is already registered.");
                    return View(model);
                }

                if (_userRepo.EnrollmentNumberExists(model.EnrollmentNumber))
                {
                    ModelState.AddModelError("EnrollmentNumber", "This enrollment number is already registered.");
                    return View(model);
                }

                var user = new User
                {
                    FullName = model.FullName, EnrollmentNumber = model.EnrollmentNumber,
                    Email = model.Email, Phone = model.Phone, Department = model.Department,
                    Semester = model.Semester, Role = "Student", IsActive = true
                };

                _userRepo.Create(user, model.Password);
                TempData["SuccessMessage"] = "Registration successful! Please login.";
                return RedirectToAction("Login");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Registration failed. Please try again.");
                return View(model);
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("ES_RememberEmail");
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }
    }
}
