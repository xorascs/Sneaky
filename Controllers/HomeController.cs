using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Sneaky.Models;
using System.Diagnostics;

namespace Sneaky.Controllers
{
    public class HomeController : Controller
    {
        private readonly Context _context;
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(Context context, ILogger<HomeController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                ViewBag.Login = _httpContextAccessor.HttpContext.Session.GetString("Login");
                ViewBag.Role = _httpContextAccessor.HttpContext.Session.GetString("Role");
                ViewBag.Id = _httpContextAccessor.HttpContext.Session.GetInt32("Id");
            }

            base.OnActionExecuting(context);
        }

        public async Task<IActionResult> Index()
        {
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .OrderByDescending(r => r.CreateCommentTime)
                .Take(10)
                .ToListAsync();
            ViewData["Reviews"] = reviews;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
