using KTPMUD.Data;
using KTPMUD.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KTPMUD.Controllers
{
    [Authorize] // Yêu cầu phải đăng nhập
    public class ThanhToanVienPhiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ThanhToanVienPhiController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: ThanhToanVienPhi
        // Hiển thị danh sách hóa đơn dựa trên vai trò
        public async Task<IActionResult> Index()
        {
            IQueryable<ThanhToanVienPhi> query = _context.ThanhToanVienPhis.Include(t => t.User);

            if (User.IsInRole("BenhNhan"))
            {
                var userId = _userManager.GetUserId(User);
                query = query.Where(t => t.UserId == userId);
            }
            // Bác sĩ sẽ thấy tất cả hóa đơn

            var danhSachHoaDon = await query.OrderByDescending(t => t.NgayThanhToan).ToListAsync();
            return View(danhSachHoaDon);
        }

        // GET: ThanhToanVienPhi/ChiTiet/5
        // Hiển thị chi tiết một hóa đơn cho bệnh nhân
        [Authorize(Roles = "BenhNhan")]
        public async Task<IActionResult> ChiTiet(int? id)
        {
            if (id == null) return NotFound();

            var hoaDon = await _context.ThanhToanVienPhis
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (hoaDon == null) return NotFound();

            // Bảo mật: Đảm bảo bệnh nhân chỉ xem được hóa đơn của chính mình
            var currentUserId = _userManager.GetUserId(User);
            if (hoaDon.UserId != currentUserId)
            {
                return Forbid();
            }

            return View(hoaDon);
        }

        // GET: ThanhToanVienPhi/TaoHoaDon (Dành cho Bác sĩ)
        [Authorize(Roles = "BacSi")]
        public async Task<IActionResult> TaoHoaDon()
        {
            // Lấy danh sách các user có vai trò là "BenhNhan"
            var benhNhanUsers = await _userManager.GetUsersInRoleAsync("BenhNhan");
            ViewBag.UserId = new SelectList(benhNhanUsers, "Id", "FullName");
            return View();
        }

        // POST: ThanhToanVienPhi/TaoHoaDon
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "BacSi")]
        public async Task<IActionResult> TaoHoaDon([Bind("UserId,SoTien,GhiChu")] ThanhToanVienPhi thanhToan)
        {
            try
            {
                // Xóa lỗi validation của các thuộc tính không được bind từ form
                ModelState.Remove("User");
                ModelState.Remove("TrangThai");
                ModelState.Remove("HinhThucThanhToan");

                if (ModelState.IsValid)
                {
                    thanhToan.NgayThanhToan = DateTime.Now;
                    thanhToan.TrangThai = "Chưa thanh toán";
                    thanhToan.HinhThucThanhToan = "Chuyển khoản"; // Mặc định

                    _context.Add(thanhToan);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Đã tạo hóa đơn viện phí thành công.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException /* ex */)
            {
                // Ghi log lỗi (ex) và thêm một model error để hiển thị trên form
                ModelState.AddModelError("", "Không thể tạo hóa đơn do lỗi cơ sở dữ liệu. Vui lòng thử lại.");
            }


            // Nếu model không hợp lệ hoặc có lỗi, tải lại danh sách bệnh nhân và hiển thị lại form
            var benhNhanUsers = await _userManager.GetUsersInRoleAsync("BenhNhan");
            ViewBag.UserId = new SelectList(benhNhanUsers, "Id", "FullName", thanhToan.UserId);
            return View(thanhToan);
        }

        // POST: ThanhToanVienPhi/XacNhanThanhToan/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "BacSi")]
        public async Task<IActionResult> XacNhanThanhToan(int id)
        {
            var hoaDon = await _context.ThanhToanVienPhis.FindAsync(id);
            if (hoaDon == null)
            {
                return NotFound();
            }

            hoaDon.TrangThai = "Đã thanh toán";
            _context.Update(hoaDon);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xác nhận thanh toán thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}