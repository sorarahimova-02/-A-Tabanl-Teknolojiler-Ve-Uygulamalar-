using Microsoft.EntityFrameworkCore;
using FinanceBudgetApp.Models;

namespace FinanceBudgetApp.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Transaction> Transactions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---------------------------
        // User
        // ---------------------------
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Password).IsRequired().HasMaxLength(255);

            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // ---------------------------
        // Category
        // ---------------------------
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(20);

            entity.HasOne(e => e.User)
                  .WithMany(u => u.Categories)
                  .HasForeignKey(e => e.UserId)
                  .IsRequired(false)                  // ✅ UserId nullable
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------------------------
        // Transaction
        // ---------------------------
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Amount)
                  .HasColumnType("decimal(18,2)")
                  .IsRequired();

            entity.Property(e => e.Description)
                  .HasMaxLength(500);

            entity.Property(e => e.Type)
                  .IsRequired()
                  .HasMaxLength(20);

            // Transaction -> User (many)
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Transactions)      // ✅ DOĞRU
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Transaction -> Category (many)
            entity.HasOne(e => e.Category)
                  .WithMany(c => c.Transactions)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed sample categories (global defaults)
        SeedCategories(modelBuilder);
    }

    private void SeedCategories(ModelBuilder modelBuilder)
    {
        var incomeCategories = new[]
        {
            new Category { Id = 1, Name = "Salary", Type = "Income", UserId = null },
            new Category { Id = 2, Name = "Freelance", Type = "Income", UserId = null },
            new Category { Id = 3, Name = "Business", Type = "Income", UserId = null }
        };

        var expenseCategories = new[]
        {
            new Category { Id = 4, Name = "Food", Type = "Expense", UserId = null },
            new Category { Id = 5, Name = "Rent", Type = "Expense", UserId = null },
            new Category { Id = 6, Name = "Transport", Type = "Expense", UserId = null },
            new Category { Id = 7, Name = "Entertainment", Type = "Expense", UserId = null },
            new Category { Id = 8, Name = "Bills", Type = "Expense", UserId = null },
            new Category { Id = 9, Name = "Shopping", Type = "Expense", UserId = null },
            new Category { Id = 10, Name = "Health", Type = "Expense", UserId = null },
            new Category { Id = 11, Name = "Other", Type = "Expense", UserId = null }
        };

        modelBuilder.Entity<Category>().HasData(incomeCategories);
        modelBuilder.Entity<Category>().HasData(expenseCategories);
    }

    // Helper method to create default categories for a new user
    public async Task CreateDefaultCategoriesForUserAsync(int userId)
    {
        var defaultCategories = new[]
        {
            new Category { Name = "Salary", Type = "Income", UserId = userId },
            new Category { Name = "Freelance", Type = "Income", UserId = userId },
            new Category { Name = "Business", Type = "Income", UserId = userId },
            new Category { Name = "Food", Type = "Expense", UserId = userId },
            new Category { Name = "Rent", Type = "Expense", UserId = userId },
            new Category { Name = "Transport", Type = "Expense", UserId = userId },
            new Category { Name = "Entertainment", Type = "Expense", UserId = userId },
            new Category { Name = "Bills", Type = "Expense", UserId = userId },
            new Category { Name = "Shopping", Type = "Expense", UserId = userId },
            new Category { Name = "Health", Type = "Expense", UserId = userId },
            new Category { Name = "Other", Type = "Expense", UserId = userId }
        };

        Categories.AddRange(defaultCategories);
        await SaveChangesAsync();
    }
}
