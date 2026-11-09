using System.Linq;
using System.Web.Mvc;
using StoreSite.Models;

namespace StoreSite.Controllers
{
    /// <summary>
    /// Quản lý nhãn hàng (SanPhams.LoaiGiay) và bộ môn (SanPhams.Sport).
    /// Hai thứ này lưu thẳng trong bảng SanPhams nên không cần tạo bảng mới:
    /// danh sách = các giá trị đang có, "đổi tên" = cập nhật mọi sản phẩm đang dùng giá trị đó.
    /// </summary>
    public class AdminCatalogController : AdminBaseController
    {
        public ActionResult Brands()
        {
            var vm = new CatalogViewModel { Kind = "brand", Title = "Nhãn hàng" };
            vm.Items = db.SanPhams.Where(p => p.LoaiGiay != null && p.LoaiGiay != "")
                         .GroupBy(p => p.LoaiGiay)
                         .Select(g => new CatalogItem { Name = g.Key, Label = g.Key, Count = g.Count() })
                         .OrderBy(x => x.Name).ToList();
            ViewBag.Title = vm.Title;
            return View("Catalog", vm);
        }

        public ActionResult Sports()
        {
            var vm = new CatalogViewModel { Kind = "sport", Title = "Bộ môn" };
            var rows = db.SanPhams.Where(p => p.Sport != null && p.Sport != "")
                         .GroupBy(p => p.Sport)
                         .Select(g => new { Name = g.Key, Count = g.Count() })
                         .OrderBy(x => x.Name).ToList();
            vm.Items = rows.Select(r => new CatalogItem
            {
                Name = r.Name,
                Label = SportNames.Label(r.Name),
                Count = r.Count
            }).ToList();
            ViewBag.Title = vm.Title;
            return View("Catalog", vm);
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
