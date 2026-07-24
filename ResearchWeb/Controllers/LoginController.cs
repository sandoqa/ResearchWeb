using Microsoft.AspNetCore.Mvc;
using ResearchWeb.Data;

namespace ResearchWeb.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }


        // صفحة تسجيل الدخول
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        // فحص بيانات الدخول
        [HttpPost]
        public IActionResult Index(string username, string password)
        {
            var user = _context.Users
                .FirstOrDefault(x =>
                    x.Username == username &&
                    x.Password == password
                );


            if (user != null)
            {
                HttpContext.Session.SetString(
                    "Username",
                    user.Username ?? ""
                );


                HttpContext.Session.SetString(
                    "Role",
                    user.Role ?? ""
                );


                return RedirectToAction(
                    "Index",
                    "Dashboard"
                );
            }


            ViewBag.Error = "اسم المستخدم أو كلمة المرور غير صحيحة";

            return View();
        }



        // تسجيل الخروج
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Index");
        }
    }
}