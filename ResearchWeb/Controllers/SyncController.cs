using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResearchWeb.Data;
using ResearchWeb.Models;

namespace ResearchWeb.Controllers
{
    [ApiController]
    [Route("Sync")]
    public class SyncController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SyncController(ApplicationDbContext context)
        {
            _context = context;
        }


        // =====================================================
        // اختبار الاتصال
        //
        // GET:
        // https://researchweb-mhot.onrender.com/Sync/Test
        // =====================================================

        [HttpGet("Test")]
        public IActionResult Test()
        {
            return Ok("SYNC API يعمل بنجاح");
        }


        // =====================================================
        // استقبال المزامنة من برنامج VB.NET
        //
        // POST:
        // https://researchweb-mhot.onrender.com/Sync/Receive
        // =====================================================

        [HttpPost("Receive")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Receive(
            [FromBody] List<Research2026> researches)
        {
            try
            {
                // =================================================
                // التحقق من البيانات
                // =================================================

                if (researches == null || researches.Count == 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "لا توجد بيانات للمزامنة."
                    });
                }


                // =================================================
                // منع تكرار نفس ID داخل البيانات القادمة
                // =================================================

                researches = researches
                    .GroupBy(x => x.ID)
                    .Select(g => g.First())
                    .ToList();


                int added = 0;
                int updated = 0;
                int skipped = 0;
                int deleted = 0;


                // =================================================
                // IDs الموجودة في Access
                //
                // هذه القائمة هي المصدر الرئيسي للحذف
                // =================================================

                var accessIds = researches
                    .Select(x => x.ID)
                    .ToHashSet();


                // =================================================
                // جلب جميع أبحاث ResearchWeb
                // =================================================

                var existingResearches =
                    await _context.Researches
                        .ToListAsync();


                // =================================================
                // معالجة الإضافة والتحديث
                // =================================================

                foreach (var source in researches)
                {
                    var existing =
                        existingResearches
                            .FirstOrDefault(x => x.ID == source.ID);


                    // =================================================
                    // البحث غير موجود في ResearchWeb
                    // → إضافة
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
                    // البحث موجود
                    // → مقارنة جميع الحقول
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
                // الحذف
                //
                // أي سجل موجود في ResearchWeb
                // وليس موجودًا في Access
                // سيتم حذفه
                // =================================================

                var researchesToDelete =
                    existingResearches
                        .Where(x => !accessIds.Contains(x.ID))
                        .ToList();


                foreach (var research in researchesToDelete)
                {
                    _context.Researches.Remove(research);

                    deleted++;
                }


                // =================================================
                // حفظ جميع التغييرات
                // =================================================

                await _context.SaveChangesAsync();


                // =================================================
                // النتيجة
                // =================================================

                return Ok(new
                {
                    success = true,

                    message =
                        "تمت المزامنة بنجاح. " +
                        "تمت مطابقة ResearchWeb مع قاعدة Access.",

                    added = added,

                    updated = updated,

                    skipped = skipped,

                    deleted = deleted,

                    total = researches.Count
                });
            }
            catch (Exception ex)
            {
                // =================================================
                // في حالة الخطأ
                // =================================================

                return StatusCode(
                    500,
                    new
                    {
                        success = false,

                        message =
                            "حدث خطأ أثناء المزامنة.",

                        error = ex.Message,

                        innerError =
                            ex.InnerException?.Message
                    }
                );
            }
        }
    }
}