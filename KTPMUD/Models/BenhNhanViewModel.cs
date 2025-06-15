using KTPMUD.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering; 

namespace KTPMUD.ViewModels
{
    public class BenhNhanViewModel
    {
        public BenhNhan BenhNhan { get; set; }
        public List<SelectListItem> DanhSachGioiTinh { get; set; }

        public BenhNhanViewModel()
        {
            BenhNhan = new BenhNhan(); 
            DanhSachGioiTinh = new List<SelectListItem>
            {
                new SelectListItem { Value = "Nam", Text = "Nam" },
                new SelectListItem { Value = "Nữ", Text = "Nữ" },
                new SelectListItem { Value = "Khác", Text = "Khác" }
            };
        }
    }
}