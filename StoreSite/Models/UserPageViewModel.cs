using System.Collections.Generic;

namespace StoreSite.Models
{
    public class UserPageViewModel
    {
        public string HoTen { get; set; }
        public string Email { get; set; }
        public List<Order> Orders { get; set; } = new List<Order>(); // chỉ có dữ liệu khi mở tab "Đơn hàng của tôi"
    }
}