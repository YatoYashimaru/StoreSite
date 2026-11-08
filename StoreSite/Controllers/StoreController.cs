using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StoreSite.Models;

namespace StoreSite.Controllers
{
    public class StoreController : BaseController
    {
        public ActionResult Index(string TimKiem, string Loai, string Gender, string Sport, string sortOrder)
        {
            ViewBag.TimKiem = TimKiem; ViewBag.Loai = Loai; ViewBag.Gender = Gender;
            ViewBag.Sport = Sport; ViewBag.Sort = sortOrder;

            // danh sách môn lấy từ database, thêm môn mới thì tự có trong bộ lọc
            ViewBag.Sports = db.SanPhams.Select(p => p.Sport).Distinct().OrderBy(s => s).ToList();

            IQueryable<SanPham> q = db.SanPhams;
            if (!string.IsNullOrEmpty(TimKiem)) q = q.Where(p => p.ItemName.Contains(TimKiem)); // sort tên
            if (!string.IsNullOrEmpty(Loai) && Loai != "All") q = q.Where(p => p.LoaiGiay == Loai); // sort loại giày
            if (Gender == "Nam" || Gender == "Nu") q = q.Where(p => p.Gender == Gender); // sort gender
            if (!string.IsNullOrEmpty(Sport) && Sport != "All") q = q.Where(p => p.Sport == Sport); // sort sport

            switch (sortOrder)
            {
                case "Tang_Dan": q = q.OrderBy(p => p.GiaTien); break;
                case "Giam_Dan": q = q.OrderByDescending(p => p.GiaTien); break;
                default: q = q.OrderBy(p => p.Id); break;
            }
            return View(q.ToList());
        }

        public ActionResult Details(int? id)
        {
            var sp = db.SanPhams.Find(id);
            if (sp == null) return HttpNotFound();

            return View(new ProductDetailsViewModel
            {
                Product = sp,
                Related = db.SanPhams
                            .Where(p => p.Id != id && (p.LoaiGiay == sp.LoaiGiay || p.Gender == sp.Gender))
                            .OrderByDescending(p => p.rating).ThenBy(p => p.Id)
                            .Take(4).ToList()
            });
        }
        // GET: Store
        [ChildActionOnly]
        public ActionResult MegaMenu()
        {
            var vm = new MegaMenuViewModel();

            try
            {
                var all = db.SanPhams.ToList();   // hàm lấy danh sách sản phẩm trong controller

                vm.BrandsNam = all.Where(p => p.Gender == "Nam" && !string.IsNullOrEmpty(p.LoaiGiay))
                                  .Select(p => p.LoaiGiay).Distinct().OrderBy(b => b).ToList();

                vm.BrandsNu = all.Where(p => p.Gender == "Nu" && !string.IsNullOrEmpty(p.LoaiGiay))
                                 .Select(p => p.LoaiGiay).Distinct().OrderBy(b => b).ToList();

                vm.Sports = all.Select(p => p.Sport)
                                .Where(s => !string.IsNullOrEmpty(s))
                                .Distinct().OrderBy(s => s).ToList();

                vm.Featured = all.Where(p => p.SanPhamMoi)
                                 .OrderByDescending(p => p.rating).ThenBy(p => p.Id)
                                 .Take(2).ToList();
            }
            catch
            {
                // lỗi dữ liệu thì menu để trống, trang web vẫn chạy bình thường
            }

            return PartialView("_MegaMenu", vm);
        }
    }
}
