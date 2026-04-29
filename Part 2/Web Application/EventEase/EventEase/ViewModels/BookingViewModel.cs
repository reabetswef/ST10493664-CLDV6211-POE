using EventEase.Models;

namespace EventEase.ViewModels
{
    public class BookingViewModel
    {
        public int BookingId { get; set; }
        public int VenueId { get; set; }
        public string VenueName { get; set; } = string.Empty;
        public string VenueLocation { get; set; } = string.Empty;
        public int VenueCapacity { get; set; }
        public string? VenueImageUrl { get; set; }

        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string EventDescription { get; set; } = string.Empty;
        public DateTime EventStartDate { get; set; }
        public DateTime EventEndDate { get; set; }
        public string? EventImageUrl { get; set; }

        public DateTime BookingDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
        public BookingStatus Status { get; set; }

        // For search functionality
        public string SearchTerm { get; set; } = string.Empty;
    }
}