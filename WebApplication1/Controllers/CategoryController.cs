using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize] // حماية كامل Controller
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Category - جميع المستخدمين المسجلين يمكنهم المشاهدة
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Include(c => c.Items)
                .ToListAsync();
            return View(categories);
        }

        // GET: Category/New - فقط Editor و Admin يمكنهم الإنشاء
        [Authorize(Policy = "EditorOrAdmin")]
        public IActionResult New()
        {
            return View();
        }

        // POST: Category/New - فقط Editor و Admin يمكنهم الإنشاء
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditorOrAdmin")]
        public async Task<IActionResult> New(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                TempData["successData"] = "Category has been added successfully";
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // GET: Category/Edit/5 - فقط Editor و Admin يمكنهم التعديل
        [Authorize(Policy = "EditorOrAdmin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Category/Edit/5 - فقط Editor و Admin يمكنهم التعديل
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditorOrAdmin")]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    TempData["successData"] = "Category has been updated successfully";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // GET: Category/Delete/5 - فقط Admin يمكنه الحذف
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var category = await _context.Categories
                .Include(c => c.Items)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Category/Delete/5 - فقط Admin يمكنه الحذف
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }
            await _context.SaveChangesAsync();
            TempData["successData"] = "Category has been deleted successfully";
            return RedirectToAction("Index");
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}