using FinanceBudgetApp.Models;

namespace FinanceBudgetApp.Services;

public class InMemoryDataService
{
    private static List<User> _users = new List<User>();
    private static List<Category> _categories = new List<Category>();
    private static List<Transaction> _transactions = new List<Transaction>();
    private static int _nextUserId = 1;
    private static int _nextCategoryId = 1;
    private static int _nextTransactionId = 1;

    static InMemoryDataService()
    {
        // Initialize with some default categories (these will be copied for each user)
        // We'll create them per user when they register
    }

    public List<User> Users => _users;
    public List<Category> Categories => _categories;
    public List<Transaction> Transactions => _transactions;

    public int GetNextUserId() => _nextUserId++;
    public int GetNextCategoryId() => _nextCategoryId++;
    public int GetNextTransactionId() => _nextTransactionId++;

    public void CreateDefaultCategoriesForUser(int userId)
    {
        var defaultCategories = new[]
        {
            new Category { Id = GetNextCategoryId(), Name = "Salary", Type = "Income", UserId = userId },
            new Category { Id = GetNextCategoryId(), Name = "Freelance", Type = "Income", UserId = userId },
            new Category { Id = GetNextCategoryId(), Name = "Business", Type = "Income", UserId = userId },
            new Category { Id = GetNextCategoryId(), Name = "Food", Type = "Expense", UserId = userId },
            new Category { Id = GetNextCategoryId(), Name = "Rent", Type = "Expense", UserId = userId },
            new Category { Id = GetNextCategoryId(), Name = "Transport", Type = "Expense", UserId = userId },
            new Category { Id = GetNextCategoryId(), Name = "Entertainment", Type = "Expense", UserId = userId },
            new Category { Id = GetNextCategoryId(), Name = "Bills", Type = "Expense", UserId = userId },
            new Category { Id = GetNextCategoryId(), Name = "Shopping", Type = "Expense", UserId = userId },
            new Category { Id = GetNextCategoryId(), Name = "Health", Type = "Expense", UserId = userId },
            new Category { Id = GetNextCategoryId(), Name = "Other", Type = "Expense", UserId = userId }
        };

        _categories.AddRange(defaultCategories);
    }
}

