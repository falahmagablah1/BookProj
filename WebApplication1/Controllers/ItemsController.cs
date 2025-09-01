using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class ItemsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;

        public ItemsController(AppDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        // GET: Items/Index
        public async Task<IActionResult> Index()
        {
            var items = await _context.Items
                .Include(i => i.Category)
                .Include(i => i.Author)
                .Include(i => i.Ratings)
                .Select(i => new ItemVM
                {
                    Id = i.Id,
                    Name = i.Name,
                    CategoryName = i.Category!.Name,
                    CategoryId = i.CategoryId,
                    AuthorName = i.Author!.AuthorName,
                    AuthorId = i.AuthorId,
                    TimeCreated = i.TimeCreated,
                    PdfPath = i.PdfPath,
                    DownloadCount = i.DownloadCount,
                    AverageRating = i.Ratings!.Any() ? i.Ratings.Average(r => r.Rating) : 0,
                    TotalRatings = i.Ratings!.Count()
                })
                .ToListAsync();

            return View(items);
        }

        // GET: Items/New
        [HttpGet]
        [Authorize(Policy = "EditorOrAdmin")]
        public IActionResult New()
        {
            var itemVM = new ItemVM();
            CreateSelectLists();
            return View(itemVM);
        }

        // POST: Items/New
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditorOrAdmin")]
        public async Task<IActionResult> New(ItemVM itemVM)
        {
            if (ModelState.IsValid)
            {
                // Check for duplicates
                var existingItem = await _context.Items
                    .FirstOrDefaultAsync(i => i.Name.ToLower() == itemVM.Name.ToLower() && i.AuthorId == itemVM.AuthorId);

                if (existingItem != null)
                {
                    ModelState.AddModelError("Name", "A book with this title already exists for this author.");
                    CreateSelectLists(itemVM.CategoryId, itemVM.AuthorId);
                    return View(itemVM);
                }

                var item = new Item
                {
                    Name = itemVM.Name,
                    CategoryId = itemVM.CategoryId,
                    AuthorId = itemVM.AuthorId,
                    TimeCreated = DateTime.Now
                };

                // Handle PDF file upload with improved security
                if (itemVM.PdfFile != null && itemVM.PdfFile.Length > 0)
                {
                    try
                    {
                        item.PdfPath = await SavePdfFileAsync(itemVM.PdfFile);
                    }
                    catch (InvalidOperationException ex)
                    {
                        ModelState.AddModelError("PdfFile", ex.Message);
                        CreateSelectLists(itemVM.CategoryId, itemVM.AuthorId);
                        return View(itemVM);
                    }
                }

                _context.Items.Add(item);
                await _context.SaveChangesAsync();
                TempData["successData"] = "Book has been added successfully";
                return RedirectToAction(nameof(Index));
            }

            CreateSelectLists(itemVM.CategoryId, itemVM.AuthorId);
            return View(itemVM);
        }

        // GET: Items/Edit/5
        [HttpGet]
        [Authorize(Policy = "EditorOrAdmin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.Items
                .Include(i => i.Category)
                .Include(i => i.Author)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null) return NotFound();

            var itemVM = new ItemVM
            {
                Id = item.Id,
                Name = item.Name,
                CategoryId = item.CategoryId,
                CategoryName = item.Category?.Name,
                AuthorId = item.AuthorId,
                AuthorName = item.Author?.AuthorName,
                PdfPath = item.PdfPath,
                TimeCreated = item.TimeCreated,
                DownloadCount = item.DownloadCount
            };

            CreateSelectLists(itemVM.CategoryId, itemVM.AuthorId);
            return View(itemVM);
        }

        // POST: Items/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditorOrAdmin")]
        public async Task<IActionResult> Edit(int id, ItemVM itemVM)
        {
            if (id != itemVM.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var item = await _context.Items.FindAsync(id);
                    if (item == null) return NotFound();

                    // Check for duplicates (excluding current item)
                    var existingItem = await _context.Items
                        .FirstOrDefaultAsync(i => i.Name.ToLower() == itemVM.Name.ToLower()
                            && i.AuthorId == itemVM.AuthorId && i.Id != id);

                    if (existingItem != null)
                    {
                        ModelState.AddModelError("Name", "A book with this title already exists for this author.");
                        CreateSelectLists(itemVM.CategoryId, itemVM.AuthorId);
                        return View(itemVM);
                    }

                    // Update basic properties
                    item.Name = itemVM.Name;
                    item.CategoryId = itemVM.CategoryId;
                    item.AuthorId = itemVM.AuthorId;

                    // Handle PDF file update
                    if (itemVM.PdfFile != null && itemVM.PdfFile.Length > 0)
                    {
                        try
                        {
                            // Delete old file
                            if (!string.IsNullOrEmpty(item.PdfPath))
                                DeletePdfFile(item.PdfPath);

                            item.PdfPath = await SavePdfFileAsync(itemVM.PdfFile);
                        }
                        catch (InvalidOperationException ex)
                        {
                            ModelState.AddModelError("PdfFile", ex.Message);
                            CreateSelectLists(itemVM.CategoryId, itemVM.AuthorId);
                            return View(itemVM);
                        }
                    }

                    _context.Update(item);
                    await _context.SaveChangesAsync();
                    TempData["successData"] = "Book updated successfully";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Items.Any(e => e.Id == id))
                        return NotFound();
                    throw;
                }
            }

            CreateSelectLists(itemVM.CategoryId, itemVM.AuthorId);
            return View(itemVM);
        }

        // GET: Items/Delete/5
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.Items
                .Include(i => i.Category)
                .Include(i => i.Author)
                .Include(i => i.Ratings)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (item == null) return NotFound();

            var itemVM = new ItemVM
            {
                Id = item.Id,
                Name = item.Name,
                CategoryName = item.Category?.Name,
                AuthorName = item.Author?.AuthorName,
                TimeCreated = item.TimeCreated,
                PdfPath = item.PdfPath,
                TotalRatings = item.Ratings?.Count ?? 0
            };

            return View(itemVM);
        }

        // POST: Items/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item != null)
            {
                // Delete PDF file
                if (!string.IsNullOrEmpty(item.PdfPath))
                    DeletePdfFile(item.PdfPath);

                _context.Items.Remove(item);
                await _context.SaveChangesAsync();
                TempData["successData"] = "Book deleted successfully";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Items/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            var item = await _context.Items
                .Include(i => i.Category)
                .Include(i => i.Author)
                .Include(i => i.Ratings)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null) return NotFound();

            // Set ViewBag data for rating functionality
            ViewBag.CanRate = !isAdmin && currentUser != null;
            ViewBag.IsAdmin = isAdmin;

            if (currentUser != null)
            {
                ViewBag.UserRating = item.Ratings?.FirstOrDefault(r => r.UserId == currentUser.Id);
            }

            return View(item);
        }

        // AJAX: Download PDF with improved functionality
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DownloadPdf(int itemId)
        {
            var item = await _context.Items.FindAsync(itemId);
            if (item == null || string.IsNullOrEmpty(item.PdfPath))
            {
                return Json(new { success = false, message = "PDF file not found" });
            }

            // Check if file exists
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, item.PdfPath.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
            {
                return Json(new { success = false, message = "PDF file not found on server" });
            }

            // Increment download count
            item.DownloadCount++;
            _context.Update(item);
            await _context.SaveChangesAsync();

            // Return success with download URL
            return Json(new
            {
                success = true,
                downloadUrl = item.PdfPath,
                downloadCount = item.DownloadCount
            });
        }

        // AJAX: Get Item Details
        [HttpGet]
        public async Task<IActionResult> GetItemDetails(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            var item = await _context.Items
                .Include(i => i.Category)
                .Include(i => i.Author)
                .Include(i => i.Ratings)
                 .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            // Check if current user has rated this book
            var userRating = item.Ratings?.FirstOrDefault(r => r.UserId == currentUser?.Id);

            var result = new
            {
                id = item.Id,
                name = item.Name,
                authorName = item.Author?.AuthorName,
                categoryName = item.Category?.Name,
                createdDate = item.TimeCreated.ToString("dd/MM/yyyy"),
                downloadCount = item.DownloadCount,
                averageRating = item.Ratings?.Any() == true ? Math.Round(item.Ratings.Average(r => r.Rating), 1) : 0,
                totalRatings = item.Ratings?.Count ?? 0,
                userRating = userRating?.Rating,
                userComment = userRating?.Comment,
                hasPdf = !string.IsNullOrEmpty(item.PdfPath),
                pdfPath = item.PdfPath,
                canRate = !isAdmin && currentUser != null, // Admin cannot rate
                ratings = isAdmin ? item.Ratings?.Select(r => new {
                    userName = r.User?.FullName ?? "Unknown User",
                    rating = r.Rating,
                    comment = r.Comment,
                    createdDate = r.CreatedDate.ToString("dd/MM/yyyy")
                }).OrderByDescending(r => r.createdDate).ToList() : null
            };

            return Json(result);
        }

        // AJAX: Rate Book
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RateBook(int itemId, int rating, string? comment = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Json(new { success = false, message = "User not authenticated" });
            }

            // Check if user is admin
            if (User.IsInRole("Admin"))
            {
                return Json(new { success = false, message = "Admins cannot rate books" });
            }

            // Validate rating
            if (rating < 1 || rating > 5)
            {
                return Json(new { success = false, message = "Rating must be between 1 and 5" });
            }

            var item = await _context.Items.FindAsync(itemId);
            if (item == null)
            {
                return Json(new { success = false, message = "Book not found" });
            }

            // Check if user already rated this book
            var existingRating = await _context.BookRatings
                .FirstOrDefaultAsync(r => r.ItemId == itemId && r.UserId == currentUser.Id);

            if (existingRating != null)
            {
                // Update existing rating
                existingRating.Rating = rating;
                existingRating.Comment = comment;
                existingRating.CreatedDate = DateTime.Now;
                _context.Update(existingRating);
            }
            else
            {
                // Create new rating
                var newRating = new BookRating
                {
                    ItemId = itemId,
                    UserId = currentUser.Id,
                    Rating = rating,
                    Comment = comment,
                    CreatedDate = DateTime.Now
                };
                _context.BookRatings.Add(newRating);
            }

            await _context.SaveChangesAsync();

            // Return updated statistics
            var updatedStats = await _context.BookRatings
                .Where(r => r.ItemId == itemId)
                .GroupBy(r => r.ItemId)
                .Select(g => new {
                    averageRating = Math.Round(g.Average(r => r.Rating), 1),
                    totalRatings = g.Count()
                })
                .FirstOrDefaultAsync();

            return Json(new
            {
                success = true,
                message = existingRating != null ? "Rating updated successfully" : "Rating added successfully",
                averageRating = updatedStats?.averageRating ?? rating,
                totalRatings = updatedStats?.totalRatings ?? 1
            });
        }

        // GET: Items/ViewRatings - Admin only view all ratings
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> ViewRatings(int? id = null)
        {
            IQueryable<BookRating> ratingsQuery = _context.BookRatings
                .Include(r => r.Item)
                    .ThenInclude(i => i.Author)
                .Include(r => r.Item)
                    .ThenInclude(i => i.Category)
                .Include(r => r.User);

            // If specific book ID provided, filter by that book
            if (id.HasValue)
            {
                ratingsQuery = ratingsQuery.Where(r => r.ItemId == id.Value);
                var bookTitle = await _context.Items.Where(i => i.Id == id.Value).Select(i => i.Name).FirstOrDefaultAsync();
                ViewData["BookTitle"] = bookTitle;
            }

            var ratings = await ratingsQuery
                .Select(r => new BookRatingVM
                {
                    Id = r.Id,
                    ItemId = r.ItemId,
                    ItemName = r.Item.Name,
                    AuthorName = r.Item.Author!.AuthorName,
                    CategoryName = r.Item.Category!.Name,
                    UserName = r.User!.FullName,
                    UserEmail = r.User.Email!,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedDate = r.CreatedDate
                })
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            return View(ratings);
        }

        // AJAX: Check for duplicates
        [HttpPost]
        public async Task<IActionResult> CheckDuplicate(string name, int authorId, int? itemId = null)
        {
            var query = _context.Items.Where(i => i.Name.ToLower() == name.ToLower() && i.AuthorId == authorId);

            if (itemId.HasValue)
            {
                query = query.Where(i => i.Id != itemId.Value);
            }

            var exists = await query.AnyAsync();

            return Json(new { exists = exists });
        }

        #region Helper Methods

        private void CreateSelectLists(int selectCategoryId = 0, int selectAuthorId = 0)
        {
            var categories = _context.Categories.ToList();
            categories.Insert(0, new Category { Id = 0, Name = "Select Category" });
            ViewBag.CategoryList = new SelectList(categories, "Id", "Name", selectCategoryId);

            var authors = _context.Authors.ToList();
            authors.Insert(0, new Author { Id = 0, AuthorName = "Select Author" });
            ViewBag.AuthorList = new SelectList(authors, "Id", "AuthorName", selectAuthorId);

            // Also populate for New view
            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Authors = _context.Authors.ToList();
        }

        private async Task<string> SavePdfFileAsync(IFormFile pdfFile)
        {
            // File size validation (10MB limit)
            if (pdfFile.Length > 10 * 1024 * 1024)
                throw new InvalidOperationException("File size cannot exceed 10MB");

            // MIME type validation
            if (pdfFile.ContentType != "application/pdf")
                throw new InvalidOperationException("Only PDF files are allowed");

            // File extension validation
            var allowedExtensions = new[] { ".pdf" };
            var fileExtension = Path.GetExtension(pdfFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
                throw new InvalidOperationException("Invalid file extension");

            // Basic file signature validation (PDF magic bytes)
            using var stream = pdfFile.OpenReadStream();
            var buffer = new byte[4];
            await stream.ReadAsync(buffer, 0, 4);
            if (buffer[0] != 0x25 || buffer[1] != 0x50 || buffer[2] != 0x44 || buffer[3] != 0x46)
                throw new InvalidOperationException("Invalid PDF file format");

            // Generate secure filename and save
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "pdfs");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string fileName = Guid.NewGuid().ToString() + ".pdf";
            string filePath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await pdfFile.CopyToAsync(fileStream);
            }

            return "/pdfs/" + fileName;
        }

        private void DeletePdfFile(string pdfPath)
        {
            if (!string.IsNullOrEmpty(pdfPath))
            {
                string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, pdfPath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    try
                    {
                        System.IO.File.Delete(fullPath);
                    }
                    catch
                    {
                        // Log error but don't throw - file might be in use
                    }
                }
            }
        }

        #endregion
    }
}