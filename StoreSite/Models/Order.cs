using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StoreSite.Models
{
    // Một đơn hàng đã đặt
    public class Order
    {
        private List<OrderItem> items = new List<OrderItem>();

        public int Id { get; private set; }
        public int UserId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public string Status { get; private set; }
        public string ReceiverName { get; private set; }
        public string Phone { get; private set; }
        public string Address { get; private set; }
        public string Note { get; private set; }
        public string PaymentMethod { get; private set; } // Trả bằng tiền mặt (cod) hoặc chuyển khoản (bank)

        // Tạo đơn từ giỏ hàng: sao chép từng dòng của giỏ thành OrderItem
        public Order(int id, int userId, string receiverName, string phone, string address,
                     string note, string paymentMethod, CartViewModel cart)
        {
            Id = id;
            UserId = userId;
            CreatedAt = DateTime.Now;
            Status = "Chờ xác nhận";
            ReceiverName = receiverName;
            Phone = phone;
            Address = address;
            Note = note;
            PaymentMethod = paymentMethod;

            foreach (var line in cart.TakeLines())
                items.Add(new OrderItem(line.Product.Name, line.Product.ImgURL, line.Product.GiaTien, line.Size, line.Quantity));
        }

        public List<OrderItem> TakeItems() => items.ToList();
        public int SumQuantity() => items.Sum(i => i.Quantity);
        public decimal TotalPrice() => items.Sum(i => i.LineTotal());
        public string PaymentMethodText() => PaymentMethod == "bank" ? "Chuyển khoản ngân hàng" : "Thanh toán khi nhận hàng";
        // Mô tả ngắn để hiện trong danh sách đơn: tên sản phẩm đầu tiên (+ "và N sản phẩm khác" nếu còn nữa)
        public string ShortDescription()
        {
            if (items.Count == 0) return "";
            if (items.Count == 1) return items[0].ProductName;
            return items[0].ProductName + " và " + (items.Count - 1) + " sản phẩm khác";
        }
    }
}