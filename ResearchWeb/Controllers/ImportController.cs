using Microsoft.AspNetCore.Mvc;
using ResearchWeb.Data;
using ResearchWeb.Models;
using System.Data.OleDb;
using System.Runtime.Versioning;

namespace ResearchWeb.Controllers
{
    [SupportedOSPlatform("windows")]
    public class ImportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ImportController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }

        public IActionResult Index()
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Dashboard");

            return View();
        }

        [HttpPost]
        public IActionResult ImportAccess(IFormFile accessFile)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Dashboard");

            if (accessFile == null || accessFile.Length == 0)
            {
                ViewBag.Message = "لم يتم اختيار ملف Access";
                return View("Index");
            }

            try
            {
                string folder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "App_Data");

                Directory.CreateDirectory(folder);

                string filePath = Path.Combine(
                    folder,
                    Path.GetFileName(accessFile.FileName));

                using (var stream = new FileStream(
                    filePath,
                    FileMode.Create))
                {
                    accessFile.CopyTo(stream);
                }

                string connectionString =
                    $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};";

                int added = 0;
                int updated = 0;
                int skipped = 0;

                using (OleDbConnection con =
                       new OleDbConnection(connectionString))
                {
                    con.Open();

                    string query =
                        "SELECT * FROM [الابحاث العلمية 2026]";

                    using (OleDbCommand cmd =
                           new OleDbCommand(query, con))

                    using (OleDbDataReader reader =
                           cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id =
                                Convert.ToInt32(reader["ID"]);

                            string researcher =
                                reader["اسم الباحث"] == DBNull.Value
                                ? ""
                                : reader["اسم الباحث"].ToString() ?? "";

                            DateTime? meetingDate =
                                reader["تاريخ الاجتماع"] == DBNull.Value
                                ? null
                                : Convert.ToDateTime(
                                    reader["تاريخ الاجتماع"]);

                            string title =
                                reader["عنوان البحث"] == DBNull.Value
                                ? ""
                                : reader["عنوان البحث"].ToString() ?? "";

                            string researchNumber =
                                reader["رقم البحث"] == DBNull.Value
                                ? ""
                                : reader["رقم البحث"].ToString() ?? "";

                            string meetingNumber =
                                reader["رقم الاجتماع"] == DBNull.Value
                                ? ""
                                : reader["رقم الاجتماع"].ToString() ?? "";

                            string result =
                                reader["نتيجة البحث"] == DBNull.Value
                                ? ""
                                : reader["نتيجة البحث"].ToString() ?? "";

                            string phone =
                                reader["رقم الهاتف"] == DBNull.Value
                                ? ""
                                : reader["رقم الهاتف"].ToString() ?? "";

                            string recommendations =
                                reader["توصيات اللجنة"] == DBNull.Value
                                ? ""
                                : reader["توصيات اللجنة"].ToString() ?? "";

                            // البحث عن البحث بواسطة ID
                            var existing =
                                _context.Researches
                                .FirstOrDefault(x => x.ID == id);

                            // =========================
                            // البحث غير موجود
                            // =========================
                            if (existing == null)
                            {
                                var research = new Research2026
                                {
                                    ID = id,
                                    اسم_الباحث = researcher,
                                    تاريخ_الاجتماع = meetingDate,
                                    عنوان_البحث = title,
                                    رقم_البحث = researchNumber,
                                    رقم_الاجتماع = meetingNumber,
                                    نتيجة_البحث = result,
                                    رقم_الهاتف = phone,
                                    توصيات_اللجنة = recommendations
                                };

                                _context.Researches.Add(research);

                                added++;
                            }
                            else
                            {
                                // =========================
                                // البحث موجود → فحص التعديلات
                                // =========================

                                bool changed = false;

                                if (existing.اسم_الباحث != researcher)
                                {
                                    existing.اسم_الباحث = researcher;
                                    changed = true;
                                }

                                if (existing.تاريخ_الاجتماع != meetingDate)
                                {
                                    existing.تاريخ_الاجتماع = meetingDate;
                                    changed = true;
                                }

                                if (existing.عنوان_البحث != title)
                                {
                                    existing.عنوان_البحث = title;
                                    changed = true;
                                }

                                if (existing.رقم_البحث != researchNumber)
                                {
                                    existing.رقم_البحث = researchNumber;
                                    changed = true;
                                }

                                if (existing.رقم_الاجتماع != meetingNumber)
                                {
                                    existing.رقم_الاجتماع = meetingNumber;
                                    changed = true;
                                }

                                if (existing.نتيجة_البحث != result)
                                {
                                    existing.نتيجة_البحث = result;
                                    changed = true;
                                }

                                if (existing.رقم_الهاتف != phone)
                                {
                                    existing.رقم_الهاتف = phone;
                                    changed = true;
                                }

                                if (existing.توصيات_اللجنة != recommendations)
                                {
                                    existing.توصيات_اللجنة = recommendations;
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
                        }
                    }
                }

                _context.SaveChanges();

                ViewBag.Message =
                    $"تمت إضافة {added} سجل جديد، " +
                    $"وتحديث {updated} سجل، " +
                    $"وتجاهل {skipped} سجل بدون تغيير";

                return View("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Message =
                    "حدث خطأ أثناء الاستيراد: " +
                    ex.Message;

                return View("Index");
            }
        }
    }
}