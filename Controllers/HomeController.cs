using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FinanceBudgetApp.Models;
using FinanceBudgetApp.Services;

namespace FinanceBudgetApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AuthService _authService;

    public HomeController(ILogger<HomeController> logger, AuthService authService)
    {
        _logger = logger;
        _authService = authService;
    }

    public IActionResult Index()
    {
        // If user is logged in, redirect to dashboard (we'll create this next)
        if (_authService.IsLoggedIn())
        {
            return RedirectToAction("Index", "Dashboard");
        }
        // Otherwise, redirect to login
        return RedirectToAction("Login", "Account");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
