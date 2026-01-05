using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceBudgetApp.Data;
using FinanceBudgetApp.Services;
using System.Security.Cryptography;
using System.Text;

namespace FinanceBudgetApp.Controllers;

public class SettingsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly AuthService _authService;

    public SettingsController(ApplicationDbContext db, AuthService authService)
    {
        _db = db;
        _authService = authService;
    }

    // GET: /Settings
    public async Task<IActionResult> Index()
    {
        var userId = _authService.GetCurrentUserId();
        if (!userId.HasValue)
            return RedirectToAction("Login", "Account");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
        if (user == null)
        {
            _authService.Logout();
            return RedirectToAction("Login", "Account");
        }

        ViewBag.User = user;
        return View();
    }

    // POST: /Settings/EditProfile
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(string email, string? fullName, string? phoneNumber, string? address)
    {
        var userId = _authService.GetCurrentUserId();
        if (!userId.HasValue)
            return RedirectToAction("Login", "Account");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
        if (user == null)
        {
            _authService.Logout();
            return RedirectToAction("Login", "Account");
        }

        email = (email ?? "").Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
        {
            TempData["ErrorMessage"] = "Invalid email address.";
            return RedirectToAction("Index");
        }

        // Check if email is already taken by another user
        var emailTaken = await _db.Users.AnyAsync(u => u.Id != userId.Value && u.Email.ToLower() == email);
        if (emailTaken)
        {
            TempData["ErrorMessage"] = "Email already exists.";
            return RedirectToAction("Index");
        }

        user.Email = email;
        user.FullName = string.IsNullOrWhiteSpace(fullName) ? null : fullName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();
        user.Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();

        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Profile updated successfully!";
        return RedirectToAction("Index");
    }

    // POST: /Settings/ChangePassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        var userId = _authService.GetCurrentUserId();
        if (!userId.HasValue)
            return RedirectToAction("Login", "Account");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
        if (user == null)
        {
            _authService.Logout();
            return RedirectToAction("Login", "Account");
        }

        if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword))
        {
            TempData["ErrorMessage"] = "Please fill all password fields.";
            return RedirectToAction("Index");
        }

        if (newPassword != confirmPassword)
        {
            TempData["ErrorMessage"] = "New passwords do not match.";
            return RedirectToAction("Index");
        }

        if (newPassword.Length < 6)
        {
            TempData["ErrorMessage"] = "Password must be at least 6 characters.";
            return RedirectToAction("Index");
        }

        // Verify current password (same SHA256 logic)
        var currentHash = HashPassword(currentPassword);
        if (user.Password != currentHash)
        {
            TempData["ErrorMessage"] = "Current password is incorrect.";
            return RedirectToAction("Index");
        }

        user.Password = HashPassword(newPassword);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Password changed successfully!";
        return RedirectToAction("Index");
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
