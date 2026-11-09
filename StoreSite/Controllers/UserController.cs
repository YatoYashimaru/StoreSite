using StoreSite.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace StoreSite.Controllers
{
    public class UserController : BaseController
    {
        private StoreSite.Models.User LayNguoiDungHienTai()
        {
            var id = Session["NguoiDungId"] as int?;
            return id == null ? null : db.Users.Find(id.Value);
        }

        [HttpGet] public ActionResult Login() { return View(); }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            string email = model.Email.Trim();
            var user = db.Users.FirstOrDefault(u => u.UserEmail == email);

            if (user == null || user.PasswordHash != model.MatKhau)
            {
                ViewBag.Loi = "Email hoặc mật khẩu không đúng";
                return View(model);
            }

            Session["NguoiDungId"] = user.UserID;
            Session["HoTen"] = user.UserName;
            Session["NguoiDungId"] = user.UserID;
            Session["HoTen"] = user.UserName;
            Session["IsAdmin"] = user.UserRole;
            return RedirectToAction("Index", "Home");
        }

        [HttpGet] public ActionResult SignUp() { return View(); }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SignUp(SignUpViewModel model)
        {
            string email = (model.Email ?? "").Trim();
            if (db.Users.Any(u => u.UserEmail == email))
                ModelState.AddModelError("Email", "Email này đã được đăng ký");

            if (!ModelState.IsValid) return View(model);

            db.Users.Add(new StoreSite.Models.User
            {
                UserName = model.HoTen.Trim(),
                UserEmail = email,
                UserPhone = model.SoDienThoai.Trim(),
                PasswordHash = model.MatKhau,
                UserRole = StoreSite.Models.AdminEmails.IsAdmin(email)
            });
            db.SaveChanges();

            TempData["ThongBao"] = "Đăng ký thành công, hãy đăng nhập";
            return RedirectToAction("Login");
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult UserPage(string tab)
        {
            var user = LayNguoiDungHienTai();
            if (user == null) return RedirectToAction("Login");

            ViewBag.Tab = (tab == "password" || tab == "orders") ? tab : "profile";
            return View(new UserPageViewModel
            {
                HoTen = user.UserName,
                Email = user.UserEmail,
                Orders = db.Orders.Where(o => o.UserID == user.UserID)
                                  .OrderByDescending(o => o.OrderDate).ToList()
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(string HoTen)
        {
            var user = LayNguoiDungHienTai();
            if (user == null) return RedirectToAction("Login");

            HoTen = (HoTen ?? "").Trim();
            if (HoTen.Length == 0) TempData["Loi"] = "Họ và tên không được để trống";
            else
            {
                user.UserName = HoTen;
                db.SaveChanges();
                Session["HoTen"] = HoTen;
                TempData["ThongBao"] = "Đã cập nhật thông tin";
            }
            return RedirectToAction("UserPage", new { tab = "profile" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ChangePassword(string MatKhauCu, string MatKhauMoi, string XacNhanMatKhauMoi)
        {
            var user = LayNguoiDungHienTai();
            if (user == null) return RedirectToAction("Login");

            if (user.PasswordHash != MatKhauCu) TempData["Loi"] = "Mật khẩu hiện tại không đúng";
            else if (string.IsNullOrEmpty(MatKhauMoi) || MatKhauMoi.Length < 8) TempData["Loi"] = "Mật khẩu mới phải từ 8 ký tự trở lên";
            else if (MatKhauMoi != XacNhanMatKhauMoi) TempData["Loi"] = "Mật khẩu xác nhận không khớp";
            else
            {
                user.PasswordHash = MatKhauMoi;
                db.SaveChanges();
                TempData["ThongBao"] = "Đã đổi mật khẩu";
            }
            return RedirectToAction("UserPage", new { tab = "password" });
        }
    }
}