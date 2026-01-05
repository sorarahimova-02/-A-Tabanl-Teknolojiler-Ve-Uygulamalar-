using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceBudgetApp.Services;
using FinanceBudgetApp.Data;
using FinanceBudgetApp.Models;

namespace FinanceBudgetApp.Controllers;

public class DashboardController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly AuthService _authService;

    public DashboardController(ApplicationDbContext db, AuthService authService)
    {
        _db = db;
        _authService = authService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _authService.GetCurrentUserId();
        if (!userId.HasValue)
            return RedirectToAction("Login", "Account");

        // ✅ User'ı DB'den al
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
        if (user == null)
        {
            // Session var ama user yoksa -> session temizle
            _authService.Logout();
            return RedirectToAction("Login", "Account");
        }

        // ✅ User'a ait kategoriler (ve global kategoriler UserId=null)
        var categories = await _db.Categories
            .Where(c => c.UserId == userId.Value || c.UserId == null)
            .ToListAsync();

        // ✅ User'a ait transactions (Category dahil)
        var transactions = await _db.Transactions
            .Where(t => t.UserId == userId.Value)
            .Include(t => t.Category)
            .ToListAsync();

        var totalIncome = transactions
            .Where(t => t.Type == "Income")
            .Sum(t => t.Amount);

        var totalExpenses = transactions
            .Where(t => t.Type == "Expense")
            .Sum(t => t.Amount);

        var balance = totalIncome - totalExpenses;

        var recentTransactions = transactions
            .OrderByDescending(t => t.Date)
            .Take(10)
            .ToList();

        var expensesByCategory = transactions
            .Where(t => t.Type == "Expense")
            .GroupBy(t => t.Category?.Name ?? "Unknown")
            .Select(g => new
            {
                Category = g.Key,
                Amount = g.Sum(t => t.Amount)
            })
            .OrderByDescending(x => x.Amount)
            .ToList();

        ViewBag.TotalIncome = totalIncome;
        ViewBag.TotalExpenses = totalExpenses;
        ViewBag.Balance = balance;
        ViewBag.RecentTransactions = recentTransactions;
        ViewBag.ExpensesByCategory = expensesByCategory;
        ViewBag.Username = user.Username;

        return View();
    }
}
