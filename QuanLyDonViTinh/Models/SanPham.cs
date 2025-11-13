using System.ComponentModel.DataAnnotations;

namespace QuanLyDonViTinh.Models
{
    public class SanPham
    {
        // === SỬA ===
        // Đổi "Ma_San_Pham" (int) thành "Id" (int) để khớp Khóa Chính DB
        public int Id { get; set; }

        // === SỬA ===
        // Đổi "Ma_SP" (string) thành "Ma_San_Pham" (string) để khớp cột Mã nghiệp vụ DB
        [Required(ErrorMessage = "Mã sản phẩm không được để trống")]
        [StringLength(50, ErrorMessage = "Mã sản phẩm tối đa 50 ký tự")]
        public string Ma_San_Pham { get; set; } // Đổi từ Ma_SP

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        // === SỬA ===
        // Đổi StringLength 100 -> 200 để khớp DB [nvarchar(200)]
        [StringLength(200, ErrorMessage = "Tên sản phẩm tối đa 200 ký tự")]
        public string Ten_San_Pham { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn Loại sản phẩm")]
        public int Loai_San_Pham_ID { get; set; } // Khớp DB

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn Đơn vị tính")]
        public int Don_Vi_Tinh_ID { get; set; } // Khớp DB

        // === THÊM (Nên có) ===
        // Thêm StringLength 500 để khớp DB [nvarchar(500)]
        [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
        public string Ghi_Chu { get; set; }

        // Thuộc tính hiển thị (không lưu DB) - Tên này vẫn OK
        public string Ten_Loai_San_Pham { get; set; }
        public string Ten_Don_Vi_Tinh { get; set; }
    }
}