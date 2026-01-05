using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FinanceBudgetApp.Data;
using FinanceBudgetApp.Models;
using FinanceBudgetApp.Services;
using System.Text;

namespace FinanceBudgetApp.Controllers;

public class TransactionController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly AuthService _authService;

    public TransactionController(ApplicationDbContext db, AuthService authService)
    {
        _db = db;
        _authService = authService;
    }

    private int? CurrentUserId() => _authService.GetCurrentUserId();

    // GET: Transaction
    public async Task<IActionResult> Index(string? type, int? categoryId, DateTime? startDate, DateTime? endDate)
    {
        var userId = CurrentUserId();
        if (!userId.HasValue)
            return RedirectToAction("Login", "Account");

        var query = _db.Transactions
            .Where(t => t.UserId == userId.Value)
            .Include(t => t.Category)
            .AsQueryable();

        if (!string.IsNullOrEmpty(type))
            query = query.Where(t => t.Type == type);

        if (categoryId.HasValue)
            query = query.Where(t => t.CategoryId == categoryId.Value);

        if (startDate.HasValue)
            query = query.Where(t => t.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.Date <= endDate.Value);

        var transactionsList = await query
            .OrderByDescending(t => t.Date)
            .ToListAsync();

        // categories for filter dropdown (user categories + global categories)
        var allCategories = await _db.Categories
            .Where(c => c.UserId == userId.Value || c.UserId == null)
            .OrderBy(c => c.Type)
            .ThenBy(c => c.Name)
            .ToListAsync();

        ViewBag.Categories = new SelectList(allCategories, "Id", "Name", categoryId);
        ViewBag.TypeFilter = type;
        ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
        ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

        return View(transactionsList);
    }

    // GET: Transaction/Create
    public async Task<IActionResult> Create(string? type)
    {
        var userId = CurrentUserId();
        if (!userId.HasValue)
            return RedirectToAction("Login", "Account");

        var transactionType = type ?? "Expense";

        var categories = await _db.Categories
            .Where(c => (c.UserId == userId.Value || c.UserId == null) && c.Type == transactionType)
            .OrderBy(c => c.Name)
            .ToListAsync();

        if (!categories.Any())
        {
            TempData["ErrorMessage"] = $"No {transactionType} categories found.";
            return RedirectToAction("Index", "Dashboard");
        }

        var viewModel = new TransactionViewModel
        {
            Type = transactionType,
            Date = DateTime.Now,
            Categories = categories
        };

        ViewBag.Categories = new SelectList(categories, "Id", "Name");
        return View(viewModel);
    }

    // POST: Transaction/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TransactionViewModel viewModel)
    {
        var userId = CurrentUserId();
        if (!userId.HasValue)
            return RedirectToAction("Login", "Account");

        // Verify category exists and matches type (allow global category too)
        var category = await _db.Categories.FirstOrDefaultAsync(c =>
            c.Id == viewModel.CategoryId &&
            (c.UserId == userId.Value || c.UserId == null));

        if (category == null || category.Type != viewModel.Type)
            ModelState.AddModelError("CategoryId", "Invalid category selected.");

        if (ModelState.IsValid)
        {
            var transaction = new Transaction
            {
                Amount = viewModel.Amount,
                Description = viewModel.Description,
                Date = viewModel.Date,
                CategoryId = viewModel.CategoryId,
                UserId = userId.Value,
                Type = viewModel.Type
            };

            _db.Transactions.Add(transaction);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{viewModel.Type} transaction added successfully!";
            return RedirectToAction("Index");
        }

        // reload categories
        var categories = await _db.Categories
            .Where(c => (c.UserId == userId.Value || c.UserId == null) && c.Type == viewModel.Type)
            .OrderBy(c => c.Name)
            .ToListAsync();

        viewModel.Categories = categories;
        ViewBag.Categories = new SelectList(categories, "Id", "Name", viewModel.CategoryId);
        return View(viewModel);
    }

    // GET: Transaction/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        var userId = CurrentUserId();
        if (!userId.HasValue)
            return RedirectToAction("Login", "Account");

        if (id == null) return NotFound();

        var transaction = await _db.Transactions
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId.Value);

        if (transaction == null) return NotFound();

        var categories = await _db.Categories
            .Where(c => (c.UserId == userId.Value || c.UserId == null) && c.Type == transaction.Type)
            .OrderBy(c => c.Name)
            .ToListAsync();

        var viewModel = new TransactionViewModel
        {
            Id = transaction.Id,
            Amount = transaction.Amount,
            Description = transaction.Description,
            Date = transaction.Date,
            CategoryId = transaction.CategoryId,
            Type = transaction.Type,
            Categories = categories
        };

        ViewBag.Categories = new SelectList(categories, "Id", "Name", transaction.CategoryId);
        return View(viewModel);
    }

    // POST: Transaction/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TransactionViewModel viewModel)
    {
        var userId = CurrentUserId();
        if (!userId.HasValue)
            return RedirectToAction("Login", "Account");

        if (id != viewModel.Id) return NotFound();

        var transaction = await _db.Transactions
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId.Value);

        if (transaction == null) return NotFound();

        var category = await _db.Categories.FirstOrDefaultAsync(c =>
            c.Id == viewModel.CategoryId &&
            (c.UserId == userId.Value || c.UserId == null));

        if (category == null || category.Type != viewModel.Type)
            ModelState.AddModelError("CategoryId", "Invalid category selected.");

        if (ModelState.IsValid)
        {
            transaction.Amount = viewModel.Amount;
            transaction.Description = viewModel.Description;
            transaction.Date = viewModel.Date;
            transaction.CategoryId = viewModel.CategoryId;
            transaction.Type = viewModel.Type;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Transaction updated successfully!";
            return RedirectToAction("Index");
        }

        var categories = await _db.Categories
            .Where(c => (c.UserId == userId.Value || c.UserId == null) && c.Type == viewModel.Type)
            .OrderBy(c => c.Name)
            .ToListAsync();

        viewModel.Categories = categories;
        ViewBag.Categories = new SelectList(categories, "Id", "Name", viewModel.CategoryId);
        return View(viewModel);
    }

    // POST: Transaction/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = CurrentUserId();
        if (!userId.HasValue)
            return RedirectToAction("Login", "Account");

        var transaction = await _db.Transactions
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId.Value);

        if (transaction == null) return NotFound();

        _db.Transactions.Remove(transaction);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Transaction deleted successfully!";
        return RedirectToAction("Index");
    }

    // ---------------------------
    // IMPORT / EXPORT
    // ---------------------------

    // GET: Transaction/ImportExport
    [HttpGet]
    public IActionResult ImportExport()
    {
        if (!_authService.IsLoggedIn())
            return RedirectToAction("Login", "Account");

        return View();
    }

    // GET: Transaction/ExportCsv
    [HttpGet]
    public async Task<IActionResult> ExportCsv()
    {
        var userId = CurrentUserId();
        if (!userId.HasValue)
            return RedirectToAction("Login", "Account");

        var txs = await _db.Transactions
            .Where(t => t.UserId == userId.Value)
            .Include(t => t.Category)
            .OrderByDescending(t => t.Date)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Date,Type,Amount,Category,Description");

        foreach (var t in txs)
        {
            var date = t.Date.ToString("yyyy-MM-dd");
            var type = EscapeCsv(t.Type);
            var amount = t.Amount.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var category = EscapeCsv(t.Category?.Name ?? "");
            var desc = EscapeCsv(t.Description ?? "");
            sb.AppendLine($"{date},{type},{amount},{category},{desc}");
        }

        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "transactions.csv");
    }

    // POST: Transaction/ImportCsv
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportCsv(IFormFile file)
    {
        var userId = CurrentUserId();
        if (!userId.HasValue)
            return RedirectToAction("Login", "Account");

        if (file == null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "Please choose a CSV file.";
            return RedirectToAction("ImportExport");
        }

        int imported = 0, skipped = 0;

        using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8, true);

        var header = await reader.ReadLineAsync();
        if (header == null)
        {
            TempData["ErrorMessage"] = "CSV is empty.";
            return RedirectToAction("ImportExport");
        }

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) { skipped++; continue; }

            var cols = ParseCsvLine(line);
            if (cols.Count < 5) { skipped++; continue; }

            if (!DateTime.TryParse(cols[0], out var date)) { skipped++; continue; }

            var type = cols[1].Trim();
            if (type != "Income" && type != "Expense") { skipped++; continue; }

            if (!decimal.TryParse(cols[2], System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var amount))
            { skipped++; continue; }

            var categoryName = cols[3].Trim();
            var desc = cols[4].Trim();

            // find category for user or global
            var category = await _db.Categories.FirstOrDefaultAsync(c =>
                c.Name == categoryName &&
                c.Type == type &&
                (c.UserId == userId.Value || c.UserId == null));

            // if not found -> create user category
            if (category == null)
            {
                category = new Category
                {
                    Name = categoryName,
                    Type = type,
                    UserId = userId.Value
                };
                _db.Categories.Add(category);
                await _db.SaveChangesAsync();
            }

            var tx = new Transaction
            {
                UserId = userId.Value,
                Date = date,
                Type = type,
                Amount = amount,
                Description = string.IsNullOrWhiteSpace(desc) ? null : desc,
                CategoryId = category.Id
            };

            _db.Transactions.Add(tx);
            imported++;
        }

        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Imported: {imported}, Skipped: {skipped}";
        return RedirectToAction("ImportExport");
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    sb.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(sb.ToString());
                sb.Clear();
            }
            else
            {
                sb.Append(c);
            }
        }

        result.Add(sb.ToString());
        return result;
    }
}
