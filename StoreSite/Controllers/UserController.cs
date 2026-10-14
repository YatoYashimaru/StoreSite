using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StoreSite.Models;

namespace StoreSite.Controllers
{
    public class UserController : Controller
    {
        private static List<NguoiDung> DanhSachNguoiDung = new List<NguoiDung>();
        // GET: User
        
        public ActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SignUp(string HoTen, string Email, string MatKhau, string XacNhanMatKhau)
        {
            if (string.IsNullOrEmpty(HoTen) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(MatKhau))
            {
                ViewBag.Loi = "Vui lòng nhập đầy đủ thông tin";
                return View();
            }

            if (MatKhau.Length < 8)
            {
                ViewBag.Loi = "Mật khẩu phải từ 8 ký tự trở lên";
                return View();
            }

            if (MatKhau != XacNhanMatKhau)
            {
                ViewBag.Loi = "Mật khẩu xác nhận không khớp";
                return View();
            }

            bool daTonTai = false;
            foreach (NguoiDung nd in DanhSachNguoiDung)
            {
                if (nd.Email.ToLower() == Email.ToLower())
                {
                    daTonTai = true;
                    break;
                }
            }
            if (daTonTai)
            {
                ViewBag.Loi = "Email này đã được đăng ký";
                return View();
            }

            NguoiDung nguoiDungMoi = new NguoiDung();
            nguoiDungMoi.Id = DanhSachNguoiDung.Count + 1;
            nguoiDungMoi.HoTen = HoTen;
            nguoiDungMoi.Email = Email;
            nguoiDungMoi.MatKhau = MatKhau;
            DanhSachNguoiDung.Add(nguoiDungMoi);

            TempData["ThongBao"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(string Email, string MatKhau)
        {
            NguoiDung nguoiDungTimThay = null;

            foreach (NguoiDung nd in DanhSachNguoiDung)
            {
                if (nd.Email.ToLower() == Email.ToLower() && nd.MatKhau == MatKhau)
                {
                    nguoiDungTimThay = nd;
                    break;
                }
            }

            if (nguoiDungTimThay == null)
            {
                ViewBag.Loi = "Email hoặc mật khẩu không đúng";
                return View();
            }

            Session["NguoiDungId"] = nguoiDungTimThay.Id;
            Session["HoTen"] = nguoiDungTimThay.HoTen;

            return RedirectToAction("Index", "Home");
        }
        // Lấy người dùng đang đăng nhập từ Session, chưa đăng nhập thì trả về null
        private NguoiDung LayNguoiDungHienTai()
        {
            var id = Session["NguoiDungId"] as int?;
            if (id == null) return null;
            return DanhSachNguoiDung.FirstOrDefault(n => n.Id == id.Value);
        }

        public ActionResult UserPage(string tab)
        {
            var nd = LayNguoiDungHienTai();
            if (nd == null) return RedirectToAction("Login");

            ViewBag.Tab = (tab == "password" || tab == "orders") ? tab : "profile";

            return View(new UserPageViewModel { HoTen = nd.HoTen, Email = nd.Email });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(string HoTen)
        {
            var nd = LayNguoiDungHienTai();
            if (nd == null) return RedirectToAction("Login");

            HoTen = (HoTen ?? "").Trim();
            if (HoTen.Length == 0)
            {
                TempData["Loi"] = "Họ và tên không được để trống";
                return RedirectToAction("UserPage", new { tab = "profile" });
            }

            nd.HoTen = HoTen;
            Session["HoTen"] = HoTen;          // để lời chào trên thanh menu đổi theo
            TempData["ThongBao"] = "Đã cập nhật thông tin";
            return RedirectToAction("UserPage", new { tab = "profile" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(string MatKhauCu, string MatKhauMoi, string XacNhanMatKhauMoi)
        {
            var nd = LayNguoiDungHienTai();
            if (nd == null) return RedirectToAction("Login");

            if (nd.MatKhau != MatKhauCu)
                TempData["Loi"] = "Mật khẩu hiện tại không đúng";
            else if (string.IsNullOrEmpty(MatKhauMoi) || MatKhauMoi.Length < 6)
                TempData["Loi"] = "Mật khẩu mới phải từ 6 ký tự trở lên";
            else if (MatKhauMoi != XacNhanMatKhauMoi)
                TempData["Loi"] = "Mật khẩu xác nhận không khớp";
            else
            {
                nd.MatKhau = MatKhauMoi;
                TempData["ThongBao"] = "Đã đổi mật khẩu";
            }

            return RedirectToAction("UserPage", new { tab = "password" });
        }
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}