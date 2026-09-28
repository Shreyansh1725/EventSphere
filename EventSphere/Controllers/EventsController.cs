using EventSphere.Models;
using EventSphere.Repositories;
using EventSphere.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EventSphere.Controllers
{
    public class EventsController : Controller
    {
        private readonly IEventRepository _eventRepo;
        private readonly IRegistrationRepository _regRepo;
        private readonly INotificationRepository _notifRepo;

        public EventsController(IEventRepository eventRepo, IRegistrationRepository regRepo, INotificationRepository notifRepo)
        {
            _eventRepo = eventRepo; _regRepo = regRepo; _notifRepo = notifRepo;
        }

        private int? GetCurrentUserId() => HttpContext.Session.GetInt32("UserId");

        public IActionResult Index(string? search, int? categoryId, string? dateFilter, string? status)
        {
            var vm = new EventFilterViewModel
            {
                Events = _eventRepo.GetAll(search, categoryId, "Published", dateFilter),
                Categories = _eventRepo.GetCategories(),
                SearchTerm = search, CategoryId = categoryId, DateFilter = dateFilter, StatusFilter = status
            };
            return View(vm);
        }

        public IActionResult Details(int id)
        {
            var evt = _eventRepo.GetById(id);
            if (evt == null) return NotFound();
            var userId = GetCurrentUserId();
            ViewBag.IsAlreadyRegistered = userId.HasValue && _regRepo.IsAlreadyRegistered(userId.Value, id);
            ViewBag.IsLoggedIn = userId.HasValue;
            return View(evt);
        }

        [HttpGet]
        public IActionResult Register(int id)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return RedirectToAction("Login", "Account", new { returnUrl = $"/Events/Register/{id}" });
            var evt = _eventRepo.GetById(id);
            if (evt == null) return NotFound();
            if (evt.Status != "Published" || evt.AvailableSeats <= 0 || (evt.RegistrationDeadline.HasValue && evt.RegistrationDeadline < DateTime.Now) || _regRepo.IsAlreadyRegistered(userId.Value, id))
            {
                TempData["ErrorMessage"] = "Cannot register for this event.";
                return RedirectToAction("Details", new { id });
            }
            ViewBag.Event = evt;
            return View(evt);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(int id, string confirm)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return RedirectToAction("Login", "Account");
            var evt = _eventRepo.GetById(id);
            if (evt == null || _regRepo.IsAlreadyRegistered(userId.Value, id)) return RedirectToAction("Details", new { id });
            
            string regCode = $"EVT{DateTime.Now.Year}-{new Random().Next(10000, 99999)}";
            int regId = _regRepo.Create(new Registration { EventId = id, UserId = userId.Value, RegistrationCode = regCode, Status = "Registered" });
            _eventRepo.DecrementSeats(id);
            _notifRepo.Create(new Notification { UserId = userId.Value, Title = "Registration Confirmed", Message = $"Registered for '{evt.EventName}'. ID: {regCode}" });
            
            TempData["SuccessMessage"] = $"Successfully registered! ID: {regCode}";
            return RedirectToAction("Confirmation", new { id = regId });
        }

        public IActionResult Confirmation(int id)
        {
            var reg = _regRepo.GetById(id);
            if (reg == null) return RedirectToAction("Index");
            return View(reg);
        }
    }
}
