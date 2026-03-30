using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;

namespace EventEase.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bookings
        public async Task<IActionResult> Index()
        {
            var bookings = _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Event);
            return View(await bookings.ToListAsync());
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Event)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            // Populate dropdown lists
            ViewBag.Venues = _context.Venues.ToList();
            ViewBag.Events = _context.Events.ToList();

            // Set default booking date to today
            var booking = new Booking
            {
                BookingDate = DateTime.Today,
                Status = BookingStatus.Confirmed
            };

            return View(booking);
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingId,VenueId,EventId,BookingDate,CustomerName,CustomerEmail,CustomerPhone,Status")] Booking booking)
        {
            // Remove validation for navigation properties
            ModelState.Remove("Venue");
            ModelState.Remove("Event");

            // Validate that the venue and event exist
            var venue = await _context.Venues.FindAsync(booking.VenueId);
            var @event = await _context.Events.FindAsync(booking.EventId);

            if (venue == null || @event == null)
            {
                ModelState.AddModelError("", "Invalid venue or event selected.");
                ViewBag.Venues = _context.Venues.ToList();
                ViewBag.Events = _context.Events.ToList();
                return View(booking);
            }

            // Validate the booking date is within the event's date range
            var bookingDateOnly = booking.BookingDate.Date;
            var eventStartDateOnly = @event.StartDate.Date;
            var eventEndDateOnly = @event.EndDate.Date;

            if (bookingDateOnly < eventStartDateOnly || bookingDateOnly > eventEndDateOnly)
            {
                ModelState.AddModelError("BookingDate",
                    $"Booking date must be between {eventStartDateOnly:MMM dd, yyyy} and {eventEndDateOnly:MMM dd, yyyy}");
                ViewBag.Venues = _context.Venues.ToList();
                ViewBag.Events = _context.Events.ToList();
                return View(booking);
            }

            // Check for double booking - only check for confirmed and pending bookings
            var isBooked = await _context.Bookings
                .AnyAsync(b => b.VenueId == booking.VenueId &&
                               b.BookingDate.Date == booking.BookingDate.Date &&
                               b.BookingId != booking.BookingId &&
                               b.Status != BookingStatus.Cancelled);

            if (isBooked)
            {
                ModelState.AddModelError("", "This venue is already booked for the selected date.");
                ViewBag.Venues = _context.Venues.ToList();
                ViewBag.Events = _context.Events.ToList();
                return View(booking);
            }

            if (ModelState.IsValid)
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Booking created successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Venues = _context.Venues.ToList();
            ViewBag.Events = _context.Events.ToList();
            return View(booking);
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Event)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            ViewBag.Venues = _context.Venues.ToList();
            ViewBag.Events = _context.Events.ToList();

            // Pass the event date range to the view for validation
            if (booking.Event != null)
            {
                ViewBag.EventStartDate = booking.Event.StartDate.ToString("yyyy-MM-dd");
                ViewBag.EventEndDate = booking.Event.EndDate.ToString("yyyy-MM-dd");
                ViewBag.EventStartDisplay = booking.Event.StartDate.ToString("MMM dd, yyyy");
                ViewBag.EventEndDisplay = booking.Event.EndDate.ToString("MMM dd, yyyy");
            }

            return View(booking);
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,VenueId,EventId,BookingDate,CustomerName,CustomerEmail,CustomerPhone,Status")] Booking booking)
        {
            if (id != booking.BookingId)
            {
                return NotFound();
            }

            // Remove validation for navigation properties
            ModelState.Remove("Venue");
            ModelState.Remove("Event");

            // Validate that the venue and event exist
            var venue = await _context.Venues.FindAsync(booking.VenueId);
            var @event = await _context.Events.FindAsync(booking.EventId);

            if (venue == null || @event == null)
            {
                ModelState.AddModelError("", "Invalid venue or event selected.");
                ViewBag.Venues = _context.Venues.ToList();
                ViewBag.Events = _context.Events.ToList();
                return View(booking);
            }

            // FIXED: Validate the booking date is within the event's date range
            var bookingDateOnly = booking.BookingDate.Date;
            var eventStartDateOnly = @event.StartDate.Date;
            var eventEndDateOnly = @event.EndDate.Date;

            if (bookingDateOnly < eventStartDateOnly || bookingDateOnly > eventEndDateOnly)
            {
                ModelState.AddModelError("BookingDate",
                    $"Booking date must be between {eventStartDateOnly:MMM dd, yyyy} and {eventEndDateOnly:MMM dd, yyyy}");
                ViewBag.Venues = _context.Venues.ToList();
                ViewBag.Events = _context.Events.ToList();

                // Pass event dates to view for display
                if (@event != null)
                {
                    ViewBag.EventStartDate = @event.StartDate.ToString("yyyy-MM-dd");
                    ViewBag.EventEndDate = @event.EndDate.ToString("yyyy-MM-dd");
                    ViewBag.EventStartDisplay = @event.StartDate.ToString("MMM dd, yyyy");
                    ViewBag.EventEndDisplay = @event.EndDate.ToString("MMM dd, yyyy");
                }

                return View(booking);
            }

            // Check for double booking (excluding current booking)
            var isBooked = await _context.Bookings
                .AnyAsync(b => b.VenueId == booking.VenueId &&
                               b.BookingDate.Date == booking.BookingDate.Date &&
                               b.BookingId != booking.BookingId &&
                               b.Status != BookingStatus.Cancelled);

            if (isBooked)
            {
                ModelState.AddModelError("", "This venue is already booked for the selected date.");
                ViewBag.Venues = _context.Venues.ToList();
                ViewBag.Events = _context.Events.ToList();
                return View(booking);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Booking updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingId))
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

            ViewBag.Venues = _context.Venues.ToList();
            ViewBag.Events = _context.Events.ToList();
            return View(booking);
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Event)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Booking deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }
    }
}