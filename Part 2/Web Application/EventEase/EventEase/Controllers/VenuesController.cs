using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;
using EventEase.Services;
using EventEase.ViewModels;

namespace EventEase.Controllers
{
    public class VenuesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobStorageService _blobStorage;
        private readonly ILogger<VenuesController> _logger;  // ADD THIS LINE

        // UPDATE THE CONSTRUCTOR
        public VenuesController(ApplicationDbContext context, BlobStorageService blobStorage, ILogger<VenuesController> logger)
        {
            _context = context;
            _blobStorage = blobStorage;
            _logger = logger;  // ADD THIS LINE
        }

        // GET: Venues
        public async Task<IActionResult> Index()
        {
            var venues = await _context.Venues.ToListAsync();
            return View(venues);
        }

        // GET: Venues/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues.FirstOrDefaultAsync(m => m.VenueId == id);
            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        // GET: Venues/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Venues/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VenueViewModel viewModel)
        {
            // Remove ModelState errors for IFormFile (optional field)
            ModelState.Remove("ExistingImageUrl");

            if (ModelState.IsValid)
            {
                try
                {
                    var venue = new Venue
                    {
                        VenueName = viewModel.VenueName,
                        Location = viewModel.Location,
                        Capacity = viewModel.Capacity,
                        ImageUrl = string.Empty
                    };

                    // Handle image upload
                    if (viewModel.ImageFile != null && viewModel.ImageFile.Length > 0)
                    {
                        try
                        {
                            var imageUrl = await _blobStorage.UploadImageAsync(viewModel.ImageFile);
                            venue.ImageUrl = imageUrl;
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
                        venue.ImageUrl = "https://via.placeholder.com/300x200?text=No+Image";
                    }

                    _context.Add(venue);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Venue created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating venue");
                    ModelState.AddModelError("", "An error occurred while creating the venue. Please try again.");
                    return View(viewModel);
                }
            }

            return View(viewModel);
        }

        // GET: Venues/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues.FindAsync(id);
            if (venue == null)
            {
                return NotFound();
            }

            var viewModel = new VenueViewModel
            {
                VenueId = venue.VenueId,
                VenueName = venue.VenueName,
                Location = venue.Location,
                Capacity = venue.Capacity,
                ExistingImageUrl = venue.ImageUrl
            };

            return View(viewModel);
        }

        // POST: Venues/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VenueViewModel viewModel)
        {
            if (id != viewModel.VenueId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var venue = await _context.Venues.FindAsync(id);
                    if (venue == null)
                    {
                        return NotFound();
                    }

                    venue.VenueName = viewModel.VenueName;
                    venue.Location = viewModel.Location;
                    venue.Capacity = viewModel.Capacity;

                    // Handle image upload
                    if (viewModel.ImageFile != null)
                    {
                        var imageUrl = await _blobStorage.UploadImageAsync(viewModel.ImageFile, viewModel.ExistingImageUrl);
                        venue.ImageUrl = imageUrl;
                    }

                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Venue updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueExists(viewModel.VenueId))
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

        // GET: Venues/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues.FirstOrDefaultAsync(m => m.VenueId == id);
            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        // POST: Venues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venues.FindAsync(id);

            if (venue != null)
            {
                // Check if venue has any associated bookings
                var hasBookings = await _context.Bookings.AnyAsync(b => b.VenueId == id);
                if (hasBookings)
                {
                    TempData["ErrorMessage"] = "Cannot delete this venue because it has existing bookings.";
                    return RedirectToAction(nameof(Index));
                }

                // Delete image from blob storage if it exists
                if (!string.IsNullOrEmpty(venue.ImageUrl) && !venue.ImageUrl.Contains("via.placeholder.com"))
                {
                    await _blobStorage.DeleteImageAsync(venue.ImageUrl);
                }

                _context.Venues.Remove(venue);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Venue deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool VenueExists(int id)
        {
            return _context.Venues.Any(e => e.VenueId == id);
        }
    }
}