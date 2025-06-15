using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KTPMUD.Models 
{
    public class BacSi
    {
        [Key]
        public int Id { get; set; }

        public string HoTen { get; set; }

        public string ChuyenKhoa { get; set; }

        public string SoDienThoai { get; set; }

        public virtual ICollection<LichHen> LichHens { get; set; }
    }
}