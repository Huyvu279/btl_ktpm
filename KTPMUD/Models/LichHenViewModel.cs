using System;
using System.ComponentModel.DataAnnotations;

namespace KTPMUD.Models 
{
    public class LichHenViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn chuyên khoa.")]
        [Display(Name = "Chuyên Khoa")]
        public string ChuyenKhoa { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn bác sĩ (không bắt buộc).")]
        [Display(Name = "Bác Sĩ")]
        public int? IdBacSi { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày hẹn.")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày Hẹn")]
        public DateTime NgayHen { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn khung giờ.")]
        [Display(Name = "Khung Giờ")]
        public string KhungGio { get; set; }

        [Display(Name = "Ghi Chú (Triệu chứng)")]
        public string GhiChu { get; set; }
    }
}
