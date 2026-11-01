using System.Collections.Generic;
using StoreSite.Controllers;   // nơi chứa class SanPham; không sao nếu thừa

namespace StoreSite.Models
{
    public class MegaMenuViewModel
    {
        public List<string> BrandsNam { get; set; } = new List<string>();
        public List<string> BrandsNu { get; set; } = new List<string>();
        public List<SanPham> Featured { get; set; } = new List<SanPham>();
    }
}