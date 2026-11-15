using System.Net;
using System.Web.Mvc;

namespace StoreSite.Controllers
{
    /// Mọi controller của khu quản trị kế thừa class này.
    /// Chỉ tài khoản có Users.UserRole = 1 (admin) mới vào được; kiểm tra lại trong database mỗi lần,
    /// không tin vào Session, nên đổi quyền trong SQL có hiệu lực ngay.
    public abstract class AdminBaseController : BaseController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            bool dangDangNhap = Session["NguoiDungId"] != null;
            var user = CurrentUser();
            if (user == null)
            {
                if (dangDangNhap)
                {
                    TempData["Loi"] = "Tài khoản quản trị của bạn đã bị khóa hoặc không còn tồn tại.";
                }
                filterContext.Result = RedirectToAction("Login", "User");
                return;
            }
            if (!user.UserRole)
            {
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden,
                    "Bạn không có quyền truy cập trang quản trị.");
                return;
            }
            ViewBag.AdminName = user.UserName;
            base.OnActionExecuting(filterContext);
        }
    }
}
