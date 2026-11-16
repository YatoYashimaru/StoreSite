using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StoreSite.Controllers;

namespace StoreSite.Models
{
    public class ProductDetailsViewModel
    {
        public SanPham Product { get; set; }
        public List<SanPham> Related { get; set; }

        // ----- Đánh giá -----
        public List<ReviewItem> Reviews { get; set; } = new List<ReviewItem>();
        public int ReviewCount { get; set; }
        public ReviewItem MyReview { get; set; }   // đánh giá của chính người đang xem (nếu có)
        public bool LoggedIn { get; set; }
        public bool IsAdmin { get; set; }
    }

    public class ReviewItem
    {
        public int ReviewID { get; set; }
        public string UserName { get; set; }
        public int Stars { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsMine { get; set; }
    }
}