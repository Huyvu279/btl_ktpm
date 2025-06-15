using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KTPMUD.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; }

        public int? BacSiId { get; set; }

        public virtual ICollection<LichHen> LichHens { get; set; } = new List<LichHen>();
        public virtual ICollection<ThanhToanVienPhi> ThanhToanVienPhis { get; set; } = new List<ThanhToanVienPhi>();
    }
}
