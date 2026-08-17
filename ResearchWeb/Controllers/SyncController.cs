using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResearchWeb.Data;
using ResearchWeb.Models;
using System.Text;
using System.Text.Json;

namespace ResearchWeb.Controllers
{
    public class SyncController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SyncController(ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================================
        // التحقق من المدير
        // =========================================================
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }

        // =========================================================
        // صفحة المزامنة
        // =========================================================

        [HttpGet]
        public IActionResult Index()
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Dashboard");

            return View();
        }


        // =========================================================
        // تصدير جميع الأبحاث إلى JSON
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Export()
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Dashboard");


            var researches = await _context.Researches
                .AsNoTracking()
                .OrderBy(x => x.ID)
                .ToListAsync();


            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };


            string json = JsonSerializer.Serialize(
                researches,
                options
            );


            byte[] bytes = Encoding.UTF8.GetBytes(json);


            string fileName =
                $"ResearchSync_{DateTime.Now:yyyyMMdd_HHmmss}.json";


            return File(
                bytes,
                "application/json",
                fileName
            );
        }


        // =========================================================
        // استيراد ملف المزامنة
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile syncFile)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Dashboard");


            if (syncFile == null || syncFile.Length == 0)
            {
                ViewBag.Message =
                    "❌ لم يتم اختيار ملف المزامنة.";

                return View("Index");
            }


            try
            {
                List<Research2026>? importedResearches;


                using (var stream = syncFile.OpenReadStream())
                {
                    importedResearches =
                        await JsonSerializer.DeserializeAsync<List<Research2026>>(
                            stream,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            }
                        );
                }


                if (importedResearches == null)
                {
                    ViewBag.Message =
                        "❌ ملف المزامنة فارغ أو غير صالح.";

                    return View("Index");
                }


                int added = 0;
                int updated = 0;
                int skipped = 0;


                // =================================================
                // معالجة كل بحث
                // =================================================

                foreach (var source in importedResearches)
                {
                    var existing =
                        await _context.Researches
                            .FirstOrDefaultAsync(x => x.ID == source.ID);


                    // =================================================
                    // السجل غير موجود → إضافة
                    // =================================================

                    if (existing == null)
                    {
                        var newResearch = new Research2026
                        {
                            ID = source.ID,

                            اسم_الباحث =
                                source.اسم_الباحث,

                            تاريخ_الاجتماع =
                                source.تاريخ_الاجتماع,

                            عنوان_البحث =
                                source.عنوان_البحث,

                            رقم_البحث =
                                source.رقم_البحث,

                            رقم_الاجتماع =
                                source.رقم_الاجتماع,

                            نتيجة_البحث =
                                source.نتيجة_البحث,

                            رقم_الهاتف =
                                source.رقم_الهاتف,

                            توصيات_اللجنة =
                                source.توصيات_اللجنة
                        };


                        _context.Researches.Add(newResearch);

                        added++;

                        continue;
                    }


                    // =================================================
                    // السجل موجود → مقارنة جميع الحقول
                    // =================================================

                    bool changed = false;


                    if (existing.اسم_الباحث != source.اسم_الباحث)
                    {
                        existing.اسم_الباحث =
                            source.اسم_الباحث;

                        changed = true;
                    }


                    if (existing.تاريخ_الاجتماع != source.تاريخ_الاجتماع)
                    {
                        existing.تاريخ_الاجتماع =
                            source.تاريخ_الاجتماع;

                        changed = true;
                    }


                    if (existing.عنوان_البحث != source.عنوان_البحث)
                    {
                        existing.عنوان_البحث =
                            source.عنوان_البحث;

                        changed = true;
                    }


                    if (existing.رقم_البحث != source.رقم_البحث)
                    {
                        existing.رقم_البحث =
                            source.رقم_البحث;

                        changed = true;
                    }


                    if (existing.رقم_الاجتماع != source.رقم_الاجتماع)
                    {
                        existing.رقم_الاجتماع =
                            source.رقم_الاجتماع;

                        changed = true;
                    }


                    if (existing.نتيجة_البحث != source.نتيجة_البحث)
                    {
                        existing.نتيجة_البحث =
                            source.نتيجة_البحث;

                        changed = true;
                    }


                    if (existing.رقم_الهاتف != source.رقم_الهاتف)
                    {
                        existing.رقم_الهاتف =
                            source.رقم_الهاتف;

                        changed = true;
                    }


                    if (existing.توصيات_اللجنة != source.توصيات_اللجنة)
                    {
                        existing.توصيات_اللجنة =
                            source.توصيات_اللجنة;

                        changed = true;
                    }


                    if (changed)
                    {
                        updated++;
                    }
                    else
                    {
                        skipped++;
                    }
                }


                // =================================================
                // حفظ التغييرات
                // =================================================

                await _context.SaveChangesAsync();


                ViewBag.Message =
                    $"✅ تمت المزامنة بنجاح<br>" +
                    $"➕ سجلات جديدة: <strong>{added}</strong><br>" +
                    $"🔄 سجلات محدثة: <strong>{updated}</strong><br>" +
                    $"⏭ سجلات بدون تغيير: <strong>{skipped}</strong>";


                return View("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Message =
                    "❌ حدث خطأ أثناء المزامنة:<br>" +
                    ex.Message;

                return View("Index");
            }
        }
    }
}