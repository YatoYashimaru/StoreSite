using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StoreSite.Controllers;

namespace StoreSite.Models
{
    // Dữ liệu cho trang giỏ hàng: ghép từng CartItem (chỉ có id) với sản phẩm thật để có tên, giá, ảnh
    public class CartViewModel
    {
        private List<CartLine> lines = new List<CartLine>();

        public CartViewModel(Cart cart, List<SanPham> products)
        {
            foreach (var item in cart.TakeList())
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null) // sản phẩm không còn trong cửa hàng thì bỏ qua dòng đó
                    lines.Add(new CartLine(product, item.Size, item.Quantity));
            }
        }

        public List<CartLine> TakeLines() => lines.ToList();
        public CartLine FindLine(int productId, int size) =>
            lines.FirstOrDefault(l => l.Product.Id == productId && l.Size == size);
        public int SumQuantity() => lines.Sum(l => l.Quantity);
        public decimal TotalPrice() => lines.Sum(l => l.LineTotal());
    }
}