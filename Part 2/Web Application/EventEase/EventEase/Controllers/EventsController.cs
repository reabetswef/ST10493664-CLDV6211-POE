using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;

namespace EventEase.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Events
        public async Task<IActionResult> Index()
        {
            return View(await _context.Events.ToListAsync());
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            // Set default values for new event
            var newEvent = new Event
            {
                StartDate = DateTime.Now.Date.AddHours(9), // Today at 9:00 AM
                EndDate = DateTime.Now.Date.AddDays(1).AddHours(17) // Tomorrow at 5:00 PM
            };
            return View(newEvent);
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventId,EventName,Description,StartDate,EndDate,ImageUrl")] Event @event)
        {
            // Remove validation errors for any fields we don't want to validate
            ModelState.Remove("Bookings");

            if (ModelState.IsValid)
            {
                // Validate that end date is after start date
                if (@event.EndDate <= @event.StartDate)
                {
                    ModelState.AddModelError("EndDate", "End date and time must be after start date and time.");
                    return View(@event);
                }

                // Validate that start date is not in the past (optional)
                if (@event.StartDate < DateTime.Now.Date)
                {
                    ModelState.AddModelError("StartDate", "Start date cannot be in the past.");
                    return View(@event);
                }

                _context.Add(@event);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Event created successfully!";
                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            return View(@event);
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }
            return View(@event);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventId,EventName,Description,StartDate,EndDate,ImageUrl")] Event @event)
        {
            if (id != @event.EventId)
            {
                return NotFound();
            }

            // Remove validation errors for any fields we don't want to validate
            ModelState.Remove("Bookings");

            if (ModelState.IsValid)
            {
                try
                {
                    // Validate that end date is after start date
                    if (@event.EndDate <= @event.StartDate)
                    {
                        ModelState.AddModelError("EndDate", "End date and time must be after start date and time.");
                        return View(@event);
                    }

                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Event updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.EventId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(@event);
        }

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (@event == null)
            {
                return NotFound();
            }

            // Check if event has any associated bookings to display warning
            var hasBookings = await _context.Bookings.AnyAsync(b => b.EventId == id);
            ViewBag.HasBookings = hasBookings;

            return View(@event);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Events.FindAsync(id);

            if (@event != null)
            {
                // Check if event has any associated bookings
                var hasBookings = await _context.Bookings.AnyAsync(b => b.EventId == id);
                if (hasBookings)
                {
                    TempData["ErrorMessage"] = "Cannot delete this event because it has existing bookings. Please remove or reassign the bookings first.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Events.Remove(@event);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Event deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventId == id);
        }

        // GET: Events/GetEventDates/5 (AJAX endpoint for booking form)
        [HttpGet]
        public async Task<IActionResult> GetEventDates(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }

            return Json(new
            {
                startDate = @event.StartDate.ToString("yyyy-MM-dd"),
                endDate = @event.EndDate.ToString("yyyy-MM-dd"),
                startDateTime = @event.StartDate.ToString("yyyy-MM-ddTHH:mm"),
                endDateTime = @event.EndDate.ToString("yyyy-MM-ddTHH:mm")
            });
        }
    }
}