using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StoreSite.Models;

namespace StoreSite.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly StoreDbEntities db = new StoreDbEntities();

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
        protected StoreSite.Models.User CurrentUser()
        {
            var id = Session["NguoiDungId"] as int?;
            if (id == null) return null;
            var user = db.Users.Find(id.Value);
            // Tự động đăng xuất và xóa Session nếu tài khoản bị xóa hoặc bị khóa
            if (user == null || user.IsLocked)
            {
                Session.Clear();
                return null;
            }
            return user;
        }
        // Hàm kiểm tra nhanh tài khoản hiện tại có phải Admin hay không
        protected bool IsAdminUser()
        {
            var user = CurrentUser();
            return user != null && user.UserRole;
        }
    }
}