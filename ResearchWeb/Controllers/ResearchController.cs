using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResearchWeb.Data;
using ResearchWeb.Models;

namespace ResearchWeb.Controllers
{
    public class ResearchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ResearchController(ApplicationDbContext context)
        {
            _context = context;
        }


        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }


        // عرض الأبحاث + البحث
        public IActionResult Index(string search)
        {
            var researches = _context.Researches.AsQueryable();


            if (!string.IsNullOrWhiteSpace(search))
            {
                string text = search.Trim();

                researches = researches.Where(x =>
                    (x.اسم_الباحث != null && x.اسم_الباحث.Contains(text)) ||
                    (x.عنوان_البحث != null && x.عنوان_البحث.Contains(text)) ||
                    (x.رقم_البحث != null && x.رقم_البحث.Contains(text)) ||
                    (x.رقم_الهاتف != null && x.رقم_الهاتف.Contains(text))
                );
            }

            researches = researches
    .AsEnumerable()
    .OrderBy(x =>
        int.TryParse(x.رقم_الاجتماع, out int meeting)
        ? meeting
        : int.MaxValue)
    .ThenBy(x =>
        int.TryParse(x.رقم_البحث, out int researchNo)
        ? researchNo
        : int.MaxValue)
    .AsQueryable();

            return View(researches.ToList());
        }



        // صفحة إضافة بحث
        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdmin())
                return RedirectToAction("Index");

            return View();
        }



        // حفظ بحث جديد
        [HttpPost]
        public IActionResult Create(Research research)
        {
            if (!IsAdmin())
                return RedirectToAction("Index");


            try
            {
                _context.Researches.Add(research);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content("خطأ أثناء الإضافة: " + ex.Message);
            }
        }




        // صفحة تعديل بحث
        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Index");


            var research = _context.Researches
                .FirstOrDefault(x => x.ID == id);


            if (research == null)
                return Content("لم يتم العثور على البحث");


            return View(research);
        }





        // حفظ تعديل البحث
        [HttpPost]
        public IActionResult Edit(Research research)
        {
            if (!IsAdmin())
                return RedirectToAction("Index");


            try
            {
                _context.Researches.Update(research);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content("خطأ أثناء التعديل: " + ex.Message);
            }
        }





        // حذف البحث
        [HttpGet]
        public IActionResult Delete(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Index");


            try
            {
                var research = _context.Researches
                    .FirstOrDefault(x => x.ID == id);


                if (research == null)
                    return Content("لم يتم العثور على السجل");


                _context.Researches.Remove(research);
                _context.SaveChanges();


                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content("خطأ أثناء الحذف: " + ex.Message);
            }
        }
    }
}