using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KTPMUD.Models
{
    public class LichHen
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [Required]
        public int IdBacSi { get; set; }

        [ForeignKey("IdBacSi")]
        public virtual BacSi BacSi { get; set; }

        public DateTime ThoiGian { get; set; }
        public string TrangThai { get; set; }
        public string GhiChu { get; set; }
    }
}