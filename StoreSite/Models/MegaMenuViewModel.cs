using System.Collections.Generic;
using StoreSite.Controllers;

namespace StoreSite.Models
{
    public class MegaMenuViewModel
    {
        public List<string> BrandsNam { get; set; } = new List<string>();
        public List<string> BrandsNu { get; set; } = new List<string>();
        public List<SanPham> Featured { get; set; } = new List<SanPham>();
        public List<string> Sports { get; set; } = new List<string>();
    }
}