using Microsoft.AspNetCore.Mvc;
using ResearchWeb.Data;

namespace ResearchWeb.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }


        public IActionResult Index()
        {
            ViewBag.Username =
                HttpContext.Session.GetString("Username");

            ViewBag.Role =
                HttpContext.Session.GetString("Role");



            // عدد أبحاث 2026
            ViewBag.Research2026Count =
                _context.Researches.Count();



            // عدد المستخدمين
            ViewBag.UsersCount =
                _context.Users.Count();



            return View();
        }
    }
}