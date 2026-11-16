using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using StoreSite.Models;

namespace StoreSite.Controllers
{
    /// <summary>
    /// Đánh giá sản phẩm. Mỗi tài khoản (trừ admin) đánh giá mỗi sản phẩm một lần và được sửa / xóa đánh giá của mình.
    /// Điểm SanPhams.rating là trung bình các đánh giá, tự tính lại mỗi khi có thay đổi
    /// (admin không còn tự nhập điểm này nữa).
    /// </summary>
    public class ReviewsController : BaseController
    {
        private const int MaxComment = 1000;

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Submit(int productId, int stars, string comment)
        {
            var user = CurrentUser();
            if (user == null) return RedirectToAction("Login", "User");
            if (user.UserRole)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Quản trị viên không thể đánh giá sản phẩm.");

            var sp = db.SanPhams.Find(productId);
            if (sp == null) return HttpNotFound();

            comment = (comment ?? "").Trim();
            if (stars < 1 || stars > 5)
            {
                TempData["ReviewError"] = "Vui lòng chọn số sao từ 1 đến 5.";
                return Back(productId);
            }
            if (comment.Length > MaxComment)
            {
                TempData["ReviewError"] = "Nhận xét tối đa " + MaxComment + " ký tự.";
                return Back(productId);
            }

            var r = db.Reviews.FirstOrDefault(x => x.ProductID == productId && x.UserID == user.UserID);
            bool moi = r == null;
            if (moi)
            {
                r = new Review { ProductID = productId, UserID = user.UserID };
                db.Reviews.Add(r);
            }
            r.Stars = stars;
            r.Comment = comment.Length == 0 ? null : comment;
            r.CreatedAt = DateTime.Now;
            db.SaveChanges();

            UpdateRating(sp);
            TempData["ReviewOk"] = moi ? "Cảm ơn bạn đã đánh giá sản phẩm!" : "Đã cập nhật đánh giá của bạn.";
            return Back(productId);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Delete(int productId)
        {
            var user = CurrentUser();
            if (user == null) return RedirectToAction("Login", "User");

            var sp = db.SanPhams.Find(productId);
            if (sp == null) return HttpNotFound();

            var r = db.Reviews.FirstOrDefault(x => x.ProductID == productId && x.UserID == user.UserID);
            if (r != null)
            {
                db.Reviews.Remove(r);
                db.SaveChanges();
                UpdateRating(sp);
                TempData["ReviewOk"] = "Đã xóa đánh giá của bạn.";
            }
            return Back(productId);
        }

        // rating = trung bình các sao (làm tròn 1 chữ số); chưa có đánh giá nào thì 0
        private void UpdateRating(SanPham sp)
        {
            var stars = db.Reviews.Where(x => x.ProductID == sp.Id).Select(x => x.Stars).ToList();
            sp.rating = stars.Count == 0 ? 0 : Math.Round(stars.Average(), 1);
            db.SaveChanges();
        }

        private ActionResult Back(int productId)
        {
            return Redirect(Url.Action("Details", "Store", new { id = productId }) + "#reviews");
        }
    }
}
