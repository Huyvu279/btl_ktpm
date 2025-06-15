

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KTPMUD.Data;       
using KTPMUD.Models;
using KTPMUD.ViewModels; 
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace KTPMUD.Controllers 
{
    [Authorize(Roles = "BacSi")]
    public class BenhNhanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BenhNhanController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: BenhNhan/NhapThongTin
        public async Task<IActionResult> NhapThongTin()
        {
            var viewModel = new BenhNhanNhapVaXemViewModel
            {
                DanhSachDaNhap = await _context.BenhNhans 
                                         .OrderByDescending(b => b.Id)
                                         .ToListAsync()
            };
            return View(viewModel);
        }

        // POST: BenhNhan/NhapThongTin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NhapThongTin(BenhNhanNhapVaXemViewModel viewModelFromForm)
        {
            if (ModelState.IsValid)
            {
                _context.Add(viewModelFromForm.BenhNhanMoi);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã thêm bệnh nhân '" + viewModelFromForm.BenhNhanMoi.HoTen + "' thành công!";
                return RedirectToAction(nameof(NhapThongTin));
            }

            viewModelFromForm.DanhSachDaNhap = await _context.BenhNhans // Giả sử DbSet tên là BenhNhans
                                                     .OrderByDescending(b => b.Id)
                                                     .ToListAsync();
            return View(viewModelFromForm);
        }

        // GET: BenhNhan/DanhSach
        public async Task<IActionResult> DanhSach(string searchString, bool showAll = false)
        {
            ViewData["CurrentFilter"] = searchString;
            var benhNhansQuery = _context.BenhNhans.AsQueryable();
            List<BenhNhan> danhSachHienThi;

            if (!string.IsNullOrEmpty(searchString))
            {
                // 1. Nếu có chuỗi tìm kiếm -> Lọc và hiển thị kết quả
                bool isNumeric = int.TryParse(searchString, out int id);
                benhNhansQuery = benhNhansQuery.Where(bn =>
                    bn.HoTen.Contains(searchString) ||
                    bn.SoDienThoai.Contains(searchString) ||
                    (isNumeric && bn.Id == id)
                );
                danhSachHienThi = await benhNhansQuery.OrderBy(bn => bn.HoTen).ToListAsync();
            }
            else if (showAll)
            {
                // 2. Nếu không có chuỗi tìm kiếm NHƯNG nhấn nút "Xem tất cả" -> Hiển thị tất cả
                danhSachHienThi = await benhNhansQuery.OrderBy(bn => bn.HoTen).ToListAsync();
            }
            else
            {
                // 3. Trường hợp mặc định (lần đầu tải trang) -> Hiển thị danh sách rỗng
                danhSachHienThi = new List<BenhNhan>();
            }

            return View(danhSachHienThi);
        }





        // GET: BenhNhan/ChiTiet/5
        public async Task<IActionResult> ChiTiet(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var benhNhan = await _context.BenhNhans // Giả sử DbSet tên là BenhNhans
                                         .FirstOrDefaultAsync(m => m.Id == id);
            if (benhNhan == null)
            {
                return NotFound();
            }

            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }
            return View(benhNhan);
        }

        // GET: BenhNhan/SuaThongTin/5
        public async Task<IActionResult> ChinhSua(int? id)
        {
            if (id == null)
            {
                return NotFound("Không tìm thấy ID bệnh nhân.");
            }

            var benhNhan = await _context.BenhNhans.FindAsync(id); // Giả sử DbSet tên là BenhNhans
            if (benhNhan == null)
            {
                return NotFound("Không tìm thấy bệnh nhân với ID này.");
            }
            return View(benhNhan);
        }

        // POST: BenhNhan/SuaThongTin/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChinhSua(int id, [Bind("Id,HoTen,NgaySinh,GioiTinh,DiaChi,SoDienThoai,Email,TienSuBenhAn,GhiChu")] BenhNhan benhNhanModel)
        {
            if (id != benhNhanModel.Id)
            {
                return NotFound("ID không khớp.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(benhNhanModel);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật thông tin bệnh nhân thành công!";
                    return RedirectToAction(nameof(ChiTiet), new { id = benhNhanModel.Id }); // Chuyển hướng về trang ChiTiet
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BenhNhanExists(benhNhanModel.Id))
                    {
                        return NotFound("Không tìm thấy bệnh nhân để cập nhật.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Thông tin bệnh nhân đã được người khác cập nhật. Vui lòng tải lại trang và thử lại.");
                        
                    }
                }
                catch (Exception ex)
                {
                    // Ghi log lỗi (ví dụ: sử dụng ILogger)
                    ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi cố gắng lưu thay đổi: " + ex.Message);
                }
                // Nếu có lỗi concurrency (và không phải NotFound) hoặc lỗi Exception khác, ModelState sẽ chứa lỗi.
                // Hiển thị lại form với dữ liệu và lỗi.
                return View(benhNhanModel);
            }
            // Nếu ModelState không hợp lệ ngay từ đầu (ví dụ: thiếu trường bắt buộc).
            return View(benhNhanModel);
        }

        // GET: BenhNhan/Xoa/5
        public async Task<IActionResult> Xoa(int? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return NotFound("Không tìm thấy ID bệnh nhân.");
            }

            var benhNhan = await _context.BenhNhans 
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (benhNhan == null)
            {
                return NotFound("Không tìm thấy bệnh nhân.");
            }

            if (saveChangesError.GetValueOrDefault())
            {
                ViewData["ErrorMessage"] = "Xóa thất bại. Vui lòng thử lại, và nếu vấn đề vẫn tiếp diễn, hãy liên hệ quản trị viên hệ thống.";
            }
            return View(benhNhan);
        }

        // POST: BenhNhan/Xoa/5
        [HttpPost, ActionName("Xoa")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaConfirmed(int id)
        {
            var benhNhan = await _context.BenhNhans.FindAsync(id);

            if (benhNhan == null)
            {
                TempData["InfoMessage"] = "Bệnh nhân có thể đã được xóa trước đó.";
                return RedirectToAction(nameof(NhapThongTin)); // Hoặc trang danh sách chính của bạn
            }

            try
            {
                _context.BenhNhans.Remove(benhNhan); // Giả sử DbSet là BenhNhans
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa thành công bệnh nhân: {benhNhan.HoTen}.";
                return RedirectToAction(nameof(NhapThongTin)); // Hoặc trang danh sách chính của bạn
            }
            catch (DbUpdateException /* ex */)
            {
                // Ghi log lỗi (ex)
                return RedirectToAction(nameof(Xoa), new { id = id, saveChangesError = true });
            }
        }

        private bool BenhNhanExists(int id)
        {
            return _context.BenhNhans.Any(e => e.Id == id); // Giả sử DbSet tên là BenhNhans
        }
    } // Dấu ngoặc nhọn đóng của class BenhNhanController
} // Dấu ngoặc nhọn đóng của namespace KTPMUD.Controllers
