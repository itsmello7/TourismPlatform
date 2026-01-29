using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TourismPlatform.Models;

namespace TourismPlatform.Controllers
{
    public class ToursController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ToursController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Tours
        public async Task<IActionResult> Index()
        {
            var tours = await _context.Tours
                .Include(t => t.Agency)
                .OrderByDescending(t => t.StartDate)
                .ToListAsync();
            return View(tours);
        }

        // GET: Tours/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tour = await _context.Tours
                .Include(t => t.Agency)
                .Include(t => t.Bookings)!
                    .ThenInclude(b => b.Feedback)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (tour == null)
            {
                return NotFound();
            }

            return View(tour);
        }

        // GET: Tours/Create
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            if (userId == null || role != "Agency")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // POST: Tours/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,DurationDays,Price,MaxGroupSize,StartDate,EndDate")] Tour tour, IFormFile? ImageFile)
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

            if (tour.EndDate < tour.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after start date");
            }

            if (ModelState.IsValid)
            {
                // Handle image upload
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "tours");
                    Directory.CreateDirectory(uploadsFolder);
                    
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + ImageFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(fileStream);
                    }
                    
                    tour.ImageUrl = "/images/tours/" + uniqueFileName;
                }

                tour.AgencyId = agencyProfile.Id;
                _context.Add(tour);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(MyTours));
            }
            return View(tour);
        }

        // GET: Tours/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            if (userId == null || role != "Agency")
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == null)
            {
                return NotFound();
            }

            var agencyProfile = await _context.AgencyProfiles.FirstOrDefaultAsync(a => a.UserId == userId);
            var tour = await _context.Tours.FindAsync(id);

            if (tour == null || agencyProfile == null || tour.AgencyId != agencyProfile.Id)
            {
                return NotFound();
            }

            return View(tour);
        }

        // POST: Tours/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Title,Description,DurationDays,Price,MaxGroupSize,StartDate,EndDate")] Tour tourModel, IFormFile? ImageFile)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            if (userId == null || role != "Agency")
            {
                return RedirectToAction("Login", "Account");
            }

            if (id <= 0)
            {
                return NotFound();
            }

            var agencyProfile = await _context.AgencyProfiles.FirstOrDefaultAsync(a => a.UserId == userId);
            if (agencyProfile == null)
            {
                return NotFound();
            }

            var tour = await _context.Tours.FindAsync(id);
            if (tour == null || tour.AgencyId != agencyProfile.Id)
            {
                return NotFound();
            }

            if (tourModel.EndDate < tourModel.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after start date");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update tour properties
                    tour.Title = tourModel.Title;
                    tour.Description = tourModel.Description;
                    tour.DurationDays = tourModel.DurationDays;
                    tour.Price = tourModel.Price;
                    tour.MaxGroupSize = tourModel.MaxGroupSize;
                    tour.StartDate = tourModel.StartDate;
                    tour.EndDate = tourModel.EndDate;

                    // Handle image upload
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "tours");
                        Directory.CreateDirectory(uploadsFolder);

                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + ImageFile.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(fileStream);
                        }

                        tour.ImageUrl = "/images/tours/" + uniqueFileName;
                    }

                    _context.Update(tour);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TourExists(tour.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(MyTours));
            }
            return View(tour);
        }

        // GET: Tours/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            if (userId == null || role != "Agency")
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == null)
            {
                return NotFound();
            }

            var agencyProfile = await _context.AgencyProfiles.FirstOrDefaultAsync(a => a.UserId == userId);
            var tour = await _context.Tours
                .Include(t => t.Agency)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (tour == null || agencyProfile == null || tour.AgencyId != agencyProfile.Id)
            {
                return NotFound();
            }

            return View(tour);
        }

        // POST: Tours/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            if (userId == null || role != "Agency")
            {
                return RedirectToAction("Login", "Account");
            }

            var agencyProfile = await _context.AgencyProfiles.FirstOrDefaultAsync(a => a.UserId == userId);
            var tour = await _context.Tours.FindAsync(id);

            if (tour != null && agencyProfile != null && tour.AgencyId == agencyProfile.Id)
            {
                _context.Tours.Remove(tour);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(MyTours));
        }

        // GET: Tours/MyTours - Agency's own tours
        public async Task<IActionResult> MyTours()
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

            var tours = await _context.Tours
                .Where(t => t.AgencyId == agencyProfile.Id)
                .Include(t => t.Bookings)
                .OrderByDescending(t => t.StartDate)
                .ToListAsync();

            return View(tours);
        }

        private bool TourExists(int id)
        {
            return _context.Tours.Any(e => e.Id == id);
        }
    }
}
