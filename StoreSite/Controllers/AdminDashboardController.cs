using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StoreSite.Models;

namespace StoreSite.Controllers
{
    public class AdminDashboardController : AdminBaseController
    {
        public ActionResult Index()
        {
            var vm = new DashboardViewModel();

            // 1. Đếm các con số tổng quan cho các thẻ (Cards) trên cùng
            vm.TongDonHang = db.Orders.Count();
            vm.TongKhachHang = db.Users.Count(u => !u.UserRole);
            vm.TongSanPham = db.SanPhams.Count();

            // Doanh thu: Chỉ cộng dồn những đơn đã thanh toán (Complete)
            // Dùng ?? 0m để đề phòng trường hợp chưa có đơn nào thì trả về 0
            vm.DoanhThu = db.Orders.Where(o => o.PaymentStatus == "Complete")
                                   .Sum(o => (decimal?)o.TotalAmount) ?? 0m;

            // 2. Gom nhóm dữ liệu để vẽ biểu đồ tròn theo Hãng giày (LoaiGiay)
            vm.BieuDoHangGiay = db.SanPhams
                                  .Where(p => p.LoaiGiay != null && p.LoaiGiay != "")
                                  .GroupBy(p => p.LoaiGiay)
                                  .Select(g => new ChartItem
                                  {
                                      Label = g.Key,
                                      Value = g.Count()
                                  })
                                  .OrderByDescending(x => x.Value)
                                  .ToList();

            // 3. Biểu đồ cột: Doanh thu theo ngày (10 ngày gần nhất có đơn hoàn thành)
            var donHoanThanh = db.Orders
                                 .Where(o => o.PaymentStatus == "Complete" && o.OrderDate != null)
                                 .Select(o => new { o.OrderDate, o.TotalAmount })
                                 .ToList();

            var doanhThuTheoNgay = donHoanThanh
                                   .GroupBy(o => o.OrderDate.Date)
                                   .OrderBy(g => g.Key)
                                   .Select(g => new RevenueChartItem
                                   {
                                       Label = g.Key.ToString("dd/MM"),
                                       Revenue = g.Sum(x => (decimal?)x.TotalAmount ?? 0m)
                                   })
                                   .ToList();
            vm.BieuDoDoanhThu = doanhThuTheoNgay.Skip(Math.Max(0, doanhThuTheoNgay.Count - 10)).ToList();

            // Theo tháng (12 tháng gần nhất có đơn hoàn thành)
            var doanhThuTheoThang = donHoanThanh
                                    .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                                    .Select(g => new RevenueChartItem
                                    {
                                        Label = g.Key.Month.ToString("00") + "/" + g.Key.Year,
                                        Revenue = g.Sum(x => (decimal?)x.TotalAmount ?? 0m)
                                    })
                                    .ToList();
            vm.BieuDoDoanhThuThang = doanhThuTheoThang.Skip(Math.Max(0, doanhThuTheoThang.Count - 12)).ToList();

            // Theo năm
            vm.BieuDoDoanhThuNam = donHoanThanh
                                   .GroupBy(o => o.OrderDate.Year)
                                   .OrderBy(g => g.Key)
                                   .Select(g => new RevenueChartItem
                                   {
                                       Label = "Năm " + g.Key,
                                       Revenue = g.Sum(x => (decimal?)x.TotalAmount ?? 0m)
                                   })
                                   .ToList();

            // 4. Thống kê Sản phẩm bán chạy & bán ế (chỉ tính trên đơn đã thanh toán)
            var chiTietDaBan = (from d in db.OrderDetails
                                join o in db.Orders on d.OrderID equals o.OrderID
                                where o.PaymentStatus == "Complete"
                                select new { d.ProductID, d.Quantity, d.UnitPrice })
                               .ToList();

            var thongKeTheoSP = chiTietDaBan
                .GroupBy(x => Convert.ToInt32(x.ProductID))
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        SoLuong = g.Sum(x => Convert.ToInt32(x.Quantity)),
                        DoanhThu = g.Sum(x => Convert.ToInt32(x.Quantity) * Convert.ToDecimal(x.UnitPrice))
                    }
                );

            var danhSachBan = db.SanPhams.ToList().Select(sp =>
            {
                var info = thongKeTheoSP.ContainsKey(sp.Id) ? thongKeTheoSP[sp.Id] : null;
                return new ProductSalesItem
                {
                    Id = sp.Id,
                    ItemName = sp.ItemName,
                    LoaiGiay = sp.LoaiGiay,
                    ImgURL = sp.ImgURL,
                    GiaTien = sp.GiaTien,
                    SoLuongBan = info != null ? info.SoLuong : 0,
                    DoanhThu = info != null ? info.DoanhThu : 0m
                };
            }).ToList();

            // Top 5 bán chạy (có ít nhất 1 sản phẩm đã bán)
            vm.SanPhamBanChay = danhSachBan
                .Where(x => x.SoLuongBan > 0)
                .OrderByDescending(x => x.SoLuongBan)
                .ThenByDescending(x => x.DoanhThu)
                .Take(5)
                .ToList();

            // Top 5 bán ế (số lượng bán ít nhất, tính cả sản phẩm bán được 0 đôi)
            vm.SanPhamBanE = danhSachBan
                .OrderBy(x => x.SoLuongBan)
                .ThenBy(x => x.Id)
                .Take(5)
                .ToList();

            ViewBag.Title = "Thống kê tổng quan";
            return View(vm);
        }
    }
}