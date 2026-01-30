using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismPlatform.Models;

namespace TourismPlatform.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reports
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            if (userId == null || role != "Agency")
            {
                return RedirectToAction("Login", "Account");
            }

            var agencyProfile = await _context.AgencyProfiles.FirstOrDefaultAsync(a => a.UserId == userId);
            if (agencyProfile == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get booking statistics
            var bookings = await _context.Bookings
                .Include(b => b.Tour)
                .Where(b => b.Tour!.AgencyId == agencyProfile.Id)
                .ToListAsync();

            var report = new
            {
                TotalBookings = bookings.Count,
                PendingBookings = bookings.Count(b => b.Status == "Pending"),
                ConfirmedBookings = bookings.Count(b => b.Status == "Confirmed"),
                CompletedBookings = bookings.Count(b => b.Status == "Completed"),
                PaidBookings = bookings.Count(b => b.PaymentStatus == "Paid"),
                UnpaidBookings = bookings.Count(b => b.PaymentStatus == "Unpaid"),
                TotalRevenue = bookings.Where(b => b.PaymentStatus == "Paid").Sum(b => b.Tour?.Price ?? 0)
            };

            return View(report);
        }
    }
}
