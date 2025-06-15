using System.ComponentModel.DataAnnotations; 

namespace KTPMUD.Models
{
    public class BenhNhan
    {
        [Key] 
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [Display(Name = "Họ và Tên")]
        [StringLength(100)]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập ngày sinh.")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày Sinh")]
        public DateTime NgaySinh { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giới tính.")]
        [Display(Name = "Giới Tính")]
        public string GioiTinh { get; set; } // Ví dụ: "Nam", "Nữ", "Khác"

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ.")]
        [Display(Name = "Địa Chỉ")]
        [StringLength(250)]
        public string DiaChi { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [Display(Name = "Số Điện Thoại")]
        [StringLength(15)]
        public string SoDienThoai { get; set; }

        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        [Display(Name = "Email")]
        [StringLength(100)]
        public string Email { get; set; }

        [Display(Name = "Tiền sử bệnh án")]
        [DataType(DataType.MultilineText)]
        public string TienSuBenhAn { get; set; }

        [Display(Name = "Ghi Chú")]
        [DataType(DataType.MultilineText)]
        public string GhiChu { get; set; }
    }
}