using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [Required]
        [Display(Name = "Venue")]
        public int VenueId { get; set; }

        [Required]
        [Display(Name = "Event")]
        public int EventId { get; set; }

        [Required(ErrorMessage = "Booking date is required")]
        [Display(Name = "Booking Date")]
        [DataType(DataType.DateTime)]
        public DateTime BookingDate { get; set; }

        [Required(ErrorMessage = "Customer name is required")]
        [StringLength(100, ErrorMessage = "Customer name cannot exceed 100 characters")]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100)]
        [Display(Name = "Customer Email")]
        public string? CustomerEmail { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20)]
        [Display(Name = "Customer Phone")]
        public string? CustomerPhone { get; set; }

        [Display(Name = "Booking Status")]
        public BookingStatus Status { get; set; } = BookingStatus.Confirmed;

        // Navigation properties
        [ForeignKey("VenueId")]
        public virtual Venue? Venue { get; set; }

        [ForeignKey("EventId")]
        public virtual Event? Event { get; set; }
    }

    public enum BookingStatus
    {
        Confirmed,
        Pending,
        Cancelled
    }
}