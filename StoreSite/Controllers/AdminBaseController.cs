using System.Net;
using System.Web.Mvc;

namespace StoreSite.Controllers
{
    /// <summary>
    /// Mọi controller của khu quản trị kế thừa class này.
    /// Chỉ tài khoản có Users.UserRole = 1 (admin) mới vào được; kiểm tra lại trong database mỗi lần,
    /// không tin vào Session, nên đổi quyền trong SQL có hiệu lực ngay.
    /// </summary>
    public abstract class AdminBaseController : BaseController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var id = Session["NguoiDungId"] as int?;
            if (id == null)
            {
                filterContext.Result = RedirectToAction("Login", "User");
                return;
            }

            var user = db.Users.Find(id.Value);
            if (user == null || !user.UserRole)
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
