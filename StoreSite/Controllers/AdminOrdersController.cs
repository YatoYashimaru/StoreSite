using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using StoreSite.Models;

namespace StoreSite.Controllers
{
    /// <summary>Quản lý đơn hàng. Kế thừa AdminBaseController nên chỉ admin mới vào được.</summary>
    public class AdminOrdersController : AdminBaseController
    {
        private const int PageSize = 10;

        // /AdminOrders
        public ActionResult Index(string status, string search, int page = 1)
        {
            var q = from o in db.Orders
                    join u in db.Users on (int?)o.UserID equals (int?)u.UserID into gj
                    from u in gj.DefaultIfEmpty()
                    select new { o, u };

            if (status == "Pending" || status == "Complete")
                q = q.Where(x => x.o.PaymentStatus == status);
            else
                status = null;

            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.Trim();
                int id;
                if (int.TryParse(s.TrimStart('#'), out id))
                    q = q.Where(x => x.o.OrderID == id);
                else
                    q = q.Where(x => x.u.UserName.Contains(s) || x.u.UserEmail.Contains(s));
            }

            int total = q.Count();
            int totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
            page = Math.Min(Math.Max(page, 1), totalPages);

            var rows = q.OrderByDescending(x => x.o.OrderID)
                        .Skip((page - 1) * PageSize).Take(PageSize)
                        .Select(x => new AdminOrderRow
                        {
                            OrderID = x.o.OrderID,
                            OrderDate = x.o.OrderDate,
                            CustomerName = x.u.UserName,
                            CustomerEmail = x.u.UserEmail,
                            TotalAmount = x.o.TotalAmount,
                            PaymentStatus = x.o.PaymentStatus,
                            ItemCount = db.OrderDetails.Count(d => d.OrderID == x.o.OrderID)
                        }).ToList();

            var vm = new AdminOrderListViewModel
            {
                Items = rows,
                Page = page,
                TotalPages = totalPages,
                Total = total,
                Status = status,
                Search = search,
                CountAll = db.Orders.Count(),
                CountPending = db.Orders.Count(o => o.PaymentStatus == "Pending"),
                CountComplete = db.Orders.Count(o => o.PaymentStatus == "Complete"),
                Revenue = db.Orders.Where(o => o.PaymentStatus == "Complete")
                                   .Sum(o => (decimal?)o.TotalAmount) ?? 0m
            };

            ViewBag.Title = "Đơn hàng";
            return View(vm);
        }

        // /AdminOrders/Details/5
        public ActionResult Details(int id)
        {
            var o = db.Orders.Find(id);
            if (o == null) return HttpNotFound();

            var u = db.Users.FirstOrDefault(x => x.UserID == o.UserID);

            var raw = (from d in db.OrderDetails
                       where d.OrderID == id
                       join p in db.SanPhams on d.ProductID equals p.Id into gj
                       from p in gj.DefaultIfEmpty()
                       select new { d.ProductID, d.SneakerSize, d.Quantity, d.UnitPrice, p.ItemName, p.ImgURL })
                      .ToList();

            var vm = new AdminOrderDetailViewModel
            {
                OrderID = o.OrderID,
                OrderDate = o.OrderDate,
                PaymentStatus = o.PaymentStatus,
                TotalAmount = o.TotalAmount,
                PaymentMethod = o.PaymentMethod,
                Note = o.Note,
                ReceiverName = string.IsNullOrWhiteSpace(o.ReceiverName) ? (u == null ? null : u.UserName) : o.ReceiverName,
                ReceiverPhone = string.IsNullOrWhiteSpace(o.ReceiverPhone) ? (u == null ? null : u.UserPhone) : o.ReceiverPhone,
                AddressDelivery = o.AddressDelivery,
                AccountName = u == null ? null : u.UserName,
                AccountEmail = u == null ? null : u.UserEmail,
                Lines = raw.Select(r => new AdminOrderLine
                {
                    ProductID = Convert.ToInt32(r.ProductID),
                    ProductName = r.ItemName ?? "(sản phẩm đã bị xóa)",
                    ImgURL = r.ImgURL,
                    Size = Convert.ToString(r.SneakerSize),
                    Quantity = Convert.ToInt32(r.Quantity),
                    UnitPrice = Convert.ToDecimal(r.UnitPrice)
                }).ToList()
            };

            ViewBag.Title = "Đơn hàng #" + id;
            return View(vm);
        }

        // Đổi trạng thái thanh toán
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SetStatus(int id, string status)
        {
            if (status != "Pending" && status != "Complete")
            {
                TempData["Loi"] = "Trạng thái không hợp lệ.";
                return RedirectToAction("Details", new { id });
            }

            var o = db.Orders.Find(id);
            if (o == null) return HttpNotFound();

            o.PaymentStatus = status;
            db.SaveChanges();

            TempData["ThongBao"] = "Đã cập nhật đơn #" + id + " thành " + (status == "Complete" ? "Đã thanh toán." : "Chờ thanh toán.");
            return RedirectToAction("Details", new { id });
        }

        // Xóa đơn (xóa chi tiết trước rồi mới xóa đơn)
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var o = db.Orders.Find(id);
            if (o == null) return HttpNotFound();

            db.OrderDetails.RemoveRange(db.OrderDetails.Where(d => d.OrderID == id));
            db.Orders.Remove(o);
            db.SaveChanges();

            TempData["ThongBao"] = "Đã xóa đơn #" + id + ".";
            return RedirectToAction("Index");
        }
    }
}
