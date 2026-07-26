using Microsoft.AspNetCore.Mvc;
using ResearchWeb.Data;
using System.Text.Json;

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



            // عدد الأبحاث الكلي
            ViewBag.Research2026Count =
                _context.Researches.Count();



            // عدد الباحثين
            ViewBag.ResearchersCount =
                _context.Researches
                .Where(x => x.اسم_الباحث != null)
                .Select(x => x.اسم_الباحث)
                .Distinct()
                .Count();



            // عدد نتائج البحث المختلفة
            ViewBag.ResultsCount =
                _context.Researches
                .Where(x => x.نتيجة_البحث != null)
                .Select(x => x.نتيجة_البحث)
                .Distinct()
                .Count();



            // عدد الاجتماعات
            ViewBag.MeetingsCount =
                _context.Researches
                .Where(x => x.رقم_الاجتماع != null)
                .Select(x => x.رقم_الاجتماع)
                .Distinct()
                .Count();



            // عدد المستخدمين
            ViewBag.UsersCount =
                _context.Users.Count();





            // ==========================
            // رسم نتائج الأبحاث
            // ==========================

            var resultsStatistics =
                _context.Researches
                .Where(x => x.نتيجة_البحث != null)
                .GroupBy(x => x.نتيجة_البحث)
                .Select(x => new
                {
                    Result = x.Key,
                    Count = x.Count()
                })
                .ToList();



            ViewBag.ResultLabels =
                JsonSerializer.Serialize(
                    resultsStatistics.Select(x => x.Result)
                );


            ViewBag.ResultValues =
                JsonSerializer.Serialize(
                    resultsStatistics.Select(x => x.Count)
                );







            // ==========================
            // رسم الأبحاث حسب رقم الاجتماع
            // ==========================

            var meetingStatistics =
                _context.Researches
                .Where(x => x.رقم_الاجتماع != null)
                .GroupBy(x => x.رقم_الاجتماع)
                .Select(x => new
                {
                    Meeting = x.Key,
                    Count = x.Count()
                })
                .OrderBy(x => x.Meeting)
                .ToList();



            ViewBag.MeetingLabels =
                JsonSerializer.Serialize(
                    meetingStatistics.Select(x => x.Meeting)
                );



            ViewBag.MeetingValues =
                JsonSerializer.Serialize(
                    meetingStatistics.Select(x => x.Count)
                );








            // آخر 10 أبحاث

            var latestResearches =
                _context.Researches
                .OrderByDescending(x => x.ID)
                .Take(10)
                .ToList();




            return View(latestResearches);

        }
        public IActionResult Committee()
        {
            return View();
        }

    }
}