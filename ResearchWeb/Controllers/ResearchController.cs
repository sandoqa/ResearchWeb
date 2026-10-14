
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResearchWeb.Data;
using ResearchWeb.Models;
using ClosedXML.Excel;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace ResearchWeb.Controllers
{
    public class ResearchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ResearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        // التحقق من صلاحية المدير
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }

        // =====================================================
        // عرض الأبحاث + البحث
        // =====================================================
        public IActionResult Index(string search)
        {
            var researches = _context.Researches.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string text = search.Trim();

                researches = researches.Where(x =>
                    (x.اسم_الباحث != null &&
                     x.اسم_الباحث.Contains(text)) ||

                    (x.عنوان_البحث != null &&
                     x.عنوان_البحث.Contains(text)) ||

                    (x.رقم_البحث != null &&
                     x.رقم_البحث.Contains(text)) ||

                    (x.رقم_الهاتف != null &&
                     x.رقم_الهاتف.Contains(text))
                );
            }

            var result = researches
                .AsEnumerable()
                .OrderBy(x =>
                    int.TryParse(x.رقم_الاجتماع, out int meeting)
                    ? meeting
                    : int.MaxValue)
                .ThenBy(x =>
                    int.TryParse(x.رقم_البحث, out int researchNo)
                    ? researchNo
                    : int.MaxValue)
                .ToList();

            return View(result);
        }

        // =====================================================
        // فحص الأبحاث المكررة حسب عنوان البحث
        // =====================================================
        public IActionResult Duplicates()
        {
            var duplicates = _context.Researches
                .Where(x => x.عنوان_البحث != null)
                .AsEnumerable()
                .GroupBy(x => x.عنوان_البحث.Trim())
                .Where(g => g.Count() > 1)
                .SelectMany(g => g)
                .OrderBy(x => x.عنوان_البحث)
                .ToList();

            return View(duplicates);
        }

        // =====================================================
        // صفحة إضافة بحث
        // =====================================================
        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdmin())
                return RedirectToAction("Index");

            return View();
        }

        // =====================================================
        // حفظ بحث جديد
        // =====================================================
        [HttpPost]
        public IActionResult Create(Research2026 research)
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

        // =====================================================
        // صفحة تعديل بحث
        // =====================================================
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

        // =====================================================
        // حفظ تعديل البحث
        // =====================================================
        [HttpPost]
        public IActionResult Edit(Research2026 research)
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

        // =====================================================
        // حذف البحث
        // =====================================================
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

        // =====================================================
        // البحث حسب رقم اللجنة (رقم الاجتماع)
        // =====================================================
        [HttpGet]
        public IActionResult ByMeeting(string meetingNumber)
        {
            var researches = new List<Research2026>();

            if (!string.IsNullOrWhiteSpace(meetingNumber))
            {
                string number = meetingNumber.Trim();

                researches = _context.Researches
                    .ToList()
                    .Where(x =>
                        x.رقم_الاجتماع != null &&
                        x.رقم_الاجتماع.Trim().Contains(number))
                    .ToList();
            }

            ViewBag.MeetingNumber = meetingNumber;

            return View(researches);
        }

        // =====================================================
        // عرض الأبحاث غير المقبولة
        // =====================================================
        public IActionResult Rejected()
        {
            var rejectedResearches = _context.Researches
                .Where(x => x.نتيجة_البحث == "1")
                .OrderByDescending(x => x.ID)
                .ToList();

            return View(rejectedResearches);
        }

        // =====================================================
        // تصدير الأبحاث إلى Excel
        // =====================================================
        [HttpGet]
        public IActionResult ExportExcel(string search)
        {
            var researches = _context.Researches.AsQueryable();

            // تطبيق البحث الموجود في الصفحة
            if (!string.IsNullOrWhiteSpace(search))
            {
                string text = search.Trim();

                researches = researches.Where(x =>
                    (x.اسم_الباحث != null &&
                     x.اسم_الباحث.Contains(text)) ||

                    (x.عنوان_البحث != null &&
                     x.عنوان_البحث.Contains(text)) ||

                    (x.رقم_البحث != null &&
                     x.رقم_البحث.Contains(text)) ||

                    (x.رقم_الهاتف != null &&
                     x.رقم_الهاتف.Contains(text))
                );
            }

            // ترتيب الأبحاث حسب رقم اللجنة ثم رقم البحث
            var result = researches
                .AsEnumerable()
                .OrderBy(x =>
                    int.TryParse(x.رقم_الاجتماع, out int meeting)
                    ? meeting
                    : int.MaxValue)
                .ThenBy(x =>
                    int.TryParse(x.رقم_البحث, out int researchNo)
                    ? researchNo
                    : int.MaxValue)
                .ToList();

            using var workbook = new XLWorkbook();

            var worksheet = workbook.Worksheets.Add(
                "الأبحاث العلمية 2026");

            // عناوين الأعمدة
            string[] headers =
            {
                "ID",
                "اسم الباحث",
                "تاريخ الاجتماع",
                "عنوان البحث",
                "رقم البحث",
                "رقم اللجنة",
                "نتيجة البحث",
                "رقم الهاتف",
                "توصيات اللجنة"
            };

            for (int col = 0; col < headers.Length; col++)
            {
                worksheet.Cell(1, col + 1).Value = headers[col];
            }

            // تنسيق صف العناوين
            var headerRange = worksheet.Range(
                1, 1, 1, headers.Length);

            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.FontSize = 12;
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Fill.BackgroundColor = XLColor.DarkBlue;

            headerRange.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            headerRange.Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;

            // إدخال بيانات الأبحاث
            for (int i = 0; i < result.Count; i++)
            {
                var item = result[i];
                int row = i + 2;

                worksheet.Cell(row, 1).Value = item.ID;
                worksheet.Cell(row, 2).Value =
                    item.اسم_الباحث ?? "";

                if (item.تاريخ_الاجتماع.HasValue)
                {
                    worksheet.Cell(row, 3).Value =
                        item.تاريخ_الاجتماع.Value;

                    worksheet.Cell(row, 3).Style.DateFormat.Format =
                        "dd/MM/yyyy";
                }

                worksheet.Cell(row, 4).Value =
                    item.عنوان_البحث ?? "";

                worksheet.Cell(row, 5).Value =
                    item.رقم_البحث ?? "";

                worksheet.Cell(row, 6).Value =
                    item.رقم_الاجتماع ?? "";

                worksheet.Cell(row, 7).Value =
                    item.نتيجة_البحث ?? "";

                worksheet.Cell(row, 8).Value =
                    item.رقم_الهاتف ?? "";

                worksheet.Cell(row, 9).Value =
                    item.توصيات_اللجنة ?? "";
            }

            // تنسيق الجدول
            int lastRow = result.Count + 1;

            var usedRange = worksheet.Range(
                1, 1, lastRow, headers.Length);

            usedRange.Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;

            usedRange.Style.Alignment.WrapText = true;

            usedRange.Style.Border.OutsideBorder =
                XLBorderStyleValues.Thin;

            usedRange.Style.Border.InsideBorder =
                XLBorderStyleValues.Thin;

            // اتجاه الورقة من اليمين إلى اليسار
            worksheet.RightToLeft = true;

            // ارتفاع صف العناوين
            worksheet.Row(1).Height = 30;

            // ضبط عرض الأعمدة تلقائياً
            worksheet.Columns().AdjustToContents();

            // تحديد عرض الأعمدة الطويلة
            worksheet.Column(2).Width =
                Math.Min(Math.Max(worksheet.Column(2).Width, 18), 30);

            worksheet.Column(4).Width =
                Math.Min(Math.Max(worksheet.Column(4).Width, 25), 50);

            worksheet.Column(9).Width =
                Math.Min(Math.Max(worksheet.Column(9).Width, 25), 50);

            worksheet.Column(8).Width =
                Math.Min(Math.Max(worksheet.Column(8).Width, 15), 22);

            // تثبيت صف العناوين عند التمرير
            worksheet.SheetView.FreezeRows(1);

            // إضافة أسهم التصفية
            worksheet.Range(1, 1, lastRow, headers.Length)
                .SetAutoFilter();

            // إنشاء ملف Excel في الذاكرة
            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            string fileName =
                $"الأبحاث_العلمية_2026_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
    }
}
