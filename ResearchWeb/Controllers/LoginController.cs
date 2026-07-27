using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
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
            // إذا كان هناك Cookie محفوظة
            var username = Request.Cookies["RememberUsername"];
            var role = Request.Cookies["RememberRole"];


            if (!string.IsNullOrEmpty(username) &&
                !string.IsNullOrEmpty(role))
            {
                HttpContext.Session.SetString(
                    "Username",
                    username
                );


                HttpContext.Session.SetString(
                    "Role",
                    role
                );


                return RedirectToAction(
                    "Index",
                    "Dashboard"
                );
            }


            return View();
        }





        // فحص الدخول
        [HttpPost]
        public IActionResult Index(
            string username,
            string password,
            bool rememberMe = false)
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



                // حفظ الدخول لمدة 30 يوم
                if (rememberMe)
                {

                    CookieOptions options = new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(30),
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None
                    };


                    Response.Cookies.Append(
                        "RememberUsername",
                        user.Username ?? "",
                        options
                    );


                    Response.Cookies.Append(
                        "RememberRole",
                        user.Role ?? "",
                        options
                    );

                }


                return RedirectToAction(
                    "Index",
                    "Dashboard"
                );
            }



            ViewBag.Error =
                "اسم المستخدم أو كلمة المرور غير صحيحة";


            return View();
        }





        // تسجيل الخروج
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();


            Response.Cookies.Delete(
                "RememberUsername"
            );


            Response.Cookies.Delete(
                "RememberRole"
            );


            return RedirectToAction("Index");
        }

    }
}