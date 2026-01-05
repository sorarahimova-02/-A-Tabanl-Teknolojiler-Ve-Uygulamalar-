using System.Security.Cryptography;
using System.Text;
using FinanceBudgetApp.Data;
using FinanceBudgetApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceBudgetApp.Services;

public class AuthService
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    // Hash password using SHA256 (mevcut mantığını koruyoruz)
    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    // Register a new user (DB)
    public async Task<(bool Success, string Message, User? User)> RegisterAsync(string username, string email, string password)
    {
        username = (username ?? "").Trim();
        email = (email ?? "").Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return (false, "Username, email and password are required", null);

        // Username already exists?
        if (await _db.Users.AnyAsync(u => u.Username == username))
            return (false, "Username already exists", null);

        // Email already exists?
        if (await _db.Users.AnyAsync(u => u.Email == email))
            return (false, "Email already exists", null);

        var user = new User
        {
            Username = username,
            Email = email,
            Password = HashPassword(password),
            CreatedDate = DateTime.Now
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(); // burada user.Id oluşur

        // Create default categories for the user (DbContext içinde var)
        //await _db.CreateDefaultCategoriesForUserAsync(user.Id);

        return (true, "Registration successful", user);
    }

    // Login user (DB)
    public async Task<(bool Success, string Message, User? User)> LoginAsync(string username, string password)
    {
        username = (username ?? "").Trim();
        password = password ?? "";

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
            return (false, "Invalid username or password", null);

        var hashedPassword = HashPassword(password);
        if (user.Password != hashedPassword)
            return (false, "Invalid username or password", null);

        return (true, "Login successful", user);
    }

    // Set user session
    public void SetUserSession(User user)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            httpContext.Session.SetInt32("UserId", user.Id);
            httpContext.Session.SetString("Username", user.Username);
        }
    }

    // Get current user from session
    public int? GetCurrentUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.Session.GetInt32("UserId");
    }

    public string? GetCurrentUsername()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.Session.GetString("Username");
    }

    public bool IsLoggedIn() => GetCurrentUserId().HasValue;

    public void Logout()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        httpContext?.Session.Clear();
    }
}
