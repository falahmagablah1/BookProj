using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    // HomeController يبقى مفتوح للجميع (بدون [Authorize])
    // هذا يسمح بالوصول المجهول للصفحة الرئيسية وصفحة الخصوصية
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // الصفحة الرئيسية - مفتوحة للجميع
        public IActionResult Index()
        {
            return View();
        }

        // صفحة الخصوصية - مفتوحة للجميع
        public IActionResult Privacy()
        {
            return View();
        }

        // صفحة الخطأ - مفتوحة للجميع
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}