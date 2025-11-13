using System.ComponentModel.DataAnnotations;

namespace QuanLyDonViTinh.Models
{
    public class KhoUser
    {
        // === THÊM ===
        // Thêm khóa chính "Id" của bảng
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã đăng nhập không được để trống")]
        public string Ma_Dang_Nhap { get; set; }

        [Required(ErrorMessage = "Kho không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn Kho")] // Thêm Range
        public int Kho_ID { get; set; }

        // Thuộc tính để hiển thị Tên Kho (dùng JOIN)
        public string Ten_Kho { get; set; }
    }
}