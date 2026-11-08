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
            return id == null ? null : db.Users.Find(id.Value);
        }
    }
}