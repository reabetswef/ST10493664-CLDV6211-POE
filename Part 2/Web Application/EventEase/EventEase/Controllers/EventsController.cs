using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;
using EventEase.Services;
using EventEase.ViewModels;

namespace EventEase.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobStorageService _blobStorage;
        private readonly ILogger<EventsController> _logger;

        public EventsController(ApplicationDbContext context, BlobStorageService blobStorage, ILogger<EventsController> logger)
        {
            _context = context;
            _blobStorage = blobStorage;
            _logger = logger;
        }

        // GET: Events
        public async Task<IActionResult> Index()
        {
            var events = await _context.Events.ToListAsync();
            return View(events);
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
            var viewModel = new EventViewModel
            {
                StartDate = DateTime.Now.Date.AddHours(9),
                EndDate = DateTime.Now.Date.AddDays(1).AddHours(17)
            };
            return View(viewModel);
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventViewModel viewModel)
        {
            // Remove ModelState errors for IFormFile (optional field)
            ModelState.Remove("ExistingImageUrl");

            if (ModelState.IsValid)
            {
                try
                {
                    // Validate dates
                    if (viewModel.EndDate <= viewModel.StartDate)
                    {
                        ModelState.AddModelError("EndDate", "End date must be after start date.");
                        return View(viewModel);
                    }

                    var @event = new Event
                    {
                        EventName = viewModel.EventName,
                        Description = viewModel.Description,
                        StartDate = viewModel.StartDate,
                        EndDate = viewModel.EndDate,
                        ImageUrl = string.Empty
                    };

                    // Handle image upload
                    if (viewModel.ImageFile != null && viewModel.ImageFile.Length > 0)
                    {
                        try
                        {
                            var imageUrl = await _blobStorage.UploadImageAsync(viewModel.ImageFile);
                            @event.ImageUrl = imageUrl;
                            _logger.LogInformation($"Image uploaded successfully: {imageUrl}");
                        }
                        catch (ArgumentException argEx)
                        {
                            ModelState.AddModelError("ImageFile", argEx.Message);
                            return View(viewModel);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Image upload failed");
                            ModelState.AddModelError("", "Failed to upload image. Please try again.");
                            return View(viewModel);
                        }
                    }
                    else
                    {
                        // Default placeholder
                        @event.ImageUrl = "https://via.placeholder.com/300x200?text=No+Image";
                    }

                    _context.Add(@event);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Event created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating event");
                    ModelState.AddModelError("", "An error occurred while creating the event. Please try again.");
                    return View(viewModel);
                }
            }

            return View(viewModel);
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

            var viewModel = new EventViewModel
            {
                EventId = @event.EventId,
                EventName = @event.EventName,
                Description = @event.Description,
                StartDate = @event.StartDate,
                EndDate = @event.EndDate,
                ExistingImageUrl = @event.ImageUrl
            };

            return View(viewModel);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EventViewModel viewModel)
        {
            if (id != viewModel.EventId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var @event = await _context.Events.FindAsync(id);
                    if (@event == null)
                    {
                        return NotFound();
                    }

                    @event.EventName = viewModel.EventName;
                    @event.Description = viewModel.Description;
                    @event.StartDate = viewModel.StartDate;
                    @event.EndDate = viewModel.EndDate;

                    // Validate dates
                    if (@event.EndDate <= @event.StartDate)
                    {
                        ModelState.AddModelError("EndDate", "End date must be after start date.");
                        return View(viewModel);
                    }

                    // Handle image upload
                    if (viewModel.ImageFile != null)
                    {
                        try
                        {
                            var imageUrl = await _blobStorage.UploadImageAsync(viewModel.ImageFile, viewModel.ExistingImageUrl);
                            @event.ImageUrl = imageUrl;
                            _logger.LogInformation($"Image updated successfully: {imageUrl}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Image upload failed during edit");
                            ModelState.AddModelError("", "Failed to upload image. Please try again.");
                            return View(viewModel);
                        }
                    }

                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Event updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(viewModel.EventId))
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
            return View(viewModel);
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

            // Check if event has associated bookings
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
                    TempData["ErrorMessage"] = "Cannot delete this event because it has existing bookings.";
                    return RedirectToAction(nameof(Index));
                }

                // Delete image from blob storage if it exists
                if (!string.IsNullOrEmpty(@event.ImageUrl) && !@event.ImageUrl.Contains("via.placeholder.com"))
                {
                    try
                    {
                        await _blobStorage.DeleteImageAsync(@event.ImageUrl);
                        _logger.LogInformation($"Image deleted: {@event.ImageUrl}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete image from blob storage");
                    }
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
    }
}