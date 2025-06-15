using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KTPMUD.Models
{
    public class ThanhToanVienPhi
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal SoTien { get; set; }

        public DateTime NgayThanhToan { get; set; }
        public string HinhThucThanhToan { get; set; }
        public string TrangThai { get; set; }
        public string GhiChu { get; set; }
    }
}