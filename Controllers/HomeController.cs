using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismPlatform.Models;

namespace TourismPlatform.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Get popular tours (most booked)
        var popularTours = await _context.Tours
            .Include(t => t.Agency)
            .Include(t => t.Bookings)
            .Where(t => t.StartDate >= DateTime.Now)
            .OrderByDescending(t => t.Bookings!.Count)
            .Take(6)
            .ToListAsync();

        ViewBag.PopularTours = popularTours;

        return View();
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
