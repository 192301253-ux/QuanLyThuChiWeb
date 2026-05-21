using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuChiWeb.Models;
using System;
using System.Linq;

namespace QuanLyThuChiWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        // Tiêm (Inject) DbContext vào Controller
        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        // Màn hình 1: Lấy dữ liệu thực tế từ SQL Server lên giao diện (Trang chủ)
        public IActionResult Index()
        {
            // LẤY ID NGƯỜI DÙNG THỰC TẾ TỪ SESSION ĐĂNG NHẬP
            int? userId = HttpContext.Session.GetInt32("UserId");

            // Nếu chưa đăng nhập, đá người dùng về trang đăng nhập để bảo mật dữ liệu
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy danh sách giao dịch thực tế của chính User này
            var danhSachGD = _context.Transactions
                                     .Where(t => t.UserId == userId.Value)
                                     .OrderByDescending(t => t.NgayGiaoDich)
                                     .ToList();

            // Tính toán số liệu thống kê nhanh từ Database của chính User này
            ViewBag.TongThu = _context.Transactions.Where(t => t.UserId == userId.Value && t.LoaiGiaoDich == "Thu").Sum(t => (double)t.SoTien);
            ViewBag.TongChi = _context.Transactions.Where(t => t.UserId == userId.Value && t.LoaiGiaoDich == "Chi").Sum(t => (double)t.SoTien);
            ViewBag.SoDu = ViewBag.TongThu - ViewBag.TongChi;

            return View(danhSachGD);
        }

        // Xử lý Lưu giao dịch thực sự vào Database (Bước 9 luồng chính)
        [HttpPost]
        public IActionResult ThemGiaoDich(Transaction model)
        {
            // LẤY ID NGƯỜI DÙNG THỰC TẾ TỪ SESSION ĐĂNG NHẬP
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            model.UserId = userId.Value; // Gán ID thực tế của người đang đăng nhập vào bản ghi giao dịch

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Transactions.Add(model); // Thêm đối tượng vào ngữ cảnh
                    _context.SaveChanges(); // Lệnh thực thi ghi dữ liệu xuống SQL Server

                    TempData["SuccessMessage"] = "Giao dịch được lưu vào Database thành công!"; // Bước 10
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Mất kết nối database, lưu thất bại!"); // Xử lý lỗi E08 trong file thiết kế
                }
            }

            // Nếu lỗi, tải lại trang kèm thông điệp báo lỗi của chính User đó
            var danhSachGD = _context.Transactions.Where(t => t.UserId == userId.Value).ToList();
            return View("Index", danhSachGD);
        }

        // ==========================================
        // CHỨC NĂNG MỚI: XỬ LÝ TRANG BÁO CÁO TÀI CHÍNH LỌC THEO THÁNG
        // ==========================================
        public IActionResult BaoCao(int? thang, int? nam)
        {
            // 1. Kiểm tra trạng thái đăng nhập từ Session
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // 2. Nếu người dùng chưa bấm chọn bộ lọc, mặc định hệ thống sẽ lấy Tháng và Năm hiện tại máy tính
            int thangLoc = thang ?? DateTime.Now.Month;
            int namLoc = nam ?? DateTime.Now.Year;

            // 3. Tính toán tổng tiền Thu và Chi trong SQL Server được LỌC CHÍNH XÁC THEO THÁNG/NĂM của User này
            var totalThu = _context.Transactions
                .Where(t => t.UserId == userId.Value
                         && t.LoaiGiaoDich == "Thu"
                         && t.NgayGiaoDich.Month == thangLoc
                         && t.NgayGiaoDich.Year == namLoc)
                .Sum(t => (double)t.SoTien);

            var totalChi = _context.Transactions
                .Where(t => t.UserId == userId.Value
                         && t.LoaiGiaoDich == "Chi"
                         && t.NgayGiaoDich.Month == thangLoc
                         && t.NgayGiaoDich.Year == namLoc)
                .Sum(t => (double)t.SoTien);

            // 4. Đẩy số liệu đã lọc và các điều kiện thời gian ngược lại sang View
            ViewBag.TotalThu = totalThu;
            ViewBag.TotalChi = totalChi;
            ViewBag.ThangHienTai = thangLoc;
            ViewBag.NamHienTai = namLoc;

            return View();
        }
    }
}