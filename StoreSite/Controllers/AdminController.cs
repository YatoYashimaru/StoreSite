using System;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using StoreSite.Models;

namespace StoreSite.Controllers
{
    public class AdminController : AdminBaseController
    {
        private const int PageSize = 10;

        // /Admin  ->  chuyển sang trang Tổng quan
        public ActionResult Index()
        {
            return RedirectToAction("Index", "AdminDashboard");
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
            return View("ProductForm", new ProductFormViewModel { Gender = "Nam" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ProductCreate(ProductFormViewModel m)
        {
            ValidateProduct(m);
            var plan = PlanImages(m, null);

            if (ModelState.IsValid)
            {
                var sp = new SanPham();
                Apply(m, sp);

                string loiAnh = RunImages(plan, sp);
                if (loiAnh == null)
                {
                    db.SanPhams.Add(sp);
                    string loi = TrySave();
                    if (loi == null)
                    {
                        TempData["ThongBao"] = "Đã thêm sản phẩm “" + sp.ItemName + "” và lưu ảnh vào " + ProductImageStore.RelativeText(plan.Folder);
                        return RedirectToAction("Products");
                    }

                    // lưu database lỗi: bỏ thư mục vừa tạo để không để lại ảnh mồ côi
                    if (plan.IsNewFolder)
                    {
                        try { Directory.Delete(plan.Folder, true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
                    }
                    ModelState.AddModelError("", loi);
                }
                else
                {
                    ModelState.AddModelError("", loiAnh);
                }
            }

            LoadLookups();
            FillImageInfo(m, null);
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
            var form = new ProductFormViewModel
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
                SanPhamMoi = sp.SanPhamMoi
            };
            FillImageInfo(form, sp);
            return View("ProductForm", form);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ProductEdit(ProductFormViewModel m)
        {
            var sp = db.SanPhams.Find(m.Id);
            if (sp == null) return HttpNotFound();

            ValidateProduct(m);
            var plan = PlanImages(m, sp);

            if (ModelState.IsValid)
            {
                Apply(m, sp);

                string loiAnh = RunImages(plan, sp);
                if (loiAnh == null)
                {
                    string loi = TrySave();
                    if (loi == null)
                    {
                        TempData["ThongBao"] = "Đã lưu thay đổi cho “" + sp.ItemName + "”";
                        return RedirectToAction("Products");
                    }
                    ModelState.AddModelError("", loi);
                }
                else
                {
                    ModelState.AddModelError("", loiAnh);
                }
            }

            LoadLookups();
            FillImageInfo(m, sp);
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
            // Gồm cả nhãn hàng / bộ môn mới thêm ở trang Danh mục mà chưa có sản phẩm nào
            ViewBag.Brands = CatalogStore.Merge(db.SanPhams.Select(p => p.LoaiGiay).Distinct().ToList(),
                                                CatalogStore.BrandFolders());
            ViewBag.Sports = CatalogStore.Merge(db.SanPhams.Select(p => p.Sport).Distinct().ToList(),
                                                CatalogStore.ExtraSports());
        }

        // Các luật mà annotation không viết được
        private void ValidateProduct(ProductFormViewModel m)
        {
            if (m.Gender != "Nam" && m.Gender != "Nu")
                ModelState.AddModelError("Gender", "Vui lòng chọn giới tính");

            if (m.GiaTien.HasValue && m.GiaTienGoc.HasValue && m.GiaTienGoc.Value <= m.GiaTien.Value)
                ModelState.AddModelError("GiaTienGoc", "Giá gốc phải lớn hơn giá bán (hoặc để trống nếu không giảm giá)");
        }

        // =====================================================================
        //  ẢNH SẢN PHẨM
        // =====================================================================

        /// <summary>
        /// Kiểm tra phần ảnh của form (chưa ghi gì xuống đĩa); lỗi được thêm vào ModelState.
        /// sp = null khi thêm mới, là sản phẩm đang sửa khi chỉnh sửa.
        /// </summary>
        private ImagePlan PlanImages(ProductFormViewModel m, SanPham sp)
        {
            var plan = new ImagePlan { Uploads = ProductImageStore.ReadUploads(Request) };
            string[] danhDauXoa = Request.Form.GetValues("removeImages") ?? new string[0];

            string brand = (m.LoaiGiay ?? "").Trim();
            string loiHang = CatalogStore.ValidateName(brand, "hãng");
            if (loiHang != null)
            {
                if (ModelState.IsValid) ModelState.AddModelError("LoaiGiay", loiHang);
                return plan;
            }
            if (!ModelState.IsValid) return plan;     // lỗi khác trước, sửa xong rồi mới kiểm tra ảnh

            // Đang sửa: dùng thư mục ảnh hiện có của sản phẩm (không tự đổi tên thư mục khi sửa tên / hãng)
            string folder = sp == null ? null : ProductImageStore.CurrentFolder(sp.ImgURL);

            if (folder == null)
            {
                // Sản phẩm cũ có ảnh nằm ngoài cấu trúc thư mục và không tải ảnh mới: giữ nguyên
                if (sp != null && plan.Uploads.Count == 0)
                {
                    plan.Skip = true;
                    return plan;
                }

                folder = ProductImageStore.FolderPath(m.Gender, brand, m.ItemName);
                if (ProductImageStore.InUseByOther(db, folder, sp == null ? 0 : sp.Id))
                {
                    ModelState.AddModelError("ItemName", "Thư mục ảnh “" + ProductImageStore.RelativeText(folder)
                        + "” đang được sản phẩm khác dùng. Hãy đổi tên sản phẩm.");
                    return plan;
                }
            }

            plan.Folder = folder;
            plan.IsNewFolder = !Directory.Exists(folder);

            var coSan = ProductImageStore.ListFiles(folder);
            plan.Remove = coSan.Where(f => danhDauXoa.Contains(f, StringComparer.OrdinalIgnoreCase)).ToList();
            int giuLai = coSan.Count - plan.Remove.Count;

            string loiTai = ProductImageStore.ValidateUploads(plan.Uploads, giuLai);
            if (loiTai != null)
                ModelState.AddModelError("images", loiTai);
            else if (giuLai + plan.Uploads.Count == 0)
                ModelState.AddModelError("images", sp == null
                    ? "Vui lòng chọn ít nhất 1 ảnh cho sản phẩm."
                    : "Sản phẩm phải còn ít nhất 1 ảnh.");

            return plan;
        }

        /// <summary>Ghi ảnh xuống đĩa theo kế hoạch rồi gán sp.ImgURL = ảnh số 1. Trả về thông báo lỗi hoặc null.</summary>
        private string RunImages(ImagePlan plan, SanPham sp)
        {
            if (plan.Skip) return null;

            try
            {
                if (plan.IsNewFolder) CatalogStore.CreateBrandFolders(Path.GetFileName(Path.GetDirectoryName(plan.Folder)));
                string first = ProductImageStore.Execute(plan);
                if (first != null) sp.ImgURL = ProductImageStore.UrlOf(plan.Folder, first);
                return null;
            }
            catch (Exception ex)
            {
                if (!(ex is IOException || ex is UnauthorizedAccessException)) throw;
                return "Không lưu được ảnh: " + ex.Message;
            }
        }

        // Điền thông tin ảnh hiện có cho form (chỉ khi sửa)
        private void FillImageInfo(ProductFormViewModel m, SanPham sp)
        {
            m.ExistingImages = new System.Collections.Generic.List<ExistingImage>();
            if (sp == null) return;

            m.ImgURL = sp.ImgURL;
            string folder = ProductImageStore.CurrentFolder(sp.ImgURL);
            if (folder == null) return;

            m.ImageFolderText = ProductImageStore.RelativeText(folder);
            foreach (var f in ProductImageStore.ListFiles(folder))
            {
                m.ExistingImages.Add(new ExistingImage
                {
                    FileName = f,
                    Url = Url.Content("~" + ProductImageStore.UrlOf(folder, f))
                });
            }
        }

        private static void Apply(ProductFormViewModel m, SanPham sp)
        {
            sp.ItemName = m.ItemName.Trim();
            sp.LoaiGiay = m.LoaiGiay.Trim();
            sp.Sport = m.Sport.Trim();
            sp.Gender = m.Gender;
            sp.GiaTien = Math.Round(m.GiaTien.Value, 0);
            sp.GiaTienGoc = m.GiaTienGoc.HasValue ? (decimal?)Math.Round(m.GiaTienGoc.Value, 0) : null;
            sp.DescriptionTxt = string.IsNullOrWhiteSpace(m.DescriptionTxt) ? null : m.DescriptionTxt.Trim();
            sp.SanPhamMoi = m.SanPhamMoi;
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
