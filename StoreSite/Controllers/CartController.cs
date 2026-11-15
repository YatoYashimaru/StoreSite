using StoreSite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StoreSite.Controllers
{
    public class CartController : BaseController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (IsAdminUser())
            {
                string actionName = filterContext.ActionDescriptor.ActionName;
                // 1. Nếu là Badge đếm số lượng trên header thì trả về rỗng để ẩn đi
                if (actionName == "Badge")
                {
                    filterContext.Result = new EmptyResult();
                    return;
                }
                string msg = "Tài khoản quản trị chỉ được xem website, không thể mua hàng.";
                // 2. Nếu bấm nút Thêm/Sửa/Xóa giỏ hàng (POST / AJAX) thì trả về JSON đúng chuẩn { ok, message } của bạn
                if (filterContext.HttpContext.Request.HttpMethod == "POST" || filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = Json(new { ok = false, message = msg });
                    return;
                }
                // 3. Nếu cố tình gõ URL /Cart để vào trang giỏ hàng thì đẩy về trang chủ
                TempData["Loi"] = msg;
                filterContext.Result = RedirectToAction("Index", "Home");
                return;
            }
            base.OnActionExecuting(filterContext);
        }

        private Cart TakeCart()
        {
            var cart = Session["Cart"] as Cart;
            if (cart == null)
            {
                cart = new Cart();
                Session["Cart"] = cart;
            }
            return cart;
        }

        private static string Money(decimal amount) => amount.ToString("N0") + " đ"; // Định dạng tiền kiểu VND

        // Trang giỏ hàng: GET /Cart
        private CartViewModel BuildViewModel(Cart cart)
        {
            var ids = cart.TakeList().Select(i => i.ProductId).Distinct().ToList();
            var products = db.SanPhams.Where(p => ids.Contains(p.Id)).ToList();
            return new CartViewModel(cart, products);
        }

        // Hiển thị số đôi giày trong giỏ trên header. ChildActionOnly: chỉ gọi được từ trong view, không gõ URL vào được
        [ChildActionOnly]
        public ActionResult Badge()
        {
            return PartialView("_CartBadge", TakeCart().SumQuantity());
        }

        public ActionResult Index()
        {
            var cart = Session["Cart"] as Cart ?? new Cart();
            return View(BuildViewModel(cart)); 
        }

        [HttpPost]
        public ActionResult AddItem(int productId, int size, int quantity)
        {
            bool existItem = db.SanPhams.Any(p => p.Id == productId);
            if (!existItem) return Json(new { ok = false, message = "Sản phẩm không tồn tại." });
            var cart = TakeCart();
            if (!cart.AddItem(productId, size, quantity))
                return Json(new { ok = false, message = "Size không hợp lệ hoặc số lượng không hợp lệ." });
            return Json(new { ok = true, sumQuantity = cart.SumQuantity() });
        }

        [HttpPost]
        public ActionResult UpdateItem(int productId, int size, int quantity)
        {
            var sp = db.SanPhams.Find(productId);
            if (sp == null || sp.SoLuongTon <= 0)
                return Json(new { ok = false, message = "Sản phẩm này hiện đã tạm hết hàng." });
            var cart = TakeCart();
            int sizeKhacTrongGio = cart.TakeList()
                                       .Where(i => i.ProductId == productId && i.Size != size)
                                       .Sum(i => i.Quantity);

            if (sizeKhacTrongGio + quantity > sp.SoLuongTon)
                return Json(new {ok = false, message = "Kho chỉ còn tối đa " 
                    + sp.SoLuongTon + " đôi (các size khác trong giỏ đã có " + sizeKhacTrongGio + " đôi)." });
            if (!cart.UpdateQuantity(productId, size, quantity))
                return Json(new { ok = false, message = "Không cập nhật được sản phẩm này." });
            var vm = BuildViewModel(cart);
            var line = vm.FindLine(productId, size);
            return Json(new
            {
                ok = true,
                quantity = line.Quantity,
                lineTotal = Money(line.LineTotal()),
                totalPrice = Money(vm.TotalPrice()),
                sumQuantity = vm.SumQuantity()
            });
        }

        [HttpPost]
        public ActionResult RemoveItem(int productId, int size)
        {
            var cart = TakeCart();
            if (!cart.RemoveItem(productId, size))
                return Json(new { ok = false, message = "Sản phẩm không có trong giỏ hàng." });

            var vm = BuildViewModel(cart);
            return Json(new
            {
                ok = true,
                totalPrice = Money(vm.TotalPrice()),
                sumQuantity = vm.SumQuantity()
            });
        }
    }
}