using EventSphere.Models;
using EventSphere.Repositories;
using EventSphere.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EventSphere.Controllers
{
    public class CoordinatorController : Controller
    {
        private readonly IEventRepository _eventRepo;

        public CoordinatorController(IEventRepository eventRepo)
        {
            _eventRepo = eventRepo;
        }

        private int GetCurrentUserId() => HttpContext.Session.GetInt32("UserId") ?? 0;

        public IActionResult Dashboard()
        {
            var myEvents = _eventRepo.GetByCoordinatorId(GetCurrentUserId());
            ViewBag.TotalRequests = myEvents.Count;
            ViewBag.Approved = myEvents.Count(e => e.Status == "Published");
            ViewBag.Pending = myEvents.Count(e => e.Status == "PendingApproval");
            ViewBag.Rejected = myEvents.Count(e => e.Status == "Rejected");
            
            return View();
        }

        public IActionResult MyRequests()
        {
            var events = _eventRepo.GetByCoordinatorId(GetCurrentUserId());
            return View(events);
        }

        [HttpGet]
        public IActionResult RequestEvent()
        {
            return View(new EventFormViewModel { Categories = _eventRepo.GetCategories(), EventDate = DateTime.Today.AddDays(14) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RequestEvent(EventFormViewModel model)
        {
            if (!ModelState.IsValid) 
            { 
                model.Categories = _eventRepo.GetCategories(); 
                return View(model); 
            }

            var evt = new Event
            {
                EventName = model.EventName, 
                Description = model.Description, 
                CategoryId = model.CategoryId, 
                Venue = model.Venue,
                EventDate = model.EventDate, 
                StartTime = model.StartTime, 
                EndTime = model.EndTime, 
                Organizer = HttpContext.Session.GetString("UserName"),
                MaximumParticipants = model.MaximumParticipants, 
                AvailableSeats = model.MaximumParticipants,
                RegistrationDeadline = model.RegistrationDeadline, 
                ImagePath = model.ImagePath, 
                Status = "PendingApproval",
                CoordinatorId = GetCurrentUserId()
            };

            _eventRepo.Create(evt);
            TempData["SuccessMessage"] = "Event request submitted successfully. Waiting for admin approval.";
            return RedirectToAction("MyRequests");
        }
    }
}
