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
    public class ReviewsController : Controller
    {
        private readonly Context _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReviewsController(Context context, IHttpContextAccessor httpContextAccessor)
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

        // GET: Reviews
        public async Task<IActionResult> Index()
        {
            var context = _context.Reviews.Include(r => r.User);
            return View(await context.ToListAsync());
        }

        // GET: Reviews/Create
        public IActionResult Create()
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Users");
            }
            return View();
        }

        // POST: Reviews/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,Comment,CreateCommentTime")] Review review)
        {
            if (ModelState.IsValid)
            {
                review.CreateCommentTime = DateTime.Now;
                _context.Add(review);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(review);
        }

        // GET: Reviews/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Users");
            }
            if (review.UserId != GetCurrentUserIdFromSession())
            {
                return RedirectToAction("Index", "Home");
            }
            return View(review);
        }

        // POST: Reviews/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,Comment,CreateCommentTime")] Review review)
        {
            if (id != review.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    review.CreateCommentTime = DateTime.Now;
                    _context.Update(review);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReviewExists(review.Id))
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
            return View(review);
        }

        // GET: Reviews/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Reviews
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (review == null)
            {
                return NotFound();
            }

            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Users");
            }
            if (review.UserId != GetCurrentUserIdFromSession())
            {
                return RedirectToAction("Index", "Home");
            }

            return View(review);
        }

        // POST: Reviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReviewExists(int id)
        {
            return _context.Reviews.Any(e => e.Id == id);
        }
    }
}
