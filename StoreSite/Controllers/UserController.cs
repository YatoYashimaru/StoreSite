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
    public class UserController : Controller
    {
        private static List<NguoiDung> DanhSachNguoiDung = new List<NguoiDung>();
        // GET: User
        
        public ActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SignUp(string HoTen, string Email, string MatKhau, string XacNhanMatKhau, string SDT)
        {
            if (string.IsNullOrEmpty(HoTen) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(MatKhau) ||
                string.IsNullOrWhiteSpace(MatKhau) || string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrEmpty(SDT) || string.IsNullOrWhiteSpace(SDT))
            {
                ViewBag.Loi = "Vui lòng nhập đầy đủ thông tin!";
                return View();
            }

            // Kiểm tra định dạng email cơ bản
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(Email, emailPattern))
            {
                ViewBag.Loi = "Định dạng email không hợp lệ!";
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

            // Cài đặt sdt từ 10 - 15 số, không có khoản trắng
            string sdt = @"^\d{10,15}$";
            if (!Regex.IsMatch(SDT, sdt))
            {
                ViewBag.Loi = "Số điện thoại không hợp lệ!";
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
            nguoiDungMoi.SDT = SDT;
            DanhSachNguoiDung.Add(nguoiDungMoi);

            TempData["ThongBao"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl; // trang cần quay lại sau khi đăng nhập (có thể rỗng)
            return View();
        }
        [HttpPost]
        public ActionResult Login(string Email, string MatKhau, string returnUrl)
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
                ViewBag.ReturnUrl = returnUrl; // giữ lại để lần đăng nhập sau vẫn quay về đúng trang
                return View();
            }

            Session["NguoiDungId"] = nguoiDungTimThay.Id;
            Session["HoTen"] = nguoiDungTimThay.HoTen;

            // Chỉ quay về trang thuộc website này(chặn đường link giả dẫn sang web khác)
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        // Cho controller khác (vd OrdersController) tìm người dùng theo Id
        [NonAction]
        public static NguoiDung TimTheoId(int id)
        {
            return DanhSachNguoiDung.FirstOrDefault(n => n.Id == id);
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
            else if (string.IsNullOrEmpty(MatKhauMoi) || MatKhauMoi.Length < 8)
                TempData["Loi"] = "Mật khẩu mới phải từ 8 ký tự trở lên";
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