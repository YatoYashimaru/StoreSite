using System.Linq;
using System.Web.Mvc;

namespace StoreSite.Controllers
{
    /// <summary>Nhân viên = tài khoản có Users.UserRole = 1. Cấp / thu hồi quyền admin tại đây.</summary>
    public class AdminStaffController : AdminBaseController
    {
        public ActionResult Index()
        {
            ViewBag.Me = (int)(Session["NguoiDungId"] as int?).GetValueOrDefault();
            ViewBag.Candidates = db.Users.Where(u => !u.UserRole)
                                         .OrderBy(u => u.UserEmail)
                                         .Select(u => u.UserEmail).Take(200).ToList();
            ViewBag.Title = "Nhân viên";
            return View(db.Users.Where(u => u.UserRole).OrderBy(u => u.UserID).ToList());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Grant(string email)
        {
            email = (email ?? "").Trim();
            var u = db.Users.FirstOrDefault(x => x.UserEmail == email);
            if (u == null)
            {
                TempData["Loi"] = "Không có tài khoản nào dùng email “" + email + "”. Người đó cần đăng ký trước.";
                return RedirectToAction("Index");
            }
            if (u.UserRole)
            {
                TempData["Loi"] = u.UserName + " đã là nhân viên rồi.";
                return RedirectToAction("Index");
            }

            u.UserRole = true;
            db.SaveChanges();
            TempData["ThongBao"] = "Đã cấp quyền admin cho " + u.UserName + ".";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Revoke(int id)
        {
            int me = (Session["NguoiDungId"] as int?).GetValueOrDefault();
            if (id == me)
            {
                TempData["Loi"] = "Bạn không thể tự thu hồi quyền của chính mình.";
                return RedirectToAction("Index");
            }

            var u = db.Users.Find(id);
            if (u == null) return HttpNotFound();

            if (db.Users.Count(x => x.UserRole) <= 1)
            {
                TempData["Loi"] = "Phải còn ít nhất một admin.";
                return RedirectToAction("Index");
            }

            u.UserRole = false;
            db.SaveChanges();
            TempData["ThongBao"] = "Đã thu hồi quyền admin của " + u.UserName + ".";
            return RedirectToAction("Index");
        }
    }
}
