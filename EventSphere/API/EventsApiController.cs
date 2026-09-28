using EventSphere.Models;
using EventSphere.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EventSphere.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly IEventRepository _eventRepo;

        public EventsController(IEventRepository eventRepo)
        {
            _eventRepo = eventRepo;
        }

        [HttpGet]
        public IActionResult GetEvents()
        {
            var events = _eventRepo.GetAll(status: "Published");
            return Ok(events);
        }

        [HttpGet("{id}")]
        public IActionResult GetEvent(int id)
        {
            var evt = _eventRepo.GetById(id);
            if (evt == null) return NotFound();
            return Ok(evt);
        }

        [HttpPost]
        public IActionResult CreateEvent([FromBody] Event evt)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            evt.Status = "Draft";
            int id = _eventRepo.Create(evt);
            evt.EventId = id;
            return CreatedAtAction(nameof(GetEvent), new { id = id }, evt);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateEvent(int id, [FromBody] Event evt)
        {
            if (id != evt.EventId) return BadRequest();
            if (!_eventRepo.Update(evt)) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteEvent(int id)
        {
            if (!_eventRepo.Delete(id)) return NotFound();
            return NoContent();
        }
    }
}
