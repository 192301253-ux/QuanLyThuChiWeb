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
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int thangLoc = thang ?? DateTime.Now.Month;
            int namLoc = nam ?? DateTime.Now.Year;

            // 1. Tính tổng Thu, Tổng Chi, Số dư của tháng được chọn
            var totalThu = _context.Transactions
                .Where(t => t.UserId == userId.Value && t.LoaiGiaoDich == "Thu" && t.NgayGiaoDich.Month == thangLoc && t.NgayGiaoDich.Year == namLoc)
                .Sum(t => (double?)t.SoTien) ?? 0;

            var totalChi = _context.Transactions
                .Where(t => t.UserId == userId.Value && t.LoaiGiaoDich == "Chi" && t.NgayGiaoDich.Month == thangLoc && t.NgayGiaoDich.Year == namLoc)
                .Sum(t => (double?)t.SoTien) ?? 0;

            var chiTieuTheoDanhMuc = _context.Transactions
                .Where(t => t.UserId == userId.Value && t.LoaiGiaoDich == "Chi" && t.NgayGiaoDich.Month == thangLoc && t.NgayGiaoDich.Year == namLoc)
                .GroupBy(t => t.DanhMuc)
                .Select(g => new {
                    TenDanhMuc = g.Key,
                    TongTien = g.Sum(t => (double)t.SoTien)
                }).ToList();

            // 3. Gửi toàn bộ dữ liệu qua ViewBag sang View
            ViewBag.TotalThu = totalThu;
            ViewBag.TotalChi = totalChi;
            ViewBag.SoDu = totalThu - totalChi;
            ViewBag.ThangHienTai = thangLoc;
            ViewBag.NamHienTai = namLoc;

            // Chuyển mảng danh mục thành danh sách dạng chuỗi để JavaScript dễ đọc
            ViewBag.LabelsDanhMuc = chiTieuTheoDanhMuc.Select(x => x.TenDanhMuc).ToArray();
            ViewBag.DataDanhMuc = chiTieuTheoDanhMuc.Select(x => x.TongTien).ToArray();

            return View();
        }
    }
}