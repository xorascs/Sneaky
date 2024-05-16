using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Sneaky;
using Sneaky.Classes;
using Sneaky.Models;

namespace Sneaky.Controllers
{
    public class ShoesController : Controller
    {
        private readonly Context _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ShoesController(Context context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private bool IsAdminRole()
        {
            return _httpContextAccessor.HttpContext!.Session.GetString("Role") == "Admin";
        }

        private int? GetCurrentUserIdFromSession()
        {
            return _httpContextAccessor.HttpContext!.Session.GetInt32("Id");
        }

        private bool IsUserLoggedIn()
        {
            return !_httpContextAccessor.HttpContext!.Session.GetString("Login").IsNullOrEmpty();
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                base.OnActionExecuting(context);
                return;
            }

            var sessionLogin = _httpContextAccessor.HttpContext.Session.GetString("Login");
            var sessionRole = _httpContextAccessor.HttpContext.Session.GetString("Role");
            var sessionId = _httpContextAccessor.HttpContext.Session.GetInt32("Id");
            var sessionPassword = _httpContextAccessor.HttpContext.Session.GetString("Password");

            var isValidSession = !string.IsNullOrEmpty(sessionLogin) &&
                                 !string.IsNullOrEmpty(sessionRole) &&
                                 sessionId.HasValue &&
                                 !string.IsNullOrEmpty(sessionPassword);

            if (isValidSession)
            {
                var existingUser = _context.Users.FirstOrDefault(u =>
                    u.Login == sessionLogin &&
                    u.Password == sessionPassword &&
                    u.Id == sessionId);

                if (existingUser != null)
                {
                    ViewBag.Id = sessionId;
                    ViewBag.Login = sessionLogin;
                    ViewBag.Role = sessionRole;
                    return;
                }
            }

            // Clear session if user not found or incomplete session data
            _httpContextAccessor.HttpContext.Session.Remove("Id");
            _httpContextAccessor.HttpContext.Session.Remove("Login");
            _httpContextAccessor.HttpContext.Session.Remove("Role");
            _httpContextAccessor.HttpContext.Session.Remove("Password");

            // Set ViewBag to null if session is not valid
            ViewBag.Id = null;
            ViewBag.Login = null;
            ViewBag.Role = null;

            base.OnActionExecuting(context);
        }

        // GET: Shoes
        public async Task<IActionResult> Index(string? brand, string? name)
        {
            var allBrands = await _context.Brands.ToListAsync();
            ViewData["Brands"] = allBrands;
            var context = _context.Shoes.AsQueryable(); // Start with the IQueryable

            // Apply brand filter if specified
            if (!string.IsNullOrEmpty(brand))
            {
                context = context.Where(s => s.Brand!.Name == brand);
            }

            // Apply name filter if specified
            if (!string.IsNullOrEmpty(name))
            {
                context = context.Where(s => s.Name.Contains(name));
            }

            // Apply Include after filtering
            context = context.Include(s => s.Brand);

            return View(await context.ToListAsync());
        }

        public async Task<IActionResult> AddCommentToShoe(int? id, string reviewText)
        {
            var shoe = await _context.Shoes.FirstOrDefaultAsync(s => s.Id == id);
            if (shoe == null)
                return NotFound("Shoe not found");

            var userId = GetCurrentUserIdFromSession();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return RedirectToAction("Login", "Users");

            if (ModelState.IsValid)
            {
                var review = new ShoeReview
                {
                    UserId = user.Id,
                    Comment = reviewText,
                    CreateCommentTime = DateTime.Now,
                };

                shoe.Reviews.Add(review);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Details", "Shoes", new { id = shoe.Id });
        }

        public async Task<IActionResult> RemoveCommentFromShoe(int? id, int? reviewId, int? userId)
        {
            var shoe = await _context.Shoes.FirstOrDefaultAsync(s => s.Id == id);
            if (shoe == null)
                return NotFound("Shoe not found");

            var review = await _context.ShoeReviews.FirstOrDefaultAsync(r => r.Id == reviewId);
            if (review == null)
                return NotFound("Review not found");

            if (userId != GetCurrentUserIdFromSession())
                return BadRequest("You are not the owner of this message!");

            _context.ShoeReviews.Remove(review);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Shoes", new { id = shoe.Id });
        }   

        public async Task<IActionResult> AddToComparison(int? shoeId)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Users");
            }

            var userId = GetCurrentUserIdFromSession();
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _context.Users.Include(u => u.Comparison).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            if (user.Comparison == null)
            {
                // Create a new Comparison object for the user if it doesn't exist
                user.Comparison = new Comparison();
            }

            var shoe = await _context.Shoes.FirstOrDefaultAsync(s => s.Id == shoeId);
            if (shoe == null)
            {
                return NotFound("Shoe not found in comparison");
            }

            // Ensure that the Shoes property of the Comparison object is initialized
            if (user.Comparison.Shoes == null)
            {
                user.Comparison.Shoes = new List<Shoe>();
            }

            user.Comparison.Shoes.Add(shoe);
            await _context.SaveChangesAsync();

            // Redirect to the details action of Users controller with id from ViewBag
            return RedirectToAction("Details", "Users", new { id = user.Id });
        }

        public async Task<IActionResult> RemoveFromComparison(int? shoeId)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Users");
            }

            var userId = GetCurrentUserIdFromSession();
            var user = await _context.Users.Include(u => u.Comparison).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            if (user.Comparison == null)
            {
                // The user's comparison has not been initialized
                return NotFound("Comparison not found");
            }

            var shoe = await _context.Shoes.FirstOrDefaultAsync(s => s.Id == shoeId);
            if (shoe == null)
            {
                return NotFound("Shoe not found in comparison");
            }

            // Remove the shoe from the comparison
            user.Comparison.Shoes.Remove(shoe);
            await _context.SaveChangesAsync();

            // Redirect to the details action of Users controller with id from ViewBag
            return RedirectToAction("Details", "Users", new { id = user.Id });
        }

        public async Task<IActionResult> AddToFavourites(int? shoeId)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Users");
            }

            var userId = GetCurrentUserIdFromSession();
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _context.Users.Include(u => u.Favourite).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound();
            }

            if (user.Favourite == null)
            {
                // Create a new Favourite object for the user if it doesn't exist
                user.Favourite = new Favourite();
            }

            var shoe = await _context.Shoes.FirstOrDefaultAsync(s => s.Id == shoeId);
            if (shoe == null)
            {
                return NotFound();
            }

            // Ensure that the Favourite property is loaded before accessing Shoes
            if (user.Favourite.Shoes == null)
            {
                user.Favourite.Shoes = new List<Shoe>();
            }

            if (user.Favourite.Shoes.Any(f => f.Id == shoeId))
            {
                return Conflict();
            }

            user.Favourite.Shoes.Add(shoe);
            _context.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Users", new { id = user.Id });
        }


        public async Task<IActionResult> RemoveFromFavourites(int? shoeId)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Users");
            }

            var userId = GetCurrentUserIdFromSession();
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _context.Users.Include(u => u.Favourite).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound();
            }

            var shoe = await _context.Shoes.FirstOrDefaultAsync(s => s.Id == shoeId);
            if (shoe == null)
            {
                return NotFound();
            }

            if (!user.Favourite!.Shoes.Any(f => f.Id == shoeId))
            {
                return Conflict();
            }

            user.Favourite!.Shoes.Remove(shoe);
            _context.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Users", new { id = user.Id });
        }

        // GET: Shoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shoe = await _context.Shoes
                .Include(s => s.Brand)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (shoe == null)
            {
                return NotFound("Shoe not found!");
            }

            var reviews = await _context.ShoeReviews
                .Include(r => r.User)
                .Where(r => r.Shoes.Contains(shoe))
                .ToListAsync();

            shoe.Reviews = reviews;

            return View(shoe);
        }

        // GET: Shoes/Create
        public IActionResult Create()
        {
            if (!IsAdminRole())
            {
                return RedirectToAction("Index", "Home");
            }
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name");
            return View();
        }

        // POST: Shoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,BrandId,Name,Description,Images")] Shoe shoe, List<IFormFile> Photos)
        {
            if (!IsAdminRole())
            {
                return RedirectToAction("Index", "Home");
            }
            if (ModelState.IsValid)
            {
                if (Photos != null && Photos.Count > 0)
                {
                    foreach (var file in Photos.Where(f => f != null && f.Length > 0))
                    {
                        try
                        {
                            string uniqueFileName = GenerateUniqueFileName(file.FileName);
                            string filePath = SaveFileToDisk(file, uniqueFileName);

                            shoe.Images.Add(uniqueFileName);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
                _context.Add(shoe);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name", shoe.BrandId);
            return View(shoe);
        }

        // GET: Shoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdminRole())
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return NotFound();
            }

            var shoe = await _context.Shoes.FindAsync(id);
            if (shoe == null)
            {
                return NotFound();
            }
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name", shoe.BrandId);
            return View(shoe);
        }

        // POST: Shoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BrandId,Name,Description,Images")] Shoe shoe, List<IFormFile> Photos)
        {
            if (!IsAdminRole())
            {
                return RedirectToAction("Index", "Home");
            }

            if (id != shoe.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var origShoe = await _context.Shoes.FirstOrDefaultAsync(c => c.Id == shoe.Id);

                    if (Photos != null && Photos.Count > 0)
                    {
                        foreach (var imagePath in origShoe!.Images)
                        {
                            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "shoesPhotos", imagePath);
                            if (System.IO.File.Exists(filePath))
                            {
                                System.IO.File.Delete(filePath);
                            }
                        }

                        origShoe.Images.Clear();

                        foreach (var file in Photos.Where(f => f != null && f.Length > 0))
                        {
                            try
                            {
                                string uniqueFileName = GenerateUniqueFileName(file.FileName);
                                string filePath = SaveFileToDisk(file, uniqueFileName);

                                origShoe.Images.Add(uniqueFileName);
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }

                    if (origShoe == null)
                    {
                        return NotFound();
                    }

                    origShoe.BrandId = shoe.BrandId;
                    origShoe.Name = shoe.Name;
                    origShoe.Description = shoe.Description;

                    _context.Update(origShoe);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShoeExists(shoe.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name", shoe.BrandId);
            return View(shoe);
        }

        // GET: Shoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsAdminRole())
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return NotFound();
            }

            var shoe = await _context.Shoes
                .Include(s => s.Brand)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (shoe == null)
            {
                return NotFound();
            }

            return View(shoe);
        }

        // POST: Shoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdminRole())
            {
                return RedirectToAction("Index", "Home");
            }

            var shoe = await _context.Shoes.FindAsync(id);
            if (shoe != null)
            {
                _context.Shoes.Remove(shoe);
            }

            foreach (var image in shoe!.Images)
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "shoesPhotos", image);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ShoeExists(int id)
        {
            return _context.Shoes.Any(e => e.Id == id);
        }
        private string GenerateUniqueFileName(string fileName)
        {
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(fileName);
            return uniqueFileName;
        }

        // Helper method to save file to disk
        private string SaveFileToDisk(IFormFile file, string uniqueFileName)
        {
            string uploadsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "shoesPhotos");
            if (!Directory.Exists(uploadsDirectory))
            {
                Directory.CreateDirectory(uploadsDirectory);
            }

            string filePath = Path.Combine(uploadsDirectory, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            return filePath;
        }
    }
}
