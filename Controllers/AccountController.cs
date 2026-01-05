using Microsoft.AspNetCore.Mvc;
using FinanceBudgetApp.Models;
using FinanceBudgetApp.Services;

namespace FinanceBudgetApp.Controllers;

public class AccountController : Controller
{
    private readonly AuthService _authService;

    public AccountController(AuthService authService)
    {
        _authService = authService;
    }

    // GET: Account/Register
    public IActionResult Register()
    {
        // If already logged in, redirect to dashboard
        if (_authService.IsLoggedIn())
        {
            return RedirectToAction("Index", "Dashboard");
        }
        return View();
    }

    // POST: Account/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.RegisterAsync(model.Username, model.Email, model.Password);

        if (result.Success && result.User != null)
        {
            _authService.SetUserSession(result.User);
            TempData["SuccessMessage"] = "Registration successful! Welcome to Finance Budget Tracker.";
            return RedirectToAction("Index", "Dashboard");
        }

        ModelState.AddModelError("", result.Message);
        return View(model);
    }

    // GET: Account/Login
    public IActionResult Login()
    {
        // If already logged in, redirect to dashboard
        if (_authService.IsLoggedIn())
        {
            return RedirectToAction("Index", "Dashboard");
        }
        return View();
    }

    // POST: Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.LoginAsync(model.Username, model.Password);

        if (result.Success && result.User != null)
        {
            _authService.SetUserSession(result.User);
            TempData["SuccessMessage"] = "Welcome back!";
            return RedirectToAction("Index", "Dashboard");
        }

        ModelState.AddModelError("", result.Message);
        return View(model);
    }

    // POST: Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        _authService.Logout();
        TempData["SuccessMessage"] = "You have been logged out successfully.";
        return RedirectToAction("Login");
    }
}

