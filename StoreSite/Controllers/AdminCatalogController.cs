using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using StoreSite.Models;

namespace StoreSite.Controllers
{
    /// Quản lý nhãn hàng (SanPhams.LoaiGiay) và bộ môn (SanPhams.Sport).
    /// Hai thứ này lưu thẳng trong bảng SanPhams nên không cần tạo bảng mới:
    /// danh sách = các giá trị đang có, "đổi tên" = cập nhật mọi sản phẩm đang dùng giá trị đó.
    /// Mục mới chưa có sản phẩm được lưu bằng CatalogStore (nhãn hàng = folder ảnh, bộ môn = ExtraSports.txt).
    public class AdminCatalogController : AdminBaseController
    {
        public ActionResult Brands()
        {
            var vm = new CatalogViewModel { Kind = "brand", Title = "Nhãn hàng" };
            var rows = db.SanPhams.Where(p => p.LoaiGiay != null && p.LoaiGiay != "")
                         .GroupBy(p => p.LoaiGiay)
                         .Select(g => new { Name = g.Key, Count = g.Count() })
                         .ToList();

            // Nhãn hàng có folder ảnh nhưng chưa có sản phẩm cũng hiện ra (Số sản phẩm = 0)
            var names = CatalogStore.Merge(rows.Select(r => r.Name), CatalogStore.BrandFolders());
            vm.Items = names.Select(n =>
            {
                var r = rows.FirstOrDefault(x => string.Equals(x.Name, n, StringComparison.OrdinalIgnoreCase));
                string real = r != null ? r.Name : n;
                return new CatalogItem { Name = real, Label = real, Count = r != null ? r.Count : 0 };
            }).ToList();

            ViewBag.Title = vm.Title;
            return View("Catalog", vm);
        }

        public ActionResult Sports()
        {
            var vm = new CatalogViewModel { Kind = "sport", Title = "Bộ môn" };
            var rows = db.SanPhams.Where(p => p.Sport != null && p.Sport != "")
                         .GroupBy(p => p.Sport)
                         .Select(g => new { Name = g.Key, Count = g.Count() })
                         .ToList();

            var names = CatalogStore.Merge(rows.Select(r => r.Name), CatalogStore.ExtraSports());
            vm.Items = names.Select(n =>
            {
                var r = rows.FirstOrDefault(x => string.Equals(x.Name, n, StringComparison.OrdinalIgnoreCase));
                string real = r != null ? r.Name : n;
                return new CatalogItem { Name = real, Label = SportNames.Label(real), Count = r != null ? r.Count : 0 };
            }).ToList();

            ViewBag.Title = vm.Title;
            return View("Catalog", vm);
        }

        // Thêm nhãn hàng (tạo folder Men/Tên + Women/Tên) hoặc bộ môn (ghi vào ExtraSports.txt)
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Add(string kind, string name)
        {
            bool sport = kind == "sport";
            string back = sport ? "Sports" : "Brands";
            string what = sport ? "bộ môn" : "nhãn hàng";
            name = (name ?? "").Trim();

            string loi = CatalogStore.ValidateName(name, what);
            if (loi != null)
            {
                TempData["Loi"] = loi;
                return RedirectToAction(back);
            }

            bool exists = sport
                ? db.SanPhams.Any(p => p.Sport == name) || CatalogStore.ExtraSports().Contains(name, StringComparer.OrdinalIgnoreCase)
                : db.SanPhams.Any(p => p.LoaiGiay == name) || CatalogStore.BrandFolders().Contains(name, StringComparer.OrdinalIgnoreCase);
            if (exists)
            {
                TempData["Loi"] = "Đã có " + what + " “" + name + "”.";
                return RedirectToAction(back);
            }

            try
            {
                if (sport) CatalogStore.AddSport(name);
                else CatalogStore.CreateBrandFolders(name);
            }
            catch (Exception ex)
            {
                if (!(ex is IOException || ex is UnauthorizedAccessException)) throw;
                TempData["Loi"] = "Không tạo được " + what + ": " + ex.Message;
                return RedirectToAction(back);
            }

            TempData["ThongBao"] = sport
                ? "Đã thêm bộ môn “" + name + "”."
                : "Đã thêm nhãn hàng “" + name + "” và tạo folder ImageAssets/Men/" + name + ", ImageAssets/Women/" + name + ".";
            return RedirectToAction(back);
        }

        // Xóa nhãn hàng (xóa luôn 2 folder ảnh) hoặc bộ môn. Chỉ xóa được khi không còn sản phẩm nào dùng.
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Delete(string kind, string name)
        {
            bool sport = kind == "sport";
            string back = sport ? "Sports" : "Brands";
            string what = sport ? "bộ môn" : "nhãn hàng";
            name = (name ?? "").Trim();

            if (CatalogStore.ValidateName(name, what) != null)
            {
                TempData["Loi"] = "Tên " + what + " không hợp lệ.";
                return RedirectToAction(back);
            }

            int dangDung = sport
                ? db.SanPhams.Count(p => p.Sport == name)
                : db.SanPhams.Count(p => p.LoaiGiay == name);
            if (dangDung > 0)
            {
                TempData["Loi"] = "Không thể xóa " + what + " “" + name + "” vì còn " + dangDung
                                  + " sản phẩm đang dùng. Hãy xóa hoặc chuyển các sản phẩm đó sang mục khác trước.";
                return RedirectToAction(back);
            }

            try
            {
                if (sport) CatalogStore.RemoveSport(name);
                else CatalogStore.DeleteBrandFolders(name);
            }
            catch (Exception ex)
            {
                if (!(ex is IOException || ex is UnauthorizedAccessException)) throw;
                TempData["Loi"] = "Không xóa được " + what + ": " + ex.Message;
                return RedirectToAction(back);
            }

            TempData["ThongBao"] = sport
                ? "Đã xóa bộ môn “" + name + "”."
                : "Đã xóa nhãn hàng “" + name + "” và folder ảnh của nó.";
            return RedirectToAction(back);
        }

        // Đổi tên (hoặc gộp nếu tên mới đã tồn tại)
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Rename(string kind, string oldName, string newName)
        {
            string back = kind == "sport" ? "Sports" : "Brands";
            newName = (newName ?? "").Trim();

            if (string.IsNullOrEmpty(oldName) || newName.Length == 0 || newName.Length > 80)
            {
                TempData["Loi"] = "Tên mới không hợp lệ (1 - 80 ký tự).";
                return RedirectToAction(back);
            }
            if (newName == oldName)
            {
                return RedirectToAction(back);
            }

            int n;
            if (kind == "sport")
            {
                var list = db.SanPhams.Where(p => p.Sport == oldName).ToList();
                foreach (var p in list) p.Sport = newName;
                n = list.Count;
            }
            else
            {
                var list = db.SanPhams.Where(p => p.LoaiGiay == oldName).ToList();
                foreach (var p in list) p.LoaiGiay = newName;
                n = list.Count;
            }
            db.SaveChanges();

            TempData["ThongBao"] = "Đã đổi “" + oldName + "” thành “" + newName + "” cho " + n + " sản phẩm.";
            return RedirectToAction(back);
        }
    }
}
