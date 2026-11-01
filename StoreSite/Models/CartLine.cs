using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StoreSite.Controllers;

namespace StoreSite.Models
{
    public class CartLine
    {
        public SanPham Product { get; private set; }
        public int Size { get; private set; }
        public int Quantity { get; private set; }

        public CartLine(SanPham product, int size, int quantity)
        {
            Product = product;
            Size = size;
            Quantity = quantity;
        }

        public decimal LineTotal() => Product.GiaTien * Quantity; // Thành tiền của dòng này
    }
}