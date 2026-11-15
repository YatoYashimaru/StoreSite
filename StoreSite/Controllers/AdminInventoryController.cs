using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StoreSite.Controllers
{
    public class AdminInventoryController : AdminBaseController
    {
        // GET: /AdminInventory
        public ActionResult Index(string search, string status)
        {
            var query = db.SanPhams.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.Trim();
                query = query.Where(p => p.ItemName.Contains(s) || p.LoaiGiay.Contains(s));
            }
            if (status == "out")
                query = query.Where(p => p.SoLuongTon <= 0);
            else if (status == "low")
                query = query.Where(p => p.SoLuongTon > 0 && p.SoLuongTon <= 5);
            else if (status == "in")
                query = query.Where(p => p.SoLuongTon > 0);
            var list = query.OrderBy(p => p.SoLuongTon).ThenBy(p => p.Id).ToList();
            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.TotalOut = db.SanPhams.Count(p => p.SoLuongTon <= 0);
            ViewBag.TotalStock = db.SanPhams.Sum(p => (int?)p.SoLuongTon) ?? 0;
            return View(list);
        }

        // Tăng hoặc giảm nhanh số lượng (+1, -1)
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult AdjustStock(int id, int delta)
        {
            var sp = db.SanPhams.Find(id);
            if (sp == null) return HttpNotFound();
            int newQty = sp.SoLuongTon + delta;
            if (newQty < 0) newQty = 0; // Về 0 thì giữ nguyên bản ghi ở mức 0, không xóa
            sp.SoLuongTon = newQty;
            db.SaveChanges();
            TempData["ThongBao"] = "Đã cập nhật tồn kho “" + sp.ItemName + "”: " + sp.SoLuongTon + " đôi.";
            return BackToList();
        }
        // Nhập trực tiếp số lượng cụ thể (có thể nhập 0 để đánh dấu hết hàng)
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SetStock(int id, int quantity)
        {
            var sp = db.SanPhams.Find(id);
            if (sp == null) return HttpNotFound();
            if (quantity < 0) quantity = 0;
            sp.SoLuongTon = quantity;
            db.SaveChanges();
            TempData["ThongBao"] = "Đã đặt số lượng tồn “" + sp.ItemName + "” thành " + sp.SoLuongTon + " đôi.";
            return BackToList();
        }

        private ActionResult BackToList()
        {
            if (Request.UrlReferrer != null && Url.IsLocalUrl(Request.UrlReferrer.PathAndQuery))
                return Redirect(Request.UrlReferrer.PathAndQuery);
            return RedirectToAction("Index");
        }
    }
}