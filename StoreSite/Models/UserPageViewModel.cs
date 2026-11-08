using System.Collections.Generic;

namespace StoreSite.Models
{
    public class UserPageViewModel
    {
        public string HoTen { get; set; }
        public string Email { get; set; }
        public System.Collections.Generic.List<Order> Orders { get; set; }
    }
}