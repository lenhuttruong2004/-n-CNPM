using System.ComponentModel.DataAnnotations;

namespace QuanLyDonViTinh.Models
{
    public class DonViTinh
    {
        // === SỬA ===
        // Đổi "Ma_Don_Vi_Tinh" thành "Id" để khớp với tên cột trong DB
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên đơn vị tính không được để trống")]
        // === SỬA ===
        // Đổi "50" thành "100" để khớp với "nvarchar(100)" trong DB
        [StringLength(100, ErrorMessage = "Tên đơn vị tính không được quá 100 ký tự")]
        public string Ten_Don_Vi_Tinh { get; set; }

        // === THÊM (Tùy chọn nhưng nên có) ===
        // Thêm StringLength để khớp với "nvarchar(500)" trong DB
        [StringLength(500, ErrorMessage = "Ghi chú không được quá 500 ký tự")]
        public string Ghi_Chu { get; set; }
    }
}