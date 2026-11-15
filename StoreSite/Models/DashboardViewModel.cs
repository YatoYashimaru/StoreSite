using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace StoreSite.Models
{
    // Class phụ để chứa dữ liệu từng phần của biểu đồ (Ví dụ: "Nike" - 50 đôi)
    public class ChartItem
    {
        public string Label { get; set; }
        public int Value { get; set; }
    }

    // Dữ liệu biểu đồ cột (Doanh thu theo ngày)
    public class RevenueChartItem
    {
        public string Label { get; set; }
        public decimal Revenue { get; set; }
    }

    // Dữ liệu bảng sản phẩm bán chạy / bán ế
    public class ProductSalesItem
    {
        public int Id { get; set; }
        public string ItemName { get; set; }
        public string LoaiGiay { get; set; }
        public string ImgURL { get; set; }
        public decimal GiaTien { get; set; }
        public int SoLuongBan { get; set; }
        public decimal DoanhThu { get; set; }
    }

    // Class chính chứa toàn bộ số liệu của trang Thống kê
    public class DashboardViewModel
    {
        // Bảng màu cho các phần của biểu đồ tròn (hết màu thì quay lại từ đầu)
        private static readonly string[] colors =
        {
            "#1b7f9e", "#f0a030", "#5cb85c", "#d9534f", "#7e57c2", "#26a69a", "#ec407a", "#8d6e63"
        };

        public decimal DoanhThu { get; set; }
        public int TongDonHang { get; set; }
        public int TongKhachHang { get; set; }
        public int TongSanPham { get; set; }

        // Danh sách để vẽ biểu đồ tròn (Pie Chart)
        public List<ChartItem> BieuDoHangGiay { get; set; }
        public List<RevenueChartItem> BieuDoDoanhThu { get; set; }
        public List<RevenueChartItem> BieuDoDoanhThuThang { get; set; }
        public List<RevenueChartItem> BieuDoDoanhThuNam { get; set; }
        public List<ProductSalesItem> SanPhamBanChay { get; set; }
        public List<ProductSalesItem> SanPhamBanE { get; set; }

        // Tổng số sản phẩm trong biểu đồ
        public int TongBieuDo()
        {
            return BieuDoHangGiay == null ? 0 : BieuDoHangGiay.Sum(x => x.Value);
        }
    }
}