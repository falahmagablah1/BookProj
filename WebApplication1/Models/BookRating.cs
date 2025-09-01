using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Models
{
    public class BookRating
    {
        public int Id { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [StringLength(500)]
        public string? Comment { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // User relationship
        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; } // <<== هاد الجديد

        // Item relationship
        [Required]
        public int ItemId { get; set; }
        public Item? Item { get; set; }
    }
}
