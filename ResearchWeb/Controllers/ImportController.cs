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



        // التحقق من المدير
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }




        // صفحة الاستيراد
        public IActionResult Index()
        {

            if (!IsAdmin())
                return RedirectToAction(
                    "Index",
                    "Dashboard"
                );


            return View();

        }





        // استيراد Access
        [HttpPost]
        public IActionResult ImportAccess(IFormFile accessFile)
        {

            if (!IsAdmin())
                return RedirectToAction(
                    "Index",
                    "Dashboard"
                );



            if (accessFile == null)
            {

                ViewBag.Message =
                    "لم يتم اختيار ملف Access";


                return View("Index");

            }





            string folder =
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "App_Data"
                );



            Directory.CreateDirectory(folder);




            string filePath =
                Path.Combine(
                    folder,
                    accessFile.FileName
                );





            using (var stream =
                   new FileStream(
                       filePath,
                       FileMode.Create))
            {

                accessFile.CopyTo(stream);

            }






            string connectionString =
                $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};";




            int added = 0;
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
                            Convert.ToInt32(
                                reader["ID"]
                            );



                        string researcher =
                            reader["اسم الباحث"]?.ToString()
                            ?? "";



                        string title =
                            reader["عنوان البحث"]?.ToString()
                            ?? "";





                        bool exists =
                            _context.Researches.Any(x =>
                                x.ID == id ||
                                (
                                    x.اسم_الباحث == researcher &&
                                    x.عنوان_البحث == title
                                )
                            );





                        if (exists)
                        {

                            skipped++;

                            continue;

                        }






                        var research =
                            new Research2026
                            {

                                ID = id,


                                اسم_الباحث = researcher,



                                تاريخ_الاجتماع =
                                    reader["تاريخ الاجتماع"] == DBNull.Value
                                    ? null
                                    : Convert.ToDateTime(
                                        reader["تاريخ الاجتماع"]
                                    ),




                                عنوان_البحث = title,



                                رقم_البحث =
                                    reader["رقم البحث"]?.ToString()
                                    ?? "",



                                رقم_الاجتماع =
                                    reader["رقم الاجتماع"]?.ToString()
                                    ?? "",




                                نتيجة_البحث =
                                    reader["نتيجة البحث"]?.ToString()
                                    ?? "",




                                رقم_الهاتف =
                                    reader["رقم الهاتف"]?.ToString()
                                    ?? "",




                                توصيات_اللجنة =
                                    reader["توصيات اللجنة"]?.ToString()
                                    ?? ""

                            };




                        _context.Researches.Add(research);


                        added++;


                    }


                }


            }






            _context.SaveChanges();






            ViewBag.Message =
                $"تمت إضافة {added} سجل جديد، وتم تجاهل {skipped} سجل موجود مسبقاً";




            return View("Index");


        }


    }
}