using System.ComponentModel.DataAnnotations;

namespace QuanLyDonViTinh.Models
{
    public class NhaCungCap
    {
        // === SỬA ===
        // Đổi "Ma_NCC" (int) thành "Id" (int) để làm Khóa Chính
        public int Id { get; set; }

        // === THÊM MỚI ===
        // Bổ sung thuộc tính "Ma_NCC" (string) để khớp với Mã Nghiệp Vụ [nvarchar(50)]
        [Required(ErrorMessage = "Mã nhà cung cấp không được để trống")]
        [StringLength(50, ErrorMessage = "Mã nhà cung cấp tối đa 50 ký tự")]
        public string Ma_NCC { get; set; }

        [Required(ErrorMessage = "Tên nhà cung cấp không được để trống")]
        // === SỬA ===
        // Thêm StringLength(200) để khớp với DB [nvarchar(200)]
        [StringLength(200, ErrorMessage = "Tên nhà cung cấp tối đa 200 ký tự")]
        public string Ten_NCC { get; set; }

        // === SỬA ===
        // Thêm StringLength(500) để khớp với DB [nvarchar(500)]
        [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
        public string Ghi_Chu { get; set; }
    }
}