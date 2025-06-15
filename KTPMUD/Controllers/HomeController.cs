using KTPMUD.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization; // <-- THÊM DÒNG NÀY

namespace KTPMUD.Controllers
{
    [Authorize] // <-- THÊM THUỘC TÍNH NÀY
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Sau khi đăng nhập, chuyển hướng người dùng dựa trên vai trò của họ
            if (User.IsInRole("BacSi"))
            {
                // Nếu là Bác sĩ, chuyển đến trang danh sách bệnh nhân
                return RedirectToAction("DanhSach", "BenhNhan");
            }
            if (User.IsInRole("BenhNhan"))
            {
                // Nếu là Bệnh nhân, chuyển đến trang đặt lịch hẹn
                return RedirectToAction("Create", "LichHen");
            }

            // Nếu có vai trò khác (như Admin) hoặc mặc định
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
