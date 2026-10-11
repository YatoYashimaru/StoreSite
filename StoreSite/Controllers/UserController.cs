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

            if (MatKhau.Length < 6)
            {
                ViewBag.Loi = "Mật khẩu phải từ 6 ký tự trở lên";
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
        public ActionResult UserPage()
        {
            return View();
        }
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}