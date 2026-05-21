namespace QuanLyThuChiWeb.Models
{
    public class User
    {
        public int Id { get; set; }

        public required string HoTen { get; set; }

        public required string Email { get; set; }

        public required string MatKhau { get; set; }

        // Bỏ từ khóa required ở đây và gán giá trị mặc định bằng một danh sách trống
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}