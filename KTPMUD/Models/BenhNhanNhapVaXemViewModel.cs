
using KTPMUD.Models; 
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KTPMUD.ViewModels // Đảm bảo namespace này đúng
{
    public class BenhNhanNhapVaXemViewModel
    {

        public BenhNhan BenhNhanMoi { get; set; }

        public IEnumerable<BenhNhan> DanhSachDaNhap { get; set; }

        public BenhNhanNhapVaXemViewModel()
        {
            BenhNhanMoi = new BenhNhan();
            DanhSachDaNhap = new List<BenhNhan>();
        }
    }
}
