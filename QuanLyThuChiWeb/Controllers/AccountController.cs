using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuanLyThuChiWeb.Models;
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

        // 2. XỬ LÝ ĐĂNG NHẬP, LƯU SESSION VÀ QUYÊN ĐIỀU HƯỚNG VỀ MÀN HÌNH CHÍNH
        [HttpPost]
        public IActionResult LoginProcess(string email, string matKhau)
        {
            // Truy vấn kiểm tra tài khoản thực tế trong Database SQL Server
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.MatKhau == matKhau);

            if (user != null)
            {
                // LƯU THÔNG TIN ĐĂNG NHẬP VÀO HỆ THỐNG SESSION
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("UserFullName", user.HoTen);

                // ĐIỀU HƯỚNG: Quay trở thẳng về màn hình chính (Trang chủ Index của HomeController)
                return RedirectToAction("Index", "Home");
            }

            // Nếu sai thông tin, giữ lại trang Login và báo lỗi trực quan lên form
            ModelState.AddModelError("", "Tài khoản hoặc mật khẩu không chính xác!");
            return View("Login");
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