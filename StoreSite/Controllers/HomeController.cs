using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StoreSite.Models;

namespace StoreSite.Controllers
{
    public class HomeController : BaseController
    {
        private const int PageSize = 4;

        private FeaturedViewModel BuildFeatured(int page)
        {
            var query = db.SanPhams.Where(p => p.SanPhamMoi)
                                   .OrderByDescending(p => p.rating).ThenBy(p => p.Id);

            int totalPages = Math.Max(1, (int)Math.Ceiling(query.Count() / (double)PageSize));
            page = Math.Min(Math.Max(page, 1), totalPages);

            return new FeaturedViewModel
            {
                Items = query.Skip((page - 1) * PageSize).Take(PageSize).ToList(),
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