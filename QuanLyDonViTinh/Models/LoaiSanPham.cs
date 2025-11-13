using System.ComponentModel.DataAnnotations;

namespace QuanLyDonViTinh.Models
{
    public class LoaiSanPham
    {
        // === SỬA ===
        // Đổi "Ma_LSP" (int) thành "Id" (int) để khớp với Khóa Chính
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã loại sản phẩm không được để trống")]
        // === SỬA ===
        // Đổi "Ma_Loai_SP" (string) thành "Ma_LSP" (string)
        // Sửa StringLength từ 20 thành 50 để khớp DB [nvarchar](50)
        [StringLength(50, ErrorMessage = "Mã loại sản phẩm tối đa 50 ký tự")]
        public string Ma_LSP { get; set; }

        [Required(ErrorMessage = "Tên loại sản phẩm không được để trống")]
        // === SỬA ===
        // Sửa StringLength từ 50 thành 200 để khớp DB [nvarchar](200)
        [StringLength(200, ErrorMessage = "Tên loại sản phẩm tối đa 200 ký tự")]
        public string Ten_LSP { get; set; }

        // === THÊM (Nên có) ===
        // Thêm StringLength 500 để khớp DB [nvarchar](500)
        [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
        public string Ghi_Chu { get; set; }
    }
}