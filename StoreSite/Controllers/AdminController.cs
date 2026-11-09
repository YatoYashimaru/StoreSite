using System;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using StoreSite.Models;

namespace StoreSite.Controllers
{
    public class AdminController : AdminBaseController
    {
        private const int PageSize = 10;

        // /Admin  ->  chuyển thẳng sang danh sách sản phẩm (sau này có thể làm trang tổng quan)
        public ActionResult Index()
        {
            return RedirectToAction("Products");
        }

        // =====================================================================
        //  DANH SÁCH SẢN PHẨM
        // =====================================================================
        public ActionResult Products(string search, string loai, string sport, string gender, int page = 1)
        {
            IQueryable<SanPham> query = db.SanPhams;

            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.Trim();
                query = query.Where(p => p.ItemName.Contains(s));
            }
            if (!string.IsNullOrEmpty(loai)) query = query.Where(p => p.LoaiGiay == loai);
            if (!string.IsNullOrEmpty(sport)) query = query.Where(p => p.Sport == sport);
            if (gender == "Nam" || gender == "Nu") query = query.Where(p => p.Gender == gender);

            int total = query.Count();
            int totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
            page = Math.Min(Math.Max(page, 1), totalPages);

            var vm = new AdminProductListViewModel
            {
                Items = query.OrderByDescending(p => p.Id)          // sản phẩm mới thêm nằm trên cùng
                             .Skip((page - 1) * PageSize).Take(PageSize).ToList(),
                Page = page,
                TotalPages = totalPages,
                Total = total,
                Search = search,
                Loai = loai,
                Sport = sport,
                Gender = gender,
                Brands = db.SanPhams.Select(p => p.LoaiGiay).Distinct().OrderBy(x => x).ToList(),
                Sports = db.SanPhams.Select(p => p.Sport).Distinct().OrderBy(x => x).ToList()
            };

            ViewBag.Title = "Sản phẩm";
            return View(vm);
        }

        // =====================================================================
        //  THÊM
        // =====================================================================
        [HttpGet]
        public ActionResult ProductCreate()
        {
            LoadLookups();
            ViewBag.Title = "Thêm sản phẩm";
            return View("ProductForm", new ProductFormViewModel { Gender = "Nam", RatingText = "0" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ProductCreate(ProductFormViewModel m)
        {
            double rating;
            ValidateProduct(m, out rating);

            if (ModelState.IsValid)
            {
                var sp = new SanPham();
                Apply(m, sp, rating);
                db.SanPhams.Add(sp);

                string loi = TrySave();
                if (loi == null)
                {
                    TempData["ThongBao"] = "Đã thêm sản phẩm “" + sp.ItemName + "”";
                    return RedirectToAction("Products");
                }
                ModelState.AddModelError("", loi);
            }

            LoadLookups();
            ViewBag.Title = "Thêm sản phẩm";
            return View("ProductForm", m);
        }

        // =====================================================================
        //  SỬA
        // =====================================================================
        [HttpGet]
        public ActionResult ProductEdit(int id)
        {
            var sp = db.SanPhams.Find(id);
            if (sp == null) return HttpNotFound();

            LoadLookups();
            ViewBag.Title = "Sửa sản phẩm";
            return View("ProductForm", new ProductFormViewModel
            {
                Id = sp.Id,
                ItemName = sp.ItemName,
                LoaiGiay = sp.LoaiGiay,
                Sport = sp.Sport,
                Gender = sp.Gender,
                GiaTien = sp.GiaTien,
                GiaTienGoc = sp.GiaTienGoc,
                ImgURL = sp.ImgURL,
                DescriptionTxt = sp.DescriptionTxt,
                RatingText = sp.rating.ToString("0.0#", CultureInfo.InvariantCulture),
                SanPhamMoi = sp.SanPhamMoi
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ProductEdit(ProductFormViewModel m)
        {
            var sp = db.SanPhams.Find(m.Id);
            if (sp == null) return HttpNotFound();

            double rating;
            ValidateProduct(m, out rating);

            if (ModelState.IsValid)
            {
                Apply(m, sp, rating);

                string loi = TrySave();
                if (loi == null)
                {
                    TempData["ThongBao"] = "Đã lưu thay đổi cho “" + sp.ItemName + "”";
                    return RedirectToAction("Products");
                }
                ModelState.AddModelError("", loi);
            }

            LoadLookups();
            ViewBag.Title = "Sửa sản phẩm";
            return View("ProductForm", m);
        }

        // =====================================================================
        //  XÓA (chỉ nhận POST, có kiểm tra token)
        // =====================================================================
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ProductDelete(int id)
        {
            var sp = db.SanPhams.Find(id);
            if (sp == null)
            {
                TempData["Loi"] = "Không tìm thấy sản phẩm cần xóa.";
                return BackToList();
            }

            // Bảng OrderDetail có khóa ngoại tới SanPhams: sản phẩm đã từng được đặt thì không xóa được
            if (db.OrderDetails.Any(d => d.ProductID == id))
            {
                TempData["Loi"] = "“" + sp.ItemName + "” đã có trong đơn hàng nên không thể xóa.";
                return BackToList();
            }

            string ten = sp.ItemName;
            db.SanPhams.Remove(sp);

            string loi = TrySave();
            if (loi == null) TempData["ThongBao"] = "Đã xóa sản phẩm “" + ten + "”";
            else TempData["Loi"] = loi;

            return BackToList();
        }

        // =====================================================================
        //  HÀM PHỤ
        // =====================================================================

        // Sau khi xóa thì quay lại đúng trang danh sách (giữ nguyên bộ lọc, số trang) nếu có thể
        private ActionResult BackToList()
        {
            if (Request.UrlReferrer != null && Url.IsLocalUrl(Request.UrlReferrer.PathAndQuery))
                return Redirect(Request.UrlReferrer.PathAndQuery);
            return RedirectToAction("Products");
        }

        // Gợi ý hãng / môn có sẵn cho ô nhập (vẫn gõ được giá trị mới)
        private void LoadLookups()
        {
            ViewBag.Brands = db.SanPhams.Select(p => p.LoaiGiay).Distinct().OrderBy(x => x).ToList();
            ViewBag.Sports = db.SanPhams.Select(p => p.Sport).Distinct().OrderBy(x => x).ToList();
        }

        // Các luật mà annotation không viết được
        private void ValidateProduct(ProductFormViewModel m, out double rating)
        {
            // Đánh giá nhận dạng chữ để tránh lệch dấu phẩy/chấm theo ngôn ngữ máy ("4,9" hay "4.9" đều được)
            string raw = string.IsNullOrWhiteSpace(m.RatingText) ? "0" : m.RatingText.Trim().Replace(',', '.');
            if (!double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out rating) ||
                rating < 0 || rating > 5)
            {
                rating = 0;
                ModelState.AddModelError("RatingText", "Đánh giá phải là số từ 0 đến 5");
            }

            if (m.Gender != "Nam" && m.Gender != "Nu")
                ModelState.AddModelError("Gender", "Vui lòng chọn giới tính");

            if (m.GiaTien.HasValue && m.GiaTienGoc.HasValue && m.GiaTienGoc.Value <= m.GiaTien.Value)
                ModelState.AddModelError("GiaTienGoc", "Giá gốc phải lớn hơn giá bán (hoặc để trống nếu không giảm giá)");
        }

        private static void Apply(ProductFormViewModel m, SanPham sp, double rating)
        {
            sp.ItemName = m.ItemName.Trim();
            sp.LoaiGiay = m.LoaiGiay.Trim();
            sp.Sport = m.Sport.Trim();
            sp.Gender = m.Gender;
            sp.GiaTien = Math.Round(m.GiaTien.Value, 0);
            sp.GiaTienGoc = m.GiaTienGoc.HasValue ? (decimal?)Math.Round(m.GiaTienGoc.Value, 0) : null;
            sp.ImgURL = NormalizeImgUrl(m.ImgURL);
            sp.DescriptionTxt = string.IsNullOrWhiteSpace(m.DescriptionTxt) ? null : m.DescriptionTxt.Trim();
            sp.rating = rating;
            sp.SanPhamMoi = m.SanPhamMoi;
        }

        // "~/ImageAssets/..", "ImageAssets\.." hay "/ImageAssets/.." đều thành "/ImageAssets/.."
        private static string NormalizeImgUrl(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            s = s.Trim().Replace('\\', '/');
            if (s.StartsWith("~")) s = s.Substring(1);
            if (!s.StartsWith("/")) s = "/" + s;
            return s;
        }

        // Lưu và trả về thông báo lỗi dễ đọc (null = thành công)
        private string TrySave()
        {
            try
            {
                db.SaveChanges();
                return null;
            }
            catch (DbEntityValidationException ex)
            {
                var chiTiet = ex.EntityValidationErrors
                                .SelectMany(e => e.ValidationErrors)
                                .Select(e => e.PropertyName + ": " + e.ErrorMessage);
                return "Dữ liệu không hợp lệ: " + string.Join(" | ", chiTiet);
            }
            catch (DbUpdateException ex)
            {
                return "Không lưu được vào database: " + ex.GetBaseException().Message;
            }
        }
    }
}
