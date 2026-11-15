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
        public List<ProductSalesItem> SanPhamBanChay { get; set; }
        public List<ProductSalesItem> SanPhamBanE { get; set; }

        // Tổng số sản phẩm trong biểu đồ
        public int TongBieuDo()
        {
            return BieuDoHangGiay == null ? 0 : BieuDoHangGiay.Sum(x => x.Value);
        }

        // Màu của phần thứ index
        public string MauCua(int index)
        {
            return colors[index % colors.Length];
        }

        // Phần trăm (làm tròn) của một phần, để ghi trong chú thích
        public int PhanTram(ChartItem item)
        {
            int total = TongBieuDo();
            return total == 0 ? 0 : (int)Math.Round(100.0 * item.Value / total);
        }

        // Chuỗi CSS cho conic-gradient, ví dụ: "#1b7f9e 0% 40%, #f0a030 40% 100%"
        // Dùng tỉ lệ chính xác (không làm tròn) để các phần khít nhau, không hở.
        public string ChuoiMauBieuDo()
        {
            int total = TongBieuDo();
            if (total == 0) return "#e5e7eb 0% 100%";

            var inv = CultureInfo.InvariantCulture;
            var parts = new List<string>();
            double from = 0;
            for (int i = 0; i < BieuDoHangGiay.Count; i++)
            {
                double to = (i == BieuDoHangGiay.Count - 1) ? 100.0 : from + 100.0 * BieuDoHangGiay[i].Value / total;
                parts.Add(MauCua(i) + " " + from.ToString("0.##", inv) + "% " + to.ToString("0.##", inv) + "%");
                from = to;
            }
            return string.Join(", ", parts);
        }
    }
}