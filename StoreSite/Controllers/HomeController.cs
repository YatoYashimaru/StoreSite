using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StoreSite.Models;

namespace StoreSite.Controllers
{
    public class HomeController : Controller
    {
        private const int PageSize = 4;

        private FeaturedViewModel BuildFeatured(int page)
        {
            var all = StoreController.GetSanPham()
                        .Where(p => p.SanPhamMoi)
                        .OrderByDescending(p => p.rating)
                        .ToList();

            int totalPages = Math.Max(1, (int)Math.Ceiling(all.Count / (double)PageSize));
            page = Math.Min(Math.Max(page, 1), totalPages);

            return new FeaturedViewModel
            {
                Items = all.Skip((page - 1) * PageSize).Take(PageSize).ToList(),
                Page = page,
                TotalPages = totalPages
            };
        }

        public ActionResult Index()
        {
            return View(BuildFeatured(1));
        }

        //  trả về partial của trang kế/trước
        public ActionResult FeaturedPage(int page = 1)
        {
            return PartialView("_FeaturedProducts", BuildFeatured(page));
        }

    }
}