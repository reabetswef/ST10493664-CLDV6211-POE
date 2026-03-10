//ST10493664 CLDV6211 POE Part 1
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Venue
    {
        [Key]
        public int VenueId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(200)]
        public string Location { get; set; }

        [Required]
        public int Capacity { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(300)]
        [Display(Name = "Image URL")]
        public string ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Navigation property
        public ICollection<Booking> Bookings { get; set; }
    }
}