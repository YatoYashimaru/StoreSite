using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StoreSite.Controllers
{
    public class SanPham
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LoaiGiay { get; set; }
        public string Gender{ get; set; }
        public decimal GiaTien { get; set; }
        public decimal? GiaTienGoc { get; set; }
        public string ImgURL { get; set; }
        public double rating { get; set; }
        public bool SanPhamMoi { get; set; }
    }
    public class StoreController : Controller
    {
        private static List<SanPham> GetSanPham()
        {
            return new List<SanPham> /// database tam thoi
            {
                new SanPham { Id = 1, Name="Nike Air Max 1", LoaiGiay="Nike", Gender="Nam",
                    GiaTien=3000000,GiaTienGoc=0,
                    ImgURL="/ImageAssets/Men/Giay-Nike-Air-Max-1.jpg", rating=4.9, SanPhamMoi=true },
                new SanPham { Id = 2, Name="Ultraboost 22", LoaiGiay="Adidas", Gender="Nam",
                    GiaTien=4500000,GiaTienGoc=5000000,
                    ImgURL="/ImageAssets/Men/adidas_ultraboost_22.jpg", rating=5.0, SanPhamMoi=true },
                new SanPham { Id = 3, Name="RS-X", LoaiGiay="Puma", Gender="Nu",
                    GiaTien=3000000,GiaTienGoc=0,
                    ImgURL="/ImageAssets/Men/RS-X.jpg", rating=4.7, SanPhamMoi=false },
                new SanPham { Id = 4, Name="Nike Dunk Low", LoaiGiay="Nike", Gender="Nam",
                    GiaTien=2650000, GiaTienGoc=3100000,
                    ImgURL="/ImageAssets/Men/Nike Dunk Low.jpeg", rating=4.8, SanPhamMoi=true },
                new SanPham { Id = 5, Name="Puma Palermo Special", LoaiGiay="Puma", Gender="Nam",
                    GiaTien=2100000, GiaTienGoc=2500000,
                    ImgURL="/ImageAssets/Women/Puma Palermo Special.png", rating=4.6, SanPhamMoi=false },
                new SanPham { Id = 6, Name="Converse Chuck 70 High", LoaiGiay="Converse", Gender="Nu",
                    GiaTien=2000000, GiaTienGoc=0,
                    ImgURL="/ImageAssets/Men/Converse Chuck 70 High.png", rating=4.7, SanPhamMoi=false },
                new SanPham { Id = 7, Name="New Balance 530", LoaiGiay="New Balance", Gender="Nu",
                    GiaTien=2850000, GiaTienGoc=3300000,
                    ImgURL="/ImageAssets/Men/New Balance 530.png", rating=4.9, SanPhamMoi=true },
                new SanPham { Id = 8, Name="Vans Old Skool Classic", LoaiGiay="Vans", Gender="Nam",
                    GiaTien=1850000, GiaTienGoc=0,
                    ImgURL="/ImageAssets/Men/Vans Old Skool Classic.png", rating=4.5, SanPhamMoi=false },
                new SanPham { Id = 9, Name="Adidas Forum Low", LoaiGiay="Adidas", Gender="Nu",
                    GiaTien=2500000, GiaTienGoc=2900000,
                    ImgURL="/ImageAssets/Men/Adidas Forum Low.jpg", rating=4.7, SanPhamMoi=true },
            };
        }
        // GET: Store
        public ActionResult Index(string TimKiem, string Loai,string Gender, string sortOrder)
        {
            List<SanPham> sanpham = GetSanPham();//Lấy danh sách sản phẩm
            if (!string.IsNullOrEmpty(TimKiem))//Lọc theo từ khóa tìm kiếm
            {
                sanpham=sanpham.Where(sp=>sp.Name.ToLower().Contains(TimKiem.ToLower())).ToList();
            }
            if (!string.IsNullOrEmpty(Loai) && Loai != "All")
            {
                sanpham = sanpham.Where(sp => sp.LoaiGiay.ToLower() == Loai.ToLower()).ToList();
            }
            if (!string.IsNullOrEmpty(Gender) && Gender == "Nam")
            {
                sanpham = sanpham.Where(sp => sp.Gender == "Nam").ToList();
            }
            if (!string.IsNullOrEmpty(Gender) && Gender == "Nu")
            {
                sanpham = sanpham.Where(sp => sp.Gender == "Nu").ToList();
            }
            switch (sortOrder)//Thuật toán sắp xếp theo giá tiền
            {
                case "Tang_Dan":
                    sanpham = sanpham.OrderBy(p => p.GiaTien).ToList(); // Giá từ thấp đến cao
                    break;
                case "Giam_Dan":
                    sanpham = sanpham.OrderByDescending(p => p.GiaTien).ToList(); // Giá từ cao đến thấp
                    break;
                default:
                    sanpham = sanpham.OrderBy(p => p.Id).ToList(); // Mặc định sắp xếp theo ID
                    break;
            }
            ViewBag.TimKiem = TimKiem;
            ViewBag.Loai = Loai;
            ViewBag.Gender = Gender;
            ViewBag.Sort = sortOrder;
            
            return View(sanpham.ToList());
        }
    }
}
