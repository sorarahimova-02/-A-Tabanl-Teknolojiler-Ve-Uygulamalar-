using System.ComponentModel.DataAnnotations;

namespace FinanceBudgetApp.Models;

public class TransactionViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    [Display(Name = "Amount")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Description is required")]
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date is required")]
    [Display(Name = "Date")]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "Category is required")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    [Required]
    [Display(Name = "Type")]
    public string Type { get; set; } = string.Empty; // "Income" or "Expense"

    // For dropdown lists
    public List<Category>? Categories { get; set; }
}

