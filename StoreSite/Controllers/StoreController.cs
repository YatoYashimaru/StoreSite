using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StoreSite.Models;

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
        public string Description { get; set; }
        public double rating { get; set; }
        public bool SanPhamMoi { get; set; }
    }
    public class StoreController : Controller
    {
        [NonAction]
        public static List<SanPham> GetSanPham()
        {
            return new List<SanPham> /// database tam thoi
            {
                new SanPham { Id = 1, Name="Nike Air Max 1", LoaiGiay="Nike", Gender="Nam",
                    GiaTien=3000000,GiaTienGoc=0,
                    ImgURL="/ImageAssets/Men/Giay-Nike-Air-Max-1.jpg", rating=4.9, SanPhamMoi=true,
                    Description="Nike Air Max 1 là một trong những mẫu giày thể thao nổi tiếng của Nike, được ra mắt lần đầu tiên vào năm 1987. " +
                    "Với thiết kế đột phá và công nghệ Air Max, giày mang đến sự thoải mái và hỗ trợ tối đa cho người sử dụng. Đế giày có lớp đệm Air Max giúp giảm chấn động khi di chuyển, " +
                    "đồng thời tạo cảm giác nhẹ nhàng và linh hoạt. Phần upper được làm từ chất liệu cao cấp, kết hợp với các chi tiết thiết kế tinh tế, tạo nên vẻ ngoài hiện đại và phong cách. " +
                    "Nike Air Max 1 không chỉ là một đôi giày thể thao mà còn là biểu tượng thời trang, phù hợp cho cả việc tập luyện và sử dụng hàng ngày."
                },
                new SanPham { Id = 2, Name="Ultraboost 22", LoaiGiay="Adidas", Gender="Nam",
                    GiaTien=4500000,GiaTienGoc=5000000,
                    ImgURL="/ImageAssets/Men/adidas_ultraboost_22.jpg", rating=5.0, SanPhamMoi=true,
                    Description="Adidas Ultraboost 22 là phiên bản nâng cấp của dòng giày chạy bộ Ultraboost nổi tiếng, được thiết kế để mang lại hiệu suất tối ưu và sự thoải mái tuyệt đối cho người sử dụng. "
                },
                new SanPham { Id = 3, Name="RS-X", LoaiGiay="Puma", Gender="Nu",
                    GiaTien=3000000,GiaTienGoc=0,
                    ImgURL="/ImageAssets/Men/RS-X.jpg", rating=4.7, SanPhamMoi=false,
                    Description="Puma RS-X là một dòng giày thể thao nổi bật của Puma, được thiết kế với phong cách hiện đại và cá tính. "
                },
                new SanPham { Id = 4, Name="Nike Dunk Low", LoaiGiay="Nike", Gender="Nam",
                    GiaTien=2650000, GiaTienGoc=3100000,
                    ImgURL="/ImageAssets/Men/Nike Dunk Low.jpeg", rating=4.8, SanPhamMoi=true,
                    Description="Nike Dunk Low là một trong những mẫu giày thể thao nổi tiếng của Nike, được ra mắt lần đầu tiên vào năm 1985. "
                },
                new SanPham { Id = 5, Name="Puma Palermo Special", LoaiGiay="Puma", Gender="Nam",
                    GiaTien=2100000, GiaTienGoc=2500000,
                    ImgURL="/ImageAssets/Women/Puma Palermo Special.png", rating=4.6, SanPhamMoi=false,
                    Description="Puma Palermo Special là một mẫu giày thể thao nổi bật của Puma, được thiết kế với phong cách hiện đại và cá tính. "
                },
                new SanPham { Id = 6, Name="Converse Chuck 70 High", LoaiGiay="Converse", Gender="Nu",
                    GiaTien=2000000, GiaTienGoc=0,
                    ImgURL="/ImageAssets/Men/Converse Chuck 70 High.png", rating=4.7, SanPhamMoi=false,
                    Description="Converse Chuck 70 High là một phiên bản nâng cấp của dòng giày Chuck Taylor All Star nổi tiếng, được ra mắt để kỷ niệm 70 năm thành lập của Converse. "
                },
                new SanPham { Id = 7, Name="New Balance 530", LoaiGiay="New Balance", Gender="Nu",
                    GiaTien=2850000, GiaTienGoc=3300000,
                    ImgURL="/ImageAssets/Men/New Balance 530.png", rating=4.9, SanPhamMoi=true,
                    Description="New Balance 530 là một mẫu giày thể thao nổi bật của New Balance, được thiết kế với phong cách hiện đại và cá tính. "
                },
                new SanPham { Id = 8, Name="Vans Old Skool Classic", LoaiGiay="Vans", Gender="Nam",
                    GiaTien=1850000, GiaTienGoc=0,
                    ImgURL="/ImageAssets/Men/Vans Old Skool Classic.png", rating=4.5, SanPhamMoi=false,
                    Description="Vans Old Skool Classic là một trong những mẫu giày thể thao nổi tiếng của Vans, được ra mắt lần đầu tiên vào năm 1977. "
                },
                new SanPham { Id = 9, Name="Adidas Forum Low", LoaiGiay="Adidas", Gender="Nu",
                    GiaTien=2500000, GiaTienGoc=2900000,
                    ImgURL="/ImageAssets/Men/Adidas Forum Low.jpg", rating=4.7, SanPhamMoi=true,
                    Description="Adidas Forum Low là một mẫu giày thể thao nổi bật của Adidas, được thiết kế với phong cách hiện đại và cá tính. "
                },
                new SanPham { Id = 10, Name="Nike Phantom 6 Low Elite", LoaiGiay="Nike", Gender="Nam",
                    GiaTien=7500000, GiaTienGoc=8000000,
                    ImgURL="/ImageAssets/Men/Nike Phantom 6 Erling Haaland.jpg", rating=5, SanPhamMoi=true,
                    Description="Nike Phantom 6 là đôi giày biểu tượng và cũng là hình ảnh của cầu thủ Erling Haaland."
                },
                new SanPham { Id = 11, Name="F50 Messi", LoaiGiay="Adidas", Gender="Nam",
                    GiaTien=4500000, GiaTienGoc=5000000,
                    ImgURL="/ImageAssets/Men/Adidas F50 Messi.jpg", rating=5, SanPhamMoi=true,
                    Description="F50 Messi là đôi giày tượng trưng cho hình tượng Argentina bóng đá."
                },
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

        public ActionResult Details(int id)
        {
            var all = GetSanPham();
            var sp = all.FirstOrDefault(p => p.Id == id);
            if (sp == null) return HttpNotFound();

            var vm = new ProductDetailsViewModel
            {
                Product = sp,
                Theme = ThemeHelper.GetTheme(sp.ImgURL),
                Related = all.Where(p => p.Id != id && (p.LoaiGiay == sp.LoaiGiay || p.Gender == sp.Gender))
                             .OrderByDescending(p => p.rating)
                             .Take(4)
                             .ToList()
            };
            return View(vm);
        }
    }
}
