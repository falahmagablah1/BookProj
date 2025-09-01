using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    public class BookRatingVM
    {
        public int Id { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string? Comment { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // User info
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }

        // Item info
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? AuthorName { get; set; }
        public string? CategoryName { get; set; }
    }
}
