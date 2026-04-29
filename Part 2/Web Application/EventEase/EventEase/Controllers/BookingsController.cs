using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;
using EventEase.Services;
using EventEase.ViewModels;

namespace EventEase.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(ApplicationDbContext context, ILogger<BookingsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Bookings - Enhanced view with search
        public async Task<IActionResult> Index(string searchTerm)
        {
            ViewBag.CurrentSearchTerm = searchTerm;

            var bookingsQuery = _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Event)
                .AsQueryable();

            // Apply search filter if provided
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                bookingsQuery = bookingsQuery.Where(b =>
                    b.BookingId.ToString().Contains(searchTerm) ||
                    (b.Event != null && b.Event.EventName.ToLower().Contains(searchTerm)) ||
                    (b.CustomerName != null && b.CustomerName.ToLower().Contains(searchTerm)) ||
                    (b.Venue != null && b.Venue.VenueName.ToLower().Contains(searchTerm))
                );
            }

            var bookings = await bookingsQuery
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            // Map to ViewModel
            var bookingViewModels = bookings.Select(b => new BookingViewModel
            {
                BookingId = b.BookingId,
                VenueId = b.VenueId,
                VenueName = b.Venue?.VenueName ?? "N/A",
                VenueLocation = b.Venue?.Location ?? "N/A",
                VenueCapacity = b.Venue?.Capacity ?? 0,
                VenueImageUrl = b.Venue?.ImageUrl,
                EventId = b.EventId,
                EventName = b.Event?.EventName ?? "N/A",
                EventDescription = b.Event?.Description ?? "N/A",
                EventStartDate = b.Event?.StartDate ?? DateTime.MinValue,
                EventEndDate = b.Event?.EndDate ?? DateTime.MinValue,
                EventImageUrl = b.Event?.ImageUrl,
                BookingDate = b.BookingDate,
                CustomerName = b.CustomerName,
                CustomerEmail = b.CustomerEmail,
                CustomerPhone = b.CustomerPhone,
                Status = b.Status
            }).ToList();

            return View(bookingViewModels);
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
            ViewBag.Venues = _context.Venues.ToList();
            ViewBag.Events = _context.Events.ToList();

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
            ModelState.Remove("Venue");
            ModelState.Remove("Event");

            var venue = await _context.Venues.FindAsync(booking.VenueId);
            var @event = await _context.Events.FindAsync(booking.EventId);

            if (venue == null || @event == null)
            {
                ModelState.AddModelError("", "Invalid venue or event selected.");
                ViewBag.Venues = _context.Venues.ToList();
                ViewBag.Events = _context.Events.ToList();
                return View(booking);
            }

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

            ModelState.Remove("Venue");
            ModelState.Remove("Event");

            var venue = await _context.Venues.FindAsync(booking.VenueId);
            var @event = await _context.Events.FindAsync(booking.EventId);

            if (venue == null || @event == null)
            {
                ModelState.AddModelError("", "Invalid venue or event selected.");
                ViewBag.Venues = _context.Venues.ToList();
                ViewBag.Events = _context.Events.ToList();
                return View(booking);
            }

            var bookingDateOnly = booking.BookingDate.Date;
            var eventStartDateOnly = @event.StartDate.Date;
            var eventEndDateOnly = @event.EndDate.Date;

            if (bookingDateOnly < eventStartDateOnly || bookingDateOnly > eventEndDateOnly)
            {
                ModelState.AddModelError("BookingDate",
                    $"Booking date must be between {eventStartDateOnly:MMM dd, yyyy} and {eventEndDateOnly:MMM dd, yyyy}");
                ViewBag.Venues = _context.Venues.ToList();
                ViewBag.Events = _context.Events.ToList();

                if (@event != null)
                {
                    ViewBag.EventStartDate = @event.StartDate.ToString("yyyy-MM-dd");
                    ViewBag.EventEndDate = @event.EndDate.ToString("yyyy-MM-dd");
                    ViewBag.EventStartDisplay = @event.StartDate.ToString("MMM dd, yyyy");
                    ViewBag.EventEndDisplay = @event.EndDate.ToString("MMM dd, yyyy");
                }

                return View(booking);
            }

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