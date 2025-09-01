using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.DataAccess
{
    public class ItemService
    {
        private readonly AppDbContext _context;

        public ItemService(AppDbContext context)
        {
            _context = context;
        }

        public List<ItemVM> GetAllItems()
        {
            return _context.Items
                .Include(x => x.Category)
                .Include(x => x.Author)
                .Select(x => new ItemVM()
                {
                    Id = x.Id,
                    Name = x.Name,
                    CategoryName = x.Category.Name,
                    CategoryId = x.Category.Id,
                    AuthorName = x.Author.AuthorName,
                    AuthorId = x.Author.Id
                }).ToList();
        }

        public Item? GetItemById(int id)
        {
            return _context.Items
                .Include(i => i.Category)
                .Include(i => i.Author)
                .FirstOrDefault(i => i.Id == id);
        }

        public void AddItem(ItemVM itemVM)
        {
            var item = new Item
            {
                Name = itemVM.Name,
                CategoryId = itemVM.CategoryId,
                AuthorId = itemVM.AuthorId,
                TimeCreated = DateTime.Now
            };
            _context.Items.Add(item);
            _context.SaveChanges();
        }

        public void UpdateItem(ItemVM itemVM)
        {
            var item = _context.Items.FirstOrDefault(i => i.Id == itemVM.Id);
            if (item == null) return;

            item.Name = itemVM.Name;
            item.CategoryId = itemVM.CategoryId;
            item.AuthorId = itemVM.AuthorId;

            _context.Items.Update(item);
            _context.SaveChanges();
        }

        public void Remove(int id)
        {
            var item = _context.Items.FirstOrDefault(i => i.Id == id);
            if (item == null) return;

            _context.Items.Remove(item);
            _context.SaveChanges();
        }
    }
}