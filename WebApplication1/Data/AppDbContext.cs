using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Item> Items { get; set; } = null!;
        public DbSet<Author> Authors { get; set; } = null!;
        public DbSet<BookRating> BookRatings { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // This is CRUCIAL for Identity

            // Configure Author entity
            modelBuilder.Entity<Author>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AuthorName)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.AuthorEmail)
                    .HasMaxLength(100);
                entity.Property(e => e.AuthorPhone)
                    .HasMaxLength(20);
            });

            // Configure Category entity
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            // Configure Item entity and relationships
            modelBuilder.Entity<Item>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.Property(e => e.PdfPath)
                    .HasMaxLength(500);

                // Configure relationship with Category
                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Items)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Configure relationship with Author
                entity.HasOne(d => d.Author)
                    .WithMany(p => p.Books)
                    .HasForeignKey(d => d.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Add index for duplicate checking
                entity.HasIndex(e => new { e.Name, e.AuthorId })
                    .HasDatabaseName("IX_Item_Name_Author")
                    .IsUnique();
            });

            // Configure BookRating entity
            modelBuilder.Entity<BookRating>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Rating)
                    .IsRequired();

                entity.Property(e => e.Comment)
                    .HasMaxLength(500);

                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("GETDATE()");

                // Configure relationships
                entity.HasOne(e => e.Item)
                    .WithMany(i => i.Ratings)
                    .HasForeignKey(e => e.ItemId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.BookRatings)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Ensure one rating per user per book
                entity.HasIndex(e => new { e.UserId, e.ItemId })
                    .HasDatabaseName("IX_BookRating_User_Item")
                    .IsUnique();
            });

            // Configure ApplicationUser additional properties
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(50);
            });
        }
    }
}