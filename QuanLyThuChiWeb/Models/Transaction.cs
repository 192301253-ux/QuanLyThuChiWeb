using System.ComponentModel.DataAnnotations;

namespace QuanLyThuChiWeb.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int UserId { get; set; } // Khóa ngoại kết nối với User

        [Required(ErrorMessage = "Vui lòng nhập số tiền")]
        [Range(1, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 0")]
        public decimal SoTien { get; set; }

        [Required]
        public string? LoaiGiaoDich { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public string? DanhMuc { get; set; }

        [Required]
        public DateTime NgayGiaoDich { get; set; } = DateTime.Now;

        public string? MoTa { get; set; }
        public User? User { get; set; } // Navigation property
    }
}