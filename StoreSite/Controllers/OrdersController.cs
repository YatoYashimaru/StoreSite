using StoreSite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace StoreSite.Controllers
{
    public class OrdersController : Controller
    {
        // Danh sách đơn hàng tạm trong bộ nhớ (giống DanhSachNguoiDung bên UserController)
        private static List<Order> DanhSachDonHang = new List<Order>();

        // Lấy người dùng đang đăng nhập từ Session, chưa đăng nhập thì trả về null
        private NguoiDung LayNguoiDungHienTai()
        {
            var id = Session["NguoiDungId"] as int?;
            if (id == null) return null;
            return UserController.TimTheoId(id.Value);
        }

        // Chưa đăng nhập: chuyển sang trang đăng nhập, đăng nhập xong sẽ quay lại returnUrl
        private ActionResult ChuyenSangDangNhap(string thongBao, string returnUrl)
        {
            TempData["ThongBao"] = thongBao;
            return RedirectToAction("Login", "User", new { returnUrl = returnUrl });
        }

        // Hiện trang thanh toán với các ô đã điền sẵn (dùng cho lần mở đầu tiên và cả khi nhập sai)
        private ActionResult ShowPaymentPage(Cart cart, string receiverName, string phone, string address, string note, string paymentMethod)
        {
            ViewBag.ReceiverName = receiverName;
            ViewBag.Phone = phone;
            ViewBag.Address = address;
            ViewBag.Note = note;
            ViewBag.PaymentMethod = paymentMethod;
            return View("PaymentPage", new CartViewModel(cart, StoreController.GetSanPham()));
        }

        // Trang thanh toán: GET /Orders/PaymentPage
        public ActionResult PaymentPage()
        {
            var nd = LayNguoiDungHienTai();
            if (nd == null)
                return ChuyenSangDangNhap("Vui lòng đăng nhập để thanh toán.", Url.Action("PaymentPage", "Orders"));

            var cart = Session["Cart"] as Cart;
            if (cart == null || cart.SumQuantity() == 0)
                return RedirectToAction("Index", "Cart");

            return ShowPaymentPage(cart, nd.HoTen, nd.SDT, "", "", "cod");
        }

        // Bấm "Đặt hàng": POST /Orders/PlaceOrder
        [HttpPost]
        public ActionResult PlaceOrder(string ReceiverName, string Phone, string Address, string Note, string PaymentMethod)
        {
            var nd = LayNguoiDungHienTai();
            if (nd == null)
                return ChuyenSangDangNhap("Vui lòng đăng nhập để thanh toán.", Url.Action("PaymentPage", "Orders"));

            var cart = Session["Cart"] as Cart;
            if (cart == null || cart.SumQuantity() == 0)
                return RedirectToAction("Index", "Cart");

            ReceiverName = (ReceiverName ?? "").Trim();
            Phone = (Phone ?? "").Trim();
            Address = (Address ?? "").Trim();
            Note = (Note ?? "").Trim();

            string loi = null;
            if (ReceiverName.Length == 0 || Phone.Length == 0 || Address.Length == 0)
                loi = "Vui lòng nhập đầy đủ họ tên, số điện thoại và địa chỉ giao hàng";
            else if (!Regex.IsMatch(Phone, @"^\d{10,15}$"))
                loi = "Số điện thoại không hợp lệ!";
            else if (PaymentMethod != "cod" && PaymentMethod != "bank")
                loi = "Vui lòng chọn phương thức thanh toán";

            if (loi != null)
            {
                ViewBag.Loi = loi;
                return ShowPaymentPage(cart, ReceiverName, Phone, Address, Note, PaymentMethod);
            }

            var donHang = new Order(DanhSachDonHang.Count + 1, nd.Id, ReceiverName, Phone, Address, Note, PaymentMethod,
                                    new CartViewModel(cart, StoreController.GetSanPham()));
            DanhSachDonHang.Add(donHang);
            cart.Clear();

            // Chuyển hướng (không return View) để bấm F5 không gửi lại form và tạo thêm đơn
            TempData["ThongBao"] = "Đặt hàng thành công! Cảm ơn bạn đã mua sắm.";
            return RedirectToAction("OrderDetail", new { id = donHang.Id });
        }

        // Chi tiết một đơn hàng: GET /Orders/OrderDetail/1
        public ActionResult OrderDetail(int? id)
        {
            var nd = LayNguoiDungHienTai();
            if (nd == null)
                return ChuyenSangDangNhap("Vui lòng đăng nhập để xem đơn hàng.", Url.Action("OrderDetail", "Orders", new { id = id }));

            // Chỉ tìm trong đơn của chính người này: sửa số id trên thanh địa chỉ cũng không xem được đơn người khác
            var donHang = DanhSachDonHang.FirstOrDefault(o => o.Id == id && o.UserId == nd.Id);
            if (donHang == null) return HttpNotFound();

            return View(donHang);
        }
    }
}