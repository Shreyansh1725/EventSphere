using EventSphere.Models;
using EventSphere.Repositories;
using EventSphere.Services;
using EventSphere.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EventSphere.Controllers
{
    public class AdminController : Controller
    {
        private readonly IEventRepository _eventRepo;
        private readonly IRegistrationRepository _regRepo;
        private readonly IUserRepository _userRepo;
        private readonly IAttendanceRepository _attendanceRepo;
        private readonly INotificationRepository _notifRepo;
        private readonly DashboardService _dashboardService;
        private readonly ReportService _reportService;
        private readonly IVolunteerRepository _volunteerRepo;

        public AdminController(
            IEventRepository eventRepo, IRegistrationRepository regRepo,
            IUserRepository userRepo, IAttendanceRepository attendanceRepo,
            INotificationRepository notifRepo, DashboardService dashboardService,
            ReportService reportService, IVolunteerRepository volunteerRepo)
        {
            _eventRepo = eventRepo; _regRepo = regRepo; _userRepo = userRepo;
            _attendanceRepo = attendanceRepo; _notifRepo = notifRepo;
            _dashboardService = dashboardService; _reportService = reportService;
            _volunteerRepo = volunteerRepo;
        }

        public IActionResult Dashboard() => View(_dashboardService.GetAdminStats());

        // Manage Events
        public IActionResult Events() => View(_eventRepo.GetAll());
        
        [HttpGet]
        public IActionResult AddEvent() => View("EventForm", new EventFormViewModel { Categories = _eventRepo.GetCategories(), EventDate = DateTime.Today.AddDays(7) });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddEvent(EventFormViewModel model)
        {
            if (!ModelState.IsValid) { model.Categories = _eventRepo.GetCategories(); return View("EventForm", model); }
            var evt = new Event
            {
                EventName = model.EventName, Description = model.Description, CategoryId = model.CategoryId, Venue = model.Venue,
                EventDate = model.EventDate, StartTime = model.StartTime, EndTime = model.EndTime, Organizer = model.Organizer,
                MaximumParticipants = model.MaximumParticipants, AvailableSeats = model.MaximumParticipants,
                RegistrationDeadline = model.RegistrationDeadline, ImagePath = model.ImagePath, Status = model.Status
            };
            _eventRepo.Create(evt);
            TempData["SuccessMessage"] = "Event added.";
            return RedirectToAction("Events");
        }

        [HttpGet]
        public IActionResult EditEvent(int id)
        {
            var evt = _eventRepo.GetById(id);
            if (evt == null) return NotFound();
            return View("EventForm", new EventFormViewModel
            {
                EventId = evt.EventId, EventName = evt.EventName, Description = evt.Description, CategoryId = evt.CategoryId,
                Venue = evt.Venue, EventDate = evt.EventDate, StartTime = evt.StartTime, EndTime = evt.EndTime,
                Organizer = evt.Organizer, MaximumParticipants = evt.MaximumParticipants,
                RegistrationDeadline = evt.RegistrationDeadline, ImagePath = evt.ImagePath, Status = evt.Status,
                Categories = _eventRepo.GetCategories()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditEvent(EventFormViewModel model)
        {
            if (!ModelState.IsValid) { model.Categories = _eventRepo.GetCategories(); return View("EventForm", model); }
            var evt = new Event
            {
                EventId = model.EventId, EventName = model.EventName, Description = model.Description, CategoryId = model.CategoryId,
                Venue = model.Venue, EventDate = model.EventDate, StartTime = model.StartTime, EndTime = model.EndTime,
                Organizer = model.Organizer, MaximumParticipants = model.MaximumParticipants,
                RegistrationDeadline = model.RegistrationDeadline, ImagePath = model.ImagePath, Status = model.Status
            };
            _eventRepo.Update(evt);
            TempData["SuccessMessage"] = "Event updated.";
            return RedirectToAction("Events");
        }

        [HttpPost]
        public IActionResult DeleteEvent(int id)
        {
            _eventRepo.Delete(id);
            TempData["SuccessMessage"] = "Event deleted.";
            return RedirectToAction("Events");
        }

        public IActionResult EventRequests()
        {
            var events = _eventRepo.GetAll(status: "PendingApproval");
            return View(events);
        }

        [HttpPost]
        public IActionResult HandleRequest(int id, string actionType)
        {
            if (actionType == "Approve")
            {
                _eventRepo.UpdateStatus(id, "Published");
                TempData["SuccessMessage"] = "Event request approved and published.";
            }
            else if (actionType == "Reject")
            {
                _eventRepo.UpdateStatus(id, "Rejected");
                TempData["SuccessMessage"] = "Event request rejected.";
            }
            return RedirectToAction("EventRequests");
        }

        public IActionResult ToggleEventStatus(int id, string status)
        {
            _eventRepo.UpdateStatus(id, status);
            return RedirectToAction("Events");
        }

        // Manage Registrations
        public IActionResult Registrations(int? eventId) => View(_regRepo.GetAll(eventId));

        // Manage Students
        public IActionResult Students() => View(_userRepo.GetAllStudents());
        public IActionResult ToggleStudent(int id, bool active)
        {
            _userRepo.ToggleActive(id, active);
            return RedirectToAction("Students");
        }

        // Attendance & Volunteers
        public IActionResult Attendance(int? eventId)
        {
            var vm = new AttendanceViewModel { Events = _eventRepo.GetAll() };
            if (eventId.HasValue)
            {
                vm.SelectedEventId = eventId;
                vm.SelectedEventName = vm.Events.FirstOrDefault(e => e.EventId == eventId)?.EventName;
                vm.AttendanceRecords = _attendanceRepo.GetByEventId(eventId.Value);
                vm.Volunteers = _volunteerRepo.GetByEventId(eventId.Value);
                vm.AvailableStudents = _userRepo.GetAllStudents();
            }
            return View(vm);
        }

        [HttpPost]
        public IActionResult MarkAttendance(int regId, string status, int eventId)
        {
            _attendanceRepo.MarkAttendance(regId, status, HttpContext.Session.GetString("UserName") ?? "Admin");
            return RedirectToAction("Attendance", new { eventId });
        }

        [HttpPost]
        public IActionResult AssignVolunteer(int userId, int eventId)
        {
            if (!_volunteerRepo.IsAlreadyVolunteer(userId, eventId))
                _volunteerRepo.Assign(userId, eventId);
            return RedirectToAction("Attendance", new { eventId });
        }
        
        [HttpPost]
        public IActionResult RemoveVolunteer(int volunteerId, int eventId)
        {
            _volunteerRepo.Remove(volunteerId);
            return RedirectToAction("Attendance", new { eventId });
        }

        // Reports & Notifications
        public IActionResult Reports()
        {
            ViewBag.EventRegistrations = _reportService.GetEventWiseRegistrations();
            ViewBag.DepartmentParticipation = _reportService.GetDepartmentParticipation();
            return View();
        }

        [HttpPost]
        public IActionResult SendNotification(string title, string message)
        {
            _notifRepo.BroadcastToAllStudents(title, message);
            TempData["SuccessMessage"] = "Notification sent.";
            return RedirectToAction("Reports");
        }
    }
}
