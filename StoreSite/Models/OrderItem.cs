using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StoreSite.Models
{
    // Một dòng trong đơn hàng. Là bản SAO của sản phẩm lúc khách đặt (tên, giá, ảnh),
    // nên sau này giá hay tên sản phẩm có đổi thì đơn cũ vẫn giữ nguyên.
    public class OrderItem
    {
        public string ProductName { get; private set; }
        public string ImgURL { get; private set; }
        public decimal UnitPrice { get; private set; }
        public int Size { get; private set; }
        public int Quantity { get; private set; }

        public OrderItem(string productName, string imgURL, decimal unitPrice, int size, int quantity)
        {
            ProductName = productName;
            ImgURL = imgURL;
            UnitPrice = unitPrice;
            Size = size;
            Quantity = quantity;
        }

        public decimal LineTotal() => UnitPrice * Quantity; // Thành tiền của dòng này
    }
}