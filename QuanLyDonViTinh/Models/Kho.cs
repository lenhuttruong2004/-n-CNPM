using System.ComponentModel.DataAnnotations;

namespace QuanLyDonViTinh.Models
{
    public class Kho
    {
        // === SỬA ===
        // Đổi "Ma_Kho" thành "Id" để khớp với Khóa Chính [Id] [int]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên kho không được để trống")]
        // === THÊM ===
        // Thêm StringLength(200) để khớp với DB [nvarchar(200)]
        [StringLength(200, ErrorMessage = "Tên kho tối đa 200 ký tự")]
        public string Ten_Kho { get; set; }

        // === THÊM ===
        // Thêm StringLength(500) để khớp với DB [nvarchar(500)]
        [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
        public string Ghi_Chu { get; set; }
    }
}