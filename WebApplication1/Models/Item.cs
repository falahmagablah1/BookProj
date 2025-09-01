using System.ComponentModel.DataAnnotations;
using WebApplication1.ViewModels;

namespace WebApplication1.Models
{
    public class Item
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Book title is required")]
        [Display(Name = "Book Title")]
        public string Name { get; set; } = string.Empty;

        public DateTime TimeCreated { get; set; } = DateTime.Now;

        // Category relationship
        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        // Author relationship
        [Required(ErrorMessage = "Author is required")]
        public int AuthorId { get; set; }
        public Author? Author { get; set; }

        public string? PdfPath { get; set; }

        public int DownloadCount { get; set; } = 0;

        // Navigation property for ratings
        public ICollection<BookRating>? Ratings { get; set; }

        // Computed property for average rating
        public double AverageRating => Ratings?.Any() == true ? Ratings.Average(r => r.Rating) : 0;
        public int TotalRatings => Ratings?.Count ?? 0;


    }
}
