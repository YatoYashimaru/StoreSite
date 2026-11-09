using System;
using System.Collections.Generic;

namespace StoreSite.Models
{
    public class AdminOrderRow
    {
        public int OrderID { get; set; }
        public DateTime? OrderDate { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public decimal? TotalAmount { get; set; }
        public string PaymentStatus { get; set; }
        public int ItemCount { get; set; }
    }

    public class AdminOrderListViewModel
    {
        public List<AdminOrderRow> Items { get; set; } = new List<AdminOrderRow>();
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public int Total { get; set; }

        public string Status { get; set; }     // "", "Pending", "Complete"
        public string Search { get; set; }

        public int CountAll { get; set; }
        public int CountPending { get; set; }
        public int CountComplete { get; set; }
        public decimal Revenue { get; set; }   // tổng tiền các đơn đã thanh toán
    }

    public class AdminOrderLine
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ImgURL { get; set; }
        public string Size { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get { return UnitPrice * Quantity; } }
    }

    public class AdminOrderDetailViewModel
    {
        public int OrderID { get; set; }
        public DateTime? OrderDate { get; set; }
        public string PaymentStatus { get; set; }
        public decimal? TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string Note { get; set; }

        public string ReceiverName { get; set; }
        public string ReceiverPhone { get; set; }
        public string AddressDelivery { get; set; }

        public string AccountName { get; set; }
        public string AccountEmail { get; set; }

        public List<AdminOrderLine> Lines { get; set; } = new List<AdminOrderLine>();
    }
}
