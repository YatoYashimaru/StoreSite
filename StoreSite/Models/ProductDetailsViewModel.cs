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
        public ProductTheme Theme { get; set; }
    }
}