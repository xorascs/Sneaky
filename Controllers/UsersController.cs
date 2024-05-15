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
    public class UsersController : Controller
    {
        private readonly Context _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsersController(Context context, IHttpContextAccessor httpContextAccessor)
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

        public IActionResult LogOut()
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                _httpContextAccessor.HttpContext.Session.Remove("Id");
                _httpContextAccessor.HttpContext.Session.Remove("Login");
                _httpContextAccessor.HttpContext.Session.Remove("Role");
                _httpContextAccessor.HttpContext.Session.Remove("Password");
                ViewData["Login"] = null;
                ViewData["Role"] = null;
            }
            return RedirectToAction("Index", "Home");
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            if (!IsAdminRole())
            {
                return RedirectToAction("Index", "Home");
            }
            return View(await _context.Users.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Comparison)
                    .ThenInclude(c => c!.Shoes)
                        .ThenInclude(s => s.Brand)
                .Include(u => u.Favourite)
                    .ThenInclude(c => c!.Shoes)
                        .ThenInclude(s => s.Brand)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        public IActionResult Login()
        {
            if (!IsUserLoggedIn())
            {
                return View();
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("Id,Login,Password")] User user)
        {
            if (ModelState.IsValid)
            {
                var isUserExist = await _context.Users.FirstOrDefaultAsync(
                    u => u.Login == user.Login &&
                    u.Password == user.Password);

                if (isUserExist == null)
                {
                    var userData = new UserFormViewModel
                    {
                        Login = user.Login,
                        Password = user.Password,
                    };
                    ModelState.AddModelError("Login", "Incorrect login or password.");
                    return View(userData);
                }

                if (isUserExist != null && _httpContextAccessor.HttpContext != null)
                {
                    _httpContextAccessor.HttpContext.Session.SetInt32("Id", isUserExist.Id);
                    _httpContextAccessor.HttpContext.Session.SetString("Login", isUserExist.Login);
                    _httpContextAccessor.HttpContext.Session.SetString("Role", isUserExist.Role.ToString());
                    _httpContextAccessor.HttpContext.Session.SetString("Password", isUserExist.Password);
                    return RedirectToAction("Index", "Home");
                }
            }
            return View();
        }

        public IActionResult Register()
        {
            if (!IsUserLoggedIn())
            {
                return View();
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("Id,Login,Password")] User user)
        {
            if (ModelState.IsValid)
            {
                var isUserExist = await _context.Users.FirstOrDefaultAsync(
                    u => u.Login == user.Login);

                if (isUserExist != null)
                {
                    var userData = new UserFormViewModel
                    {
                        Login = user.Login,
                        Password = user.Password,
                    };
                    ModelState.AddModelError("Login", "User with this login is already exist.");
                    return View(userData);
                }

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _httpContextAccessor.HttpContext!.Session.SetInt32("Id", user.Id);
                _httpContextAccessor.HttpContext.Session.SetString("Login", user.Login);
                _httpContextAccessor.HttpContext.Session.SetString("Role", user.Role.ToString());
                _httpContextAccessor.HttpContext.Session.SetString("Password", user.Password);
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            if (!IsAdminRole())
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Role,Login,Password")] User user)
        {
            if (!IsAdminRole())
            {
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                var isUserExist = await _context.Users.FirstOrDefaultAsync(
                    u => u.Login == user.Login);

                if (isUserExist != null)
                {
                    ModelState.AddModelError("Login", "User with this login is already exist.");
                    return View(user);
                }

                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (!IsAdminRole() && user.Id != GetCurrentUserIdFromSession())
            {
                return RedirectToAction("Index", "Home");
            }

            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Role,Login,Password")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var isUserExist = await _context.Users.FirstOrDefaultAsync(
                    u => u.Login == user.Login);

                    if (isUserExist != null)
                    {
                        ModelState.AddModelError("Login", "User with this login is already exist.");
                        return View(user);
                    }

                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
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
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            if (!IsAdminRole() && user.Id != GetCurrentUserIdFromSession())
            {
                return RedirectToAction("Index", "Home");
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
