using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StoreSite.Controllers;

namespace StoreSite.Models
{
    public class FeaturedViewModel
    {
        public List<SanPham> Items { get; set; }
        public int Page { get; set; }
        public int TotalPages { get; set; }

        public bool HasPrev { get { return Page > 1; } }
        public bool HasNext { get { return Page < TotalPages; } }
    }

}