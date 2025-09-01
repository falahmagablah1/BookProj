using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using System.Text.Json;

namespace WebApplication1.Controllers
{
    [Authorize] // حماية كامل Controller
    public class AuthorController : Controller
    {
        private readonly AppDbContext _context;

        public AuthorController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Author - جميع المستخدمين المسجلين يمكنهم المشاهدة
        public async Task<IActionResult> Index()
        {
            var authors = await _context.Authors.Include(a => a.Books).ToListAsync();
            return View(authors);
        }

        // GET: Author/Details/5 - جميع المستخدمين المسجلين يمكنهم المشاهدة
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var author = await _context.Authors
                .Include(a => a.Books)
                    .ThenInclude(b => b.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (author == null)
            {
                return NotFound();
            }

            return View(author);
        }

        // GET: Author/New - فقط Editor و Admin يمكنهم الإنشاء
        [Authorize(Policy = "EditorOrAdmin")]
        public IActionResult New()
        {
            return View();
        }

        // POST: Author/New - فقط Editor و Admin يمكنهم الإنشاء
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditorOrAdmin")]
        public async Task<IActionResult> New(Author author)
        {
            if (ModelState.IsValid)
            {
                _context.Add(author);
                await _context.SaveChangesAsync();
                TempData["successData"] = "Author has been added successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(author);
        }

        // GET: Author/Edit/5 - فقط Editor و Admin يمكنهم التعديل
        [Authorize(Policy = "EditorOrAdmin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var author = await _context.Authors.FindAsync(id);
            if (author == null)
            {
                return NotFound();
            }
            return View(author);
        }

        // POST: Author/Edit/5 - فقط Editor و Admin يمكنهم التعديل
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditorOrAdmin")]
        public async Task<IActionResult> Edit(int id, Author author)
        {
            if (id != author.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(author);
                    await _context.SaveChangesAsync();
                    TempData["successData"] = "Author has been updated successfully";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AuthorExists(author.Id))
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
            return View(author);
        }

        // GET: Author/Delete/5 - فقط Admin يمكنه الحذف
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var author = await _context.Authors
                .Include(a => a.Books)
                    .ThenInclude(b => b.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (author == null)
            {
                return NotFound();
            }

            return View(author);
        }

        // POST: Author/Delete/5 - فقط Admin يمكنه الحذف
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author != null)
            {
                _context.Authors.Remove(author);
            }

            await _context.SaveChangesAsync();
            TempData["successData"] = "Author has been deleted successfully";
            return RedirectToAction(nameof(Index));
        }

        private bool AuthorExists(int id)
        {
            return _context.Authors.Any(e => e.Id == id);
        }

        // ======================
        // Ajax Methods
        // ======================

        // GET: /Author/GetAuthorsData - جميع المستخدمين المسجلين
        [HttpGet]
        public async Task<IActionResult> GetAuthorsData()
        {
            try
            {
                var authors = await _context.Authors
                    .Include(a => a.Books)
                        .ThenInclude(b => b.Category)
                    .Select(a => new
                    {
                        a.Id,
                        a.AuthorName,
                        a.AuthorEmail,
                        a.AuthorPhone,
                        a.AuthorAge,
                        BooksCount = a.Books.Count,
                        Books = a.Books.Select(b => new
                        {
                            b.Id,
                            b.Name,
                            CategoryName = b.Category != null ? b.Category.Name : "Uncategorized",
                            TimeCreated = b.TimeCreated.ToString("dd/MM/yyyy")
                        }).ToList()
                    })
                    .ToListAsync();

                var result = new
                {
                    success = true,
                    data = new
                    {
                        authors = authors,
                        canEdit = User.IsInRole("Editor") || User.IsInRole("Admin"),
                        canDelete = User.IsInRole("Admin"),
                        baseUrl = Url.Content("~")
                    }
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading data: " + ex.Message });
            }
        }

        // GET: /Author/GetAllAuthors - جميع المستخدمين المسجلين
        [HttpGet]
        public async Task<IActionResult> GetAllAuthors()
        {
            try
            {
                var authors = await _context.Authors
                    .Include(a => a.Books)
                        .ThenInclude(b => b.Category)
                    .Select(a => new
                    {
                        a.Id,
                        a.AuthorName,
                        a.AuthorEmail,
                        a.AuthorPhone,
                        a.AuthorAge,
                        BooksCount = a.Books.Count,
                        Books = a.Books.Select(b => new
                        {
                            b.Id,
                            b.Name,
                            CategoryName = b.Category != null ? b.Category.Name : "Uncategorized",
                            TimeCreated = b.TimeCreated.ToString("dd/MM/yyyy")
                        }).ToList()
                    })
                    .ToListAsync();

                return Json(new { success = true, data = authors });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading authors: " + ex.Message });
            }
        }

        // GET: /Author/GetAuthorsWithBooks - جميع المستخدمين المسجلين
        [HttpGet]
        public async Task<IActionResult> GetAuthorsWithBooks()
        {
            try
            {
                var authors = await _context.Authors
                    .Include(a => a.Books)
                        .ThenInclude(b => b.Category)
                    .Where(a => a.Books.Any())
                    .Select(a => new
                    {
                        a.Id,
                        a.AuthorName,
                        a.AuthorEmail,
                        a.AuthorPhone,
                        a.AuthorAge,
                        BooksCount = a.Books.Count,
                        Books = a.Books.Select(b => new
                        {
                            b.Id,
                            b.Name,
                            CategoryName = b.Category != null ? b.Category.Name : "Uncategorized",
                            TimeCreated = b.TimeCreated.ToString("dd/MM/yyyy")
                        }).ToList()
                    })
                    .ToListAsync();

                return Json(new { success = true, data = authors });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading authors with books: " + ex.Message });
            }
        }

        // GET: /Author/GetAuthorsWithoutBooks - جميع المستخدمين المسجلين
        [HttpGet]
        public async Task<IActionResult> GetAuthorsWithoutBooks()
        {
            try
            {
                var authors = await _context.Authors
                    .Include(a => a.Books)
                        .ThenInclude(b => b.Category)
                    .Where(a => !a.Books.Any())
                    .Select(a => new
                    {
                        a.Id,
                        a.AuthorName,
                        a.AuthorEmail,
                        a.AuthorPhone,
                        a.AuthorAge,
                        BooksCount = 0,
                        Books = new List<object>()
                    })
                    .ToListAsync();

                return Json(new { success = true, data = authors });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading authors without books: " + ex.Message });
            }
        }

        // GET: /Author/GetAuthorDetails/5 - جميع المستخدمين المسجلين
        [HttpGet]
        public async Task<IActionResult> GetAuthorDetails(int id)
        {
            try
            {
                var author = await _context.Authors
                    .Include(a => a.Books)
                        .ThenInclude(b => b.Category)
                    .Where(a => a.Id == id)
                    .Select(a => new
                    {
                        a.Id,
                        a.AuthorName,
                        a.AuthorEmail,
                        a.AuthorPhone,
                        a.AuthorAge,
                        BooksCount = a.Books.Count,
                        Books = a.Books.Select(b => new
                        {
                            b.Id,
                            b.Name,
                            CategoryName = b.Category != null ? b.Category.Name : "Uncategorized",
                            TimeCreated = b.TimeCreated.ToString("dd/MM/yyyy")
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (author == null)
                {
                    return Json(new { success = false, message = "Author not found" });
                }

                return Json(new { success = true, data = author });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading author details: " + ex.Message });
            }
        }

        // POST: /Author/SearchAuthors - جميع المستخدمين المسجلين
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SearchAuthors([FromBody] SearchRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    return await GetAllAuthors();
                }

                var searchTerm = request.SearchTerm.ToLower();

                var authors = await _context.Authors
                    .Include(a => a.Books)
                        .ThenInclude(b => b.Category)
                    .Where(a => a.AuthorName.ToLower().Contains(searchTerm) ||
                               a.AuthorEmail.ToLower().Contains(searchTerm) ||
                               (a.AuthorPhone != null && a.AuthorPhone.Contains(searchTerm)))
                    .Select(a => new
                    {
                        a.Id,
                        a.AuthorName,
                        a.AuthorEmail,
                        a.AuthorPhone,
                        a.AuthorAge,
                        BooksCount = a.Books.Count,
                        Books = a.Books.Select(b => new
                        {
                            b.Id,
                            b.Name,
                            CategoryName = b.Category != null ? b.Category.Name : "Uncategorized",
                            TimeCreated = b.TimeCreated.ToString("dd/MM/yyyy")
                        }).ToList()
                    })
                    .ToListAsync();

                return Json(new { success = true, data = authors });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Search failed: " + ex.Message });
            }
        }

        // POST: /Author/SortAuthors - جميع المستخدمين المسجلين
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SortAuthors([FromBody] SortRequest request)
        {
            try
            {
                var authorsQuery = _context.Authors
                    .Include(a => a.Books)
                        .ThenInclude(b => b.Category)
                    .AsQueryable();

                // تطبيق الترتيب
                switch (request.SortBy?.ToLower())
                {
                    case "name-asc":
                        authorsQuery = authorsQuery.OrderBy(a => a.AuthorName);
                        break;
                    case "name-desc":
                        authorsQuery = authorsQuery.OrderByDescending(a => a.AuthorName);
                        break;
                    case "books-desc":
                        authorsQuery = authorsQuery.OrderByDescending(a => a.Books.Count);
                        break;
                    case "books-asc":
                        authorsQuery = authorsQuery.OrderBy(a => a.Books.Count);
                        break;
                    case "age-asc":
                        authorsQuery = authorsQuery.OrderBy(a => a.AuthorAge);
                        break;
                    case "age-desc":
                        authorsQuery = authorsQuery.OrderByDescending(a => a.AuthorAge);
                        break;
                    default:
                        authorsQuery = authorsQuery.OrderBy(a => a.AuthorName);
                        break;
                }

                var authors = await authorsQuery
                    .Select(a => new
                    {
                        a.Id,
                        a.AuthorName,
                        a.AuthorEmail,
                        a.AuthorPhone,
                        a.AuthorAge,
                        BooksCount = a.Books.Count,
                        Books = a.Books.Select(b => new
                        {
                            b.Id,
                            b.Name,
                            CategoryName = b.Category != null ? b.Category.Name : "Uncategorized",
                            TimeCreated = b.TimeCreated.ToString("dd/MM/yyyy")
                        }).ToList()
                    })
                    .ToListAsync();

                return Json(new { success = true, data = authors });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Sort failed: " + ex.Message });
            }
        }

        // DELETE: /Author/DeleteAjax/5 - فقط Admin (Ajax Version)
        [HttpDelete]
        [Route("Author/DeleteAjax/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            try
            {
                var author = await _context.Authors
                    .Include(a => a.Books)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (author == null)
                {
                    return Json(new { success = false, message = "Author not found" });
                }

                // التحقق من وجود كتب مرتبطة (اختياري - حسب متطلبات المشروع)
                if (author.Books.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "Cannot delete author. This author has published books. Please delete the books first."
                    });
                }

                _context.Authors.Remove(author);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Author has been deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Delete failed: " + ex.Message });
            }
        }

        // GET: /Author/GetAuthorStatistics - جميع المستخدمين المسجلين
        [HttpGet]
        public async Task<IActionResult> GetAuthorStatistics()
        {
            try
            {
                var statistics = new
                {
                    totalAuthors = await _context.Authors.CountAsync(),
                    authorsWithBooks = await _context.Authors.Where(a => a.Books.Any()).CountAsync(),
                    authorsWithoutBooks = await _context.Authors.Where(a => !a.Books.Any()).CountAsync(),
                    totalBooks = await _context.Items.CountAsync(),
                    averageAge = await _context.Authors.AverageAsync(a => (double?)a.AuthorAge) ?? 0,
                    oldestAuthor = await _context.Authors.MaxAsync(a => (int?)a.AuthorAge) ?? 0,
                    youngestAuthor = await _context.Authors.MinAsync(a => (int?)a.AuthorAge) ?? 0
                };

                return Json(new { success = true, data = statistics });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading statistics: " + ex.Message });
            }
        }

        // POST: /Author/BulkDelete - فقط Admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> BulkDelete([FromBody] BulkDeleteRequest request)
        {
            try
            {
                if (request.AuthorIds == null || !request.AuthorIds.Any())
                {
                    return Json(new { success = false, message = "No authors selected" });
                }

                var authors = await _context.Authors
                    .Include(a => a.Books)
                    .Where(a => request.AuthorIds.Contains(a.Id))
                    .ToListAsync();

                // التحقق من وجود كتب (اختياري)
                var authorsWithBooks = authors.Where(a => a.Books.Any()).ToList();
                if (authorsWithBooks.Any())
                {
                    var authorNames = string.Join(", ", authorsWithBooks.Select(a => a.AuthorName));
                    return Json(new
                    {
                        success = false,
                        message = $"Cannot delete authors with books: {authorNames}"
                    });
                }

                _context.Authors.RemoveRange(authors);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"{authors.Count} authors have been deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bulk delete failed: " + ex.Message });
            }
        }

        // GET: /Author/ExportAuthors - فقط Editor و Admin
        [HttpGet]
        [Authorize(Policy = "EditorOrAdmin")]
        public async Task<IActionResult> ExportAuthors(string format = "csv")
        {
            try
            {
                var authors = await _context.Authors
                    .Include(a => a.Books)
                    .ToListAsync();

                if (format.ToLower() == "csv")
                {
                    var csvContent = GenerateCsvContent(authors);
                    return File(System.Text.Encoding.UTF8.GetBytes(csvContent),
                              "text/csv",
                              "authors.csv");
                }
                else if (format.ToLower() == "json")
                {
                    var jsonContent = JsonSerializer.Serialize(authors.Select(a => new
                    {
                        a.Id,
                        a.AuthorName,
                        a.AuthorEmail,
                        a.AuthorPhone,
                        a.AuthorAge,
                        BooksCount = a.Books.Count
                    }), new JsonSerializerOptions { WriteIndented = true });

                    return File(System.Text.Encoding.UTF8.GetBytes(jsonContent),
                              "application/json",
                              "authors.json");
                }

                return Json(new { success = false, message = "Unsupported format" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Export failed: " + ex.Message });
            }
        }

        // Helper method لإنشاء محتوى CSV
        private string GenerateCsvContent(List<Author> authors)
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("ID,Author Name,Email,Phone,Age,Books Count");

            foreach (var author in authors)
            {
                csv.AppendLine($"{author.Id}," +
                             $"\"{author.AuthorName}\"," +
                             $"\"{author.AuthorEmail}\"," +
                             $"\"{author.AuthorPhone}\"," +
                             $"{author.AuthorAge}," +
                             $"{author.Books?.Count ?? 0}");
            }

            return csv.ToString();
        }
    }

    // Request Models for Ajax
    public class SearchRequest
    {
        public string SearchTerm { get; set; } = string.Empty;
    }

    public class SortRequest
    {
        public string SortBy { get; set; } = string.Empty;
    }

    public class BulkDeleteRequest
    {
        public List<int> AuthorIds { get; set; } = new List<int>();
    }
}