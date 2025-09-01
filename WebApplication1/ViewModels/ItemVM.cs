using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;

namespace WebApplication1.ViewModels
{
    public class ItemVM
    {
        public int Id { get; set; }

        [Display(Name = "Book Title")]
        [Required(ErrorMessage = "Book title is required")]
        public string Name { get; set; } = string.Empty;

        public DateTime TimeCreated { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Category is required")]
        [DisplayName("Category")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category")]
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }

        [Required(ErrorMessage = "Author is required")]
        [DisplayName("Author")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select an author")]
        public int AuthorId { get; set; }
        public string? AuthorName { get; set; }

        [DisplayName("Book File (PDF)")]
        public IFormFile? PdfFile { get; set; }
        public string? PdfPath { get; set; }

        public int DownloadCount { get; set; }
        public double AverageRating { get; set; }
        public int TotalRatings { get; set; }

        // Ratings list for View
        public List<BookRatingVM>? Ratings { get; set; }
    }
}
