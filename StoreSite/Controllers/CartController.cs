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
            //bool existItem = .Any(p => p.Id == productId);    // đang sửa
            //if (!existItem) return Json(new { ok = false, message = "Sản phẩm không tồn tại." });
            var cart = TakeCart();
            if (!cart.AddItem(productId, size, quantity))
                return Json(new { ok = false, message = "Size không hợp lệ hoặc số lượng không hợp lệ." });
            return Json(new { ok = true, sumQuantity = cart.SumQuantity() });
        }

        [HttpPost]
        public ActionResult UpdateItem(int productId, int size, int quantity)
        {
            var cart = TakeCart();
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