using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace StoreSite.Controllers
{
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