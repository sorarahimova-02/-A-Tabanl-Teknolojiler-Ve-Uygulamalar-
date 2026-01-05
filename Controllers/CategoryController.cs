using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceBudgetApp.Data;
using FinanceBudgetApp.Models;
using FinanceBudgetApp.Services;

namespace FinanceBudgetApp.Controllers;

public class CategoryController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly AuthService _authService;

    public CategoryController(ApplicationDbContext db, AuthService authService)
    {
        _db = db;
        _authService = authService;
    }

    // GET: Category
    public async Task<IActionResult> Index()
    {
        var userId = _authService.GetCurrentUserId();
        if (!userId.HasValue)
            return RedirectToAction("Login", "Account");

        // Show user's categories + global categories (UserId == null)
        var categories = await _db.Categories
            .AsNoTracking()
            .Where(c => c.UserId == userId.Value || c.UserId == null)
            .OrderBy(c => c.Type)
            .ThenBy(c => c.Name)
            .ToListAsync();

        ViewBag.IncomeCategories = categories.Where(c => c.Type == "Income").ToList();
        ViewBag.ExpenseCategories = categories.Where(c => c.Type == "Expense").ToList();

        return View();
    }

    // GET: Category/Create
    public IActionResult Create()
    {
        var userId = _authService.GetCurrentUserId();
        if (!userId.HasValue)
            return RedirectToAction("Login", "Account");

        return View();
    }

    // POST: Category/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, string type)
    {
        var userId = _authService.GetCurrentUserId();
        if (!userId.HasValue)
            return RedirectToAction("Login", "Account");

        name = (name ?? "").Trim();

        if (string.IsNullOrWhiteSpace(name) || (type != "Income" && type != "Expense"))
        {
            ModelState.AddModelError("", "Invalid category name or type.");
            return View();
        }

        // Check duplicates (user scope + global scope)
        var exists = await _db.Categories.AnyAsync(c =>
            (c.UserId == userId.Value || c.UserId == null) &&
            c.Type == type &&
            c.Name.ToLower() == name.ToLower()
        );

        if (exists)
        {
            ModelState.AddModelError("", "Category already exists.");
            return View();
        }

        var category = new Category
        {
            Name = name,
            Type = type,
            UserId = userId.Value // user-owned
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Category created successfully!";
        return RedirectToAction("Index");
    }

    // POST: Category/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = _authService.GetCurrentUserId();
        if (!userId.HasValue)
            return RedirectToAction("Login", "Account");

        // Only allow deleting user-owned categories (NOT global categories)
        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId.Value);

        if (category == null)
            return NotFound();

        var hasTransactions = await _db.Transactions.AnyAsync(t => t.CategoryId == id && t.UserId == userId.Value);
        if (hasTransactions)
        {
            TempData["ErrorMessage"] = "Cannot delete category that has transactions. Please delete or update transactions first.";
            return RedirectToAction("Index");
        }

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Category deleted successfully!";
        return RedirectToAction("Index");
    }
}
