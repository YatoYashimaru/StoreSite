using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StoreSite.Controllers
{
    // Kế thừa AdminBaseController để đảm bảo tài khoản phải có quyền Admin mới xem được
    public class AdminUsersController : AdminBaseController
    {
        private const int PageSize = 10; // Mỗi trang hiển thị 10 khách hàng

        public ActionResult Index(string search, int page = 1)
        {
            // 1. Chỉ lấy những tài khoản là Khách hàng
            var query = db.Users.Where(u => !u.UserRole);

            // 2. Nếu Admin gõ vào ô tìm kiếm, lọc tiếp theo Tên, Email hoặc Số điện thoại
            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.Trim();
                query = query.Where(u => u.UserName.Contains(s) ||
                                         u.UserEmail.Contains(s) ||
                                         (u.UserPhone != null && u.UserPhone.Contains(s)));
            }

            // 3. Tính toán số lượng trang
            int total = query.Count();
            int totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));

            // Ép biến page nằm trong giới hạn hợp lệ (không cho âm, không vượt quá tổng số trang)
            page = Math.Min(Math.Max(page, 1), totalPages);

            // 4. Lấy dữ liệu của trang hiện tại (sắp xếp ID giảm dần để khách mới tạo lên đầu)
            var customers = query.OrderByDescending(u => u.UserID)
                                 .Skip((page - 1) * PageSize)
                                 .Take(PageSize)
                                 .ToList();

            // 5. Đóng gói các thông số phụ gửi sang View
            ViewBag.Search = search;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Total = total;

            // Gửi danh sách customers sang giao diện
            return View(customers);
        }

        // POST: /AdminUsers/ToggleLock
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ToggleLock(int id)
        {
            var u = db.Users.Find(id);
            if (u == null || u.UserRole)
            {
                TempData["Loi"] = "Không tìm thấy tài khoản khách hàng.";
                return BackToList();
            }
            u.IsLocked = !u.IsLocked;
            db.SaveChanges();

            TempData["ThongBao"] = u.IsLocked
                ? "Đã khóa tài khoản khách hàng “" + u.UserName + "”."
                : "Đã mở khóa tài khoản khách hàng “" + u.UserName + "”.";
            return BackToList();
        }
        // POST: /AdminUsers/Delete
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var u = db.Users.Find(id);
            if (u == null || u.UserRole)
            {
                TempData["Loi"] = "Không tìm thấy tài khoản khách hàng cần xóa.";
                return BackToList();
            }
            // Ràng buộc khóa ngoại: Khách hàng đã từng đặt đơn thì không xóa được để giữ lịch sử doanh thu
            if (db.Orders.Any(o => o.UserID == id))
            {
                TempData["Loi"] = "Khách hàng “" + u.UserName + "” đã có đơn hàng nên không thể xóa. Bạn hãy dùng chức năng Khóa tài khoản.";
                return BackToList();
            }
            string ten = u.UserName;
            db.Users.Remove(u);
            db.SaveChanges();
            TempData["ThongBao"] = "Đã xóa tài khoản khách hàng “" + ten + "”.";
            return BackToList();
        }
        // Quay lại đúng trang danh sách hiện tại (giữ nguyên từ khóa tìm kiếm và số trang)
        private ActionResult BackToList()
        {
            if (Request.UrlReferrer != null && Url.IsLocalUrl(Request.UrlReferrer.PathAndQuery))
                return Redirect(Request.UrlReferrer.PathAndQuery);
            return RedirectToAction("Index");
        }
    }
}