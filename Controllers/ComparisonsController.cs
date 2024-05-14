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

namespace Sneaky.Controllers
{
    public class ComparisonsController : Controller
    {
        private readonly Context _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ComparisonsController(Context context, IHttpContextAccessor httpContextAccessor)
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

        // GET: Comparisons
        public async Task<IActionResult> Index()
        {
            var context = _context.Comparisons
                .Include(c => c.User)
                .Include(c => c.ShoesList)
                    .ThenInclude(cl => cl.Brand);
            return View(await context.ToListAsync());
        }

        // GET: Comparisons/Create
        public IActionResult Create()
        {
            var allShoes = _context.Shoes.Select(s => new
            {
                Id = s.Id,
                DisplayText = $"{s.Brand.Name} - {s.Name}"
            }).ToList();

            ViewData["ComparisonList"] = new SelectList(allShoes, "Id", "DisplayText");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Login");

            return View();
        }


        // POST: Comparisons/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,ShoesList")] Comparison comparison, int[] shoeList)
        {
            var allShoes = _context.Shoes.Select(s => new
            {
                Id = s.Id,
                DisplayText = $"{s.Brand.Name} - {s.Name}"
            }).ToList();

            if (ModelState.IsValid)
            {
                // Set ComparisonList based on selected shoe IDs
                comparison.ShoesList = _context.Shoes.Where(s => shoeList.Contains(s.Id)).ToList();

                if (comparison.ShoesList.Count < 2)
                {
                    ModelState.AddModelError("ShoesList", "Pick at least 2 shoes");

                    ViewData["ComparisonList"] = new SelectList(allShoes, "Id", "DisplayText");
                    ViewData["UserId"] = new SelectList(_context.Users, "Id", "Login", comparison.UserId);
                    return View(comparison);
                }

                _context.Add(comparison);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ComparisonList"] = new SelectList(allShoes, "Id", "DisplayText");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Login", comparison.UserId);
            return View(comparison);
        }

        // GET: Comparisons/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comparison = await _context.Comparisons.FindAsync(id);
            if (comparison == null)
            {
                return NotFound();
            }
            var shoeList = _context.Shoes.Select(s => new
            {
                Id = s.Id,
                DisplayText = $"{s.Brand.Name} - {s.Name}" // Assuming Brand is a navigation property in Shoe class
            }).ToList();

            ViewData["ComparisonList"] = new SelectList(shoeList, "Id", "DisplayText", comparison.ShoesList);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Login", comparison.UserId);
            return View(comparison);
        }

        // POST: Comparisons/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,ShoesList")] Comparison comparison, int[] shoeList)
        {
            if (id != comparison.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Set ComparisonList based on selected shoe IDs
                    comparison.ShoesList = _context.Shoes.Where(s => shoeList.Contains(s.Id)).ToList();

                    _context.Update(comparison);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ComparisonExists(comparison.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ComparisonList"] = new SelectList(shoeList, "Id", "DisplayText", comparison.ShoesList);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Login", comparison.UserId);
            return View(comparison);
        }

        // GET: Comparisons/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comparison = await _context.Comparisons
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comparison == null)
            {
                return NotFound();
            }

            return View(comparison);
        }

        // POST: Comparisons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comparison = await _context.Comparisons.FindAsync(id);
            if (comparison != null)
            {
                _context.Comparisons.Remove(comparison);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ComparisonExists(int id)
        {
            return _context.Comparisons.Any(e => e.Id == id);
        }
    }
}
