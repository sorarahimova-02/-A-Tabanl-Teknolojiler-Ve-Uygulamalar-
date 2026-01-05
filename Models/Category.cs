namespace FinanceBudgetApp.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "Income" or "Expense"

    public int? UserId { get; set; }  // ✅ nullable

    // Navigation properties
    public User? User { get; set; }   // ✅ nullable
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}

