using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Author
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Author name is required")]
        [Display(Name = "Author Name")]
        public string AuthorName { get; set; } = string.Empty;

        [Display(Name = "Phone Number")]
        public string? AuthorPhone { get; set; }

        [Display(Name = "Email Address")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? AuthorEmail { get; set; }

        [Required(ErrorMessage = "Age is required")]
        [Display(Name = "Age")]
        [Range(1, 120, ErrorMessage = "Age must be between 1 and 120")]
        public int AuthorAge { get; set; }

        public ICollection<Item>? Books { get; set; }
    }
}