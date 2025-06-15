using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KTPMUD.Data;
using KTPMUD.Models;
using System.Linq;
using System.Threading.Tasks;

namespace KTPMUD.Controllers
{
    [Authorize]
    public class LichHenController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LichHenController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: LichHen
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            IQueryable<LichHen> query = _context.LichHens
                .Include(l => l.User)
                .Include(l => l.BacSi);

            if (User.IsInRole("BacSi"))
            {
                var bacSiUser = await _context.Users.FindAsync(userId);
                if (bacSiUser?.BacSiId != null)
                {
                    query = query.Where(l => l.IdBacSi == bacSiUser.BacSiId);
                }
                else
                {
                    query = query.Where(l => false);
                }
            }
            else if (User.IsInRole("BenhNhan"))
            {
                query = query.Where(l => l.UserId == userId);
            }
            else
            {
                query = query.Where(l => false);
            }

            // SẮP XẾP LỊCH HẸN: Dùng OrderBy để sắp xếp từ sớm nhất đến muộn nhất
            var lichHens = await query.OrderBy(l => l.ThoiGian).ToListAsync();
            return View(lichHens);
        }

        // GET: LichHen/Create
        [Authorize(Roles = "BenhNhan")]
        public async Task<IActionResult> Create()
        {
            // Chỉ cần lấy danh sách các chuyên khoa duy nhất từ bảng Bác sĩ
            ViewBag.ChuyenKhoaList = new SelectList(await _context.BacSis.Select(b => b.ChuyenKhoa).Distinct().ToListAsync());
            return View(new LichHenViewModel { NgayHen = DateTime.Today });
        }

        // POST: LichHen/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "BenhNhan")]
        public async Task<IActionResult> Create(LichHenViewModel model)
        {
            DateTime thoiGianHen;
            try
            {
                thoiGianHen = model.NgayHen.Add(TimeSpan.Parse(model.KhungGio));
            }
            catch
            {
                ModelState.AddModelError("", "Ngày hoặc khung giờ hẹn không hợp lệ.");
                await PopulateCreateViewBag(model.ChuyenKhoa);
                return View(model);
            }

            if (ModelState.IsValid)
            {
                // Lấy danh sách ID của các bác sĩ đã bận vào khung giờ này
                var busyDoctorIds = await _context.LichHens
                    .Where(l => l.ThoiGian == thoiGianHen && l.TrangThai != "Đã hủy")
                    .Select(l => l.IdBacSi)
                    .ToListAsync();

                // Lấy danh sách tất cả các bác sĩ thuộc chuyên khoa đã chọn
                var doctorsInSpecialty = await _context.BacSis
                    .Where(b => b.ChuyenKhoa == model.ChuyenKhoa)
                    .ToListAsync();

                // Lấy danh sách các bác sĩ còn trống lịch
                var availableDoctors = doctorsInSpecialty
                    .Where(d => !busyDoctorIds.Contains(d.Id))
                    .ToList();

                int assignedDoctorId;

                if (model.IdBacSi.HasValue) // Trường hợp bệnh nhân CHỌN bác sĩ cụ thể
                {
                    if (availableDoctors.Any(d => d.Id == model.IdBacSi.Value))
                    {
                        assignedDoctorId = model.IdBacSi.Value;
                    }
                    else
                    {
                        ModelState.AddModelError("", "Bác sĩ bạn chọn đã bận vào khung giờ này. Vui lòng chọn bác sĩ khác hoặc để trống.");
                        await PopulateCreateViewBag(model.ChuyenKhoa);
                        return View(model);
                    }
                }
                else // Trường hợp bệnh nhân KHÔNG chọn bác sĩ (để trống)
                {
                    if (availableDoctors.Any())
                    {
                        // Lấy ngẫu nhiên một bác sĩ từ danh sách còn trống
                        var random = new Random();
                        assignedDoctorId = availableDoctors[random.Next(availableDoctors.Count)].Id;
                    }
                    else
                    {
                        ModelState.AddModelError("", "Rất tiếc, tất cả các bác sĩ trong chuyên khoa này đều đã bận vào khung giờ bạn chọn. Vui lòng chọn thời gian khác.");
                        await PopulateCreateViewBag(model.ChuyenKhoa);
                        return View(model);
                    }
                }

                // Nếu đã tìm được bác sĩ, tiến hành tạo lịch hẹn
                var lichHen = new LichHen
                {
                    IdBacSi = assignedDoctorId,
                    GhiChu = model.GhiChu,
                    ThoiGian = thoiGianHen,
                    UserId = _userManager.GetUserId(User),
                    TrangThai = "Đã lên lịch"
                };

                _context.Add(lichHen);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đặt lịch hẹn thành công với bác sĩ vào lúc {thoiGianHen:HH:mm dd/MM/yyyy}!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateCreateViewBag(model.ChuyenKhoa);
            return View(model);
        }

        // *** API ĐỂ LẤY DANH SÁCH BÁC SĨ THEO CHUYÊN KHOA ***
        [HttpGet]
        public async Task<JsonResult> GetDoctorsBySpecialty(string chuyenKhoa)
        {
            var doctors = await _context.BacSis
                .Where(b => b.ChuyenKhoa == chuyenKhoa)
                .Select(b => new { id = b.Id, hoTen = b.HoTen })
                .ToListAsync();
            return Json(doctors);
        }

        // Hàm hỗ trợ để tải lại dữ liệu cho DropDownLists
        private async Task PopulateCreateViewBag(string selectedSpecialty = null)
        {
            ViewBag.ChuyenKhoaList = new SelectList(await _context.BacSis.Select(b => b.ChuyenKhoa).Distinct().ToListAsync(), selectedSpecialty);
        }


        // GET: LichHen/Cancel/5
        public async Task<IActionResult> Cancel(int? id)
        {
            if (id == null) return NotFound();
            var lichHen = await _context.LichHens
                .Include(l => l.User)
                .Include(l => l.BacSi)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (lichHen == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            // THAY ĐỔI LOGIC: Chỉ bệnh nhân đặt lịch mới được hủy
            if (lichHen.UserId != userId)
            {
                return Forbid(); // Trả về lỗi không có quyền truy cập
            }
            return View(lichHen);
        }
        // GET: LichHen/ChiTiet/5
        public async Task<IActionResult> ChiTiet(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lichHen = await _context.LichHens
                .Include(l => l.User) // Lấy thông tin tài khoản người đặt (Bệnh nhân)
                .Include(l => l.BacSi) // Lấy thông tin Bác sĩ
                .FirstOrDefaultAsync(m => m.Id == id);

            if (lichHen == null)
            {
                return NotFound();
            }

            // --- KIỂM TRA BẢO MẬT ---
            // Tách nhỏ logic kiểm tra quyền để code rõ ràng hơn
            var currentUserId = _userManager.GetUserId(User);

            // Điều kiện 1: Người dùng hiện tại có phải là bệnh nhân đã đặt lịch này không?
            bool isPatientOwner = (lichHen.UserId == currentUserId);

            // Điều kiện 2: Người dùng hiện tại có phải là bác sĩ được gán cho lịch này không?
            bool isAssignedDoctor = false;
            if (User.IsInRole("BacSi"))
            {
                var currentUser = await _context.Users.FindAsync(currentUserId);
                isAssignedDoctor = (lichHen.IdBacSi == currentUser?.BacSiId);
            }

            // Nếu người dùng không phải là bệnh nhân đặt lịch VÀ cũng không phải là bác sĩ được gán,
            // thì không có quyền xem.
            if (!isPatientOwner && !isAssignedDoctor)
            {
                return Forbid(); // Trả về lỗi không có quyền truy cập
            }
            // -------------------------

            return View(lichHen);
        }


        // POST: LichHen/Cancel/5
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            var lichHen = await _context.LichHens.FindAsync(id);
            if (lichHen == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            // THAY ĐỔI LOGIC: Chỉ bệnh nhân đặt lịch mới được hủy
            if (lichHen.UserId != userId)
            {
                return Forbid();
            }

            lichHen.TrangThai = "Đã hủy";
            _context.Update(lichHen);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã hủy lịch hẹn thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
