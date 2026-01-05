namespace FinanceBudgetApp.Models;

public class Transaction
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Now;
    public int CategoryId { get; set; }
    public int UserId { get; set; }
    public string Type { get; set; } = string.Empty; // "Income" or "Expense"

    // Navigation properties
    public Category Category { get; set; } = null!;
    public User User { get; set; } = null!;
}

