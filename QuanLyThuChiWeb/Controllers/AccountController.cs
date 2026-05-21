using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuanLyThuChiWeb.Models;
using System;
using System.Linq;

namespace QuanLyThuChiWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        // Tiêm DbContext vào để làm việc trực tiếp với SQL Server
        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // Hiển thị giao diện trang Đăng ký (Register.cshtml)
        public IActionResult Register()
        {
            return View();
        }

        // Hiển thị giao diện trang Đăng nhập (Login.cshtml)
        public IActionResult Login()
        {
            return View();
        }

        // 1. XỬ LÝ ĐĂNG KÝ TÀI KHOẢN VÀ LƯU XUỐNG DATABASE
        [HttpPost]
        public IActionResult RegisterProcess(string hoTen, string email, string matKhau, string nhapLaiMatKhau)
        {
            // Kiểm tra các trường dữ liệu có bị bỏ trống không
            if (string.IsNullOrEmpty(hoTen) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(matKhau))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin đăng ký!");
                return View("Register");
            }

            // Kiểm tra mật khẩu nhập lại có trùng khớp không
            if (matKhau != nhapLaiMatKhau)
            {
                ModelState.AddModelError("", "Mật khẩu xác nhận không trùng khớp!");
                return View("Register");
            }

            // Kiểm tra xem Email này đã tồn tại trong hệ thống chưa
            if (_context.Users.Any(u => u.Email == email))
            {
                ModelState.AddModelError("", "Email này đã được sử dụng!");
                return View("Register");
            }

            // Tạo đối tượng User mới và lưu vĩnh viễn vào SQL Server
            var newUser = new User { HoTen = hoTen, Email = email, MatKhau = matKhau };
            _context.Users.Add(newUser);
            _context.SaveChanges();

            // Đặt cờ hiệu báo đăng ký thành công để View Login.cshtml bật bảng thông báo lên
            TempData["RegisterSuccess"] = true;
            return RedirectToAction("Login");
        }

        // 2. XỬ LÝ ĐĂNG NHẬP, LƯU SESSION VÀ PHÂN QUYỀN ĐIỀU HƯỚNG VỀ MÀN HÌNH CHÍNH
        [HttpPost]
        public IActionResult LoginProcess(string email, string matKhau)
        {
            // Bước 1: Kiểm tra người dùng có bỏ trống ô nhập liệu nào không
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(matKhau))
            {
                ViewBag.ErrorMessage = "Vui lòng nhập đầy đủ cả Email và Mật khẩu!";
                return View("Login");
            }

            // Bước 2: Tìm kiếm tài khoản dựa trên Email người dùng nhập vào
            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                // Trường hợp 1: Không tìm thấy Email này trong hệ thống cơ sở dữ liệu
                ViewBag.ErrorMessage = "Tài khoản Email này không tồn tại trên hệ thống!";
                return View("Login");
            }

            // Bước 3: Tìm thấy Email rồi, tiếp tục so sánh xem Mật khẩu đúng không
            if (user.MatKhau != matKhau)
            {
                // Trường hợp 2: Đúng Email nhưng gõ sai Mật khẩu
                ViewBag.ErrorMessage = "Mật khẩu không chính xác. Vui lòng thử lại!";
                return View("Login");
            }

            // Bước 4: Đăng nhập thành công -> Tiến hành cấp quyền và lưu trữ trạng thái Session
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserFullName", user.HoTen);

            // ĐIỀU HƯỚNG: Chuyển hướng thẳng về màn hình chính (Trang chủ của dự án)
            return RedirectToAction("Index", "Home");
        }

        // 3. XỬ LÝ ĐĂNG XUẤT (RESET TRẠNG THÁI APP)
        public IActionResult Logout()
        {
            // Xóa sạch toàn bộ Session đăng nhập hiện tại
            HttpContext.Session.Clear();

            // Đưa người dùng về màn hình chính (Lúc này cụm nút Đăng ký / Đăng nhập sẽ tự động tái xuất hiện)
            return RedirectToAction("Index", "Home");
        }
    }
}