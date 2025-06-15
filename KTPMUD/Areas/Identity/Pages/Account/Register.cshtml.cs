#nullable disable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using KTPMUD.Models;
using KTPMUD.Data;

namespace KTPMUD.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }
        public string ReturnUrl { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
            [Display(Name = "Họ và Tên")]
            public string HoTen { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập Email.")]
            [EmailAddress]
            public string Email { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
            [Phone]
            [Display(Name = "Số điện thoại")]
            public string SoDienThoai { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "{0} phải dài từ {2} đến {1} ký tự.", MinimumLength = 3)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Xác nhận mật khẩu")]
            [Compare("Password", ErrorMessage = "Mật khẩu và mật khẩu xác nhận không khớp.")]
            public string ConfirmPassword { get; set; }

            [Required]
            public string Role { get; set; }

            [Display(Name = "Chuyên Khoa")]
            public string ChuyenKhoa { get; set; }
        }

        private async Task PrepareChuyenKhoaList()
        {
            var chuyenKhoaList = new List<string>
            {
                "Chuyên khoa Nội",
                "Chuyên khoa Ngoại",
                "Chuyên khoa Sản – Phụ khoa",
                "Chuyên khoa Nhi",
                "Chuyên khoa Mắt (Nhãn khoa)",
                "Chuyên khoa Tai – Mũi – Họng (TMH)",
                "Chuyên khoa Da liễu",
                "Chuyên khoa Răng – Hàm – Mặt",
                "Chuyên khoa Tâm thần – Tâm lý",
                "Chuyên khoa Xét nghiệm & Cận lâm sàng"
            };
            ViewData["ChuyenKhoaList"] = new SelectList(chuyenKhoaList);
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            await PrepareChuyenKhoaList(); // Chuẩn bị danh sách chuyên khoa
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    FullName = Input.HoTen,
                    PhoneNumber = Input.SoDienThoai
                };
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, Input.Role);

                    if (Input.Role == "BacSi")
                    {
                        var bacSi = new BacSi
                        {
                            HoTen = Input.HoTen,
                            ChuyenKhoa = Input.ChuyenKhoa,
                            SoDienThoai = Input.SoDienThoai
                        };
                        _context.BacSis.Add(bacSi);
                        await _context.SaveChangesAsync();
                        user.BacSiId = bacSi.Id;
                        await _userManager.UpdateAsync(user);
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Nếu model không hợp lệ, tải lại danh sách chuyên khoa để hiển thị lại form
            await PrepareChuyenKhoaList();
            return Page();
        }
    }
}
