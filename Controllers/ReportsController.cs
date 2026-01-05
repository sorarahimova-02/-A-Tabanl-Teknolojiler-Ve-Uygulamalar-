using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceBudgetApp.Data;
using FinanceBudgetApp.Models;
using FinanceBudgetApp.Services;

namespace FinanceBudgetApp.Controllers;

public class CategoryReport
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Count { get; set; }
}

public class ReportsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly AuthService _authService;

    public ReportsController(ApplicationDbContext db, AuthService authService)
    {
        _db = db;
        _authService = authService;
    }

    // GET: Reports/Index
    public async Task<IActionResult> Index()
    {
        var userId = _authService.GetCurrentUserId();
        if (!userId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        // Pull user transactions with category (so Category.Name is available)
        var transactions = await _db.Transactions
            .AsNoTracking()
            .Include(t => t.Category)
            .Where(t => t.UserId == userId.Value)
            .ToListAsync();

        // Monthly report (current month)
        var currentMonth = DateTime.Now.Month;
        var currentYear = DateTime.Now.Year;

        var monthlyIncome = transactions
            .Where(t => t.Type == "Income" && t.Date.Year == currentYear && t.Date.Month == currentMonth)
            .Sum(t => (decimal?)t.Amount) ?? 0;

        var monthlyExpenses = transactions
            .Where(t => t.Type == "Expense" && t.Date.Year == currentYear && t.Date.Month == currentMonth)
            .Sum(t => (decimal?)t.Amount) ?? 0;

        // Yearly report
        var yearlyIncome = transactions
            .Where(t => t.Type == "Income" && t.Date.Year == currentYear)
            .Sum(t => (decimal?)t.Amount) ?? 0;

        var yearlyExpenses = transactions
            .Where(t => t.Type == "Expense" && t.Date.Year == currentYear)
            .Sum(t => (decimal?)t.Amount) ?? 0;

        // Top spending categories
        var topCategories = transactions
            .Where(t => t.Type == "Expense")
            .GroupBy(t => t.Category != null ? t.Category.Name : "Unknown")
            .Select(g => new CategoryReport
            {
                Category = g.Key,
                Amount = g.Sum(x => x.Amount),
                Count = g.Count()
            })
            .OrderByDescending(x => x.Amount)
            .Take(5)
            .ToList();

        // Monthly trend (last 6 months)
        var monthlyTrend = new List<object>();
        for (int i = 5; i >= 0; i--)
        {
            var monthDate = DateTime.Now.AddMonths(-i);

            var monthExp = transactions
                .Where(t => t.Type == "Expense" && t.Date.Year == monthDate.Year && t.Date.Month == monthDate.Month)
                .Sum(t => (decimal?)t.Amount) ?? 0;

            monthlyTrend.Add(new
            {
                Month = monthDate.ToString("MMM yyyy"),
                Expenses = monthExp
            });
        }

        ViewBag.MonthlyIncome = monthlyIncome;
        ViewBag.MonthlyExpenses = monthlyExpenses;
        ViewBag.YearlyIncome = yearlyIncome;
        ViewBag.YearlyExpenses = yearlyExpenses;
        ViewBag.TopCategories = topCategories;
        ViewBag.MonthlyTrend = monthlyTrend;

        return View();
    }
}
