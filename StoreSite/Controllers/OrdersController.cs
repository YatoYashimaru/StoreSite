using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using StoreSite.Models;

namespace StoreSite.Controllers
{
    public class OrdersController : BaseController
    {
        // PHẢI cùng khóa Session với CartController (tìm chữ Session[" trong CartController.cs)
        private Cart GetCart()
        {
            var cart = Session["Cart"] as Cart;
            if (cart == null) { cart = new Cart(); Session["Cart"] = cart; }
            return cart;
        }

        private CartViewModel BuildViewModel(Cart cart)
        {
            var ids = cart.TakeList().Select(i => i.ProductId).Distinct().ToList();
            var products = db.SanPhams.Where(p => ids.Contains(p.Id)).ToList();
            return new CartViewModel(cart, products);
        }

        // /Orders/OdersPage -> danh sách đơn nằm ở tab "Đơn hàng của tôi" trong trang cá nhân
        public ActionResult OdersPage()
        {
            return RedirectToAction("UserPage", "User", new { tab = "orders" });
        }

        // ---------------- Thanh toán ----------------
        [HttpGet]
        public ActionResult PaymentPage()
        {
            var user = CurrentUser();
            if (user == null)
            {
                TempData["ThongBao"] = "Vui lòng đăng nhập để thanh toán";
                return RedirectToAction("Login", "User");
            }

            var vm = BuildViewModel(GetCart());
            if (vm.TakeLines().Count == 0) return RedirectToAction("Index", "Cart");

            ViewBag.ReceiverName = user.UserName;      // điền sẵn từ tài khoản
            ViewBag.Phone = user.UserPhone;
            ViewBag.Address = user.UserAddress;
            ViewBag.PaymentMethod = "cod";
            return View(vm);
        }

        // ---------------- Đặt hàng: lưu vào Order + OrderDetail ----------------
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult PlaceOrder(string ReceiverName, string Phone, string Address, string Note, string PaymentMethod)
        {
            var user = CurrentUser();
            if (user == null) return RedirectToAction("Login", "User");

            var cart = GetCart();
            var vm = BuildViewModel(cart);
            var lines = vm.TakeLines();
            if (lines.Count == 0) return RedirectToAction("Index", "Cart");

            ReceiverName = (ReceiverName ?? "").Trim();
            Phone = (Phone ?? "").Trim();
            Address = (Address ?? "").Trim();
            Note = (Note ?? "").Trim();
            if (PaymentMethod != "bank") PaymentMethod = "cod";

            string loi = null;
            if (ReceiverName.Length == 0) loi = "Vui lòng nhập họ tên người nhận";
            else if (!Regex.IsMatch(Phone, @"^0\d{9}$")) loi = "Số điện thoại gồm 10 số, bắt đầu bằng 0";
            else if (Address.Length < 10) loi = "Vui lòng nhập địa chỉ giao hàng đầy đủ";

            if (loi != null)
            {
                ViewBag.Loi = loi;
                ViewBag.ReceiverName = ReceiverName; ViewBag.Phone = Phone;
                ViewBag.Address = Address; ViewBag.Note = Note; ViewBag.PaymentMethod = PaymentMethod;
                return View("PaymentPage", vm);
            }

            // Giá lấy từ database tại thời điểm đặt, không tin dữ liệu từ trình duyệt
            var order = new StoreSite.Models.Order
            {
                UserID = user.UserID,
                OrderDate = DateTime.Now,
                TotalAmount = vm.TotalPrice(),
                PaymentStatus = "Pending",
                AddressDelivery = Address,
                ReceiverName = ReceiverName,
                ReceiverPhone = Phone,
                Note = Note.Length == 0 ? null : Note,
                PaymentMethod = PaymentMethod
            };

            foreach (var line in lines)
            {
                order.OrderDetails.Add(new StoreSite.Models.OrderDetail
                {
                    ProductID = line.Product.Id,
                    SneakerSize = line.Size,
                    Quantity = line.Quantity,
                    UnitPrice = line.Product.GiaTien
                });
            }

            try
            {
                db.Orders.Add(order);
                db.SaveChanges();            // đơn + các dòng chi tiết lưu cùng lúc, lỗi thì không lưu gì cả
            }
            catch (DbEntityValidationException ex)
            {
                ViewBag.Loi = "Dữ liệu không hợp lệ: " + string.Join(" | ",
                    ex.EntityValidationErrors.SelectMany(e => e.ValidationErrors)
                      .Select(e => e.PropertyName + ": " + e.ErrorMessage));
            }
            catch (DbUpdateException ex)
            {
                ViewBag.Loi = "Không đặt được hàng: " + ex.GetBaseException().Message;
            }

            if (ViewBag.Loi != null)
            {
                ViewBag.ReceiverName = ReceiverName; ViewBag.Phone = Phone;
                ViewBag.Address = Address; ViewBag.Note = Note; ViewBag.PaymentMethod = PaymentMethod;
                return View("PaymentPage", vm);
            }

            cart.Clear();
            Session["Cart"] = cart;
            TempData["ThongBao"] = "Đặt hàng thành công! Cảm ơn bạn.";
            return RedirectToAction("OrderDetail", new { id = order.OrderID });
        }

        // ---------------- Chi tiết đơn (chỉ chủ đơn hoặc admin xem được) ----------------
        public ActionResult OrderDetail(int id)
        {
            var user = CurrentUser();
            if (user == null) return RedirectToAction("Login", "User");

            var order = db.Orders
                          .Include(o => o.OrderDetails.Select(d => d.SanPham))
                          .FirstOrDefault(o => o.OrderID == id && (o.UserID == user.UserID || user.UserRole));
            if (order == null) return HttpNotFound();

            return View(order);
        }
    }
}