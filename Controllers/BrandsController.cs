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
    public class BrandsController : Controller
    {
        private readonly Context _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BrandsController(Context context, IHttpContextAccessor httpContextAccessor)
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

        // GET: Brands
        public async Task<IActionResult> Index()
        {
            if (!IsAdminRole())
            {
                return RedirectToAction("Index", "Home");
            }
            return View(await _context.Brands.ToListAsync());
        }

        // GET: Brands/Create
        public IActionResult Create()
        {
            if (!IsAdminRole())
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Brands/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Brand brand)
        {
            if (!IsAdminRole())
            {
                return RedirectToAction("Index", "Home");
            }
            if (ModelState.IsValid)
            {
                _context.Add(brand);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(brand);
        }

        // GET: Brands/Edit/5
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

            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                return NotFound();
            }
            return View(brand);
        }

        // POST: Brands/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Brand brand)
        {
            if (!IsAdminRole())
            {
                return RedirectToAction("Index", "Home");
            }

            if (id != brand.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(brand);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BrandExists(brand.Id))
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
            return View(brand);
        }

        // GET: Brands/Delete/5
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

            var brand = await _context.Brands
                .FirstOrDefaultAsync(m => m.Id == id);
            if (brand == null)
            {
                return NotFound();
            }

            return View(brand);
        }

        // POST: Brands/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdminRole())
            {
                return RedirectToAction("Index", "Home");
            }

            var brand = await _context.Brands.FindAsync(id);
            if (brand != null)
            {
                _context.Brands.Remove(brand);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BrandExists(int id)
        {
            return _context.Brands.Any(e => e.Id == id);
        }
    }
}
