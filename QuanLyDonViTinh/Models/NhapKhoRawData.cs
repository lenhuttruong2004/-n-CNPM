using System.ComponentModel.DataAnnotations;

namespace QuanLyDonViTinh.Models
{
    public class NhapKhoRawData
    {
        // === SỬA ===
        // Đổi "ID" (viết hoa) thành "Id" (PascalCase) để khớp với cột [Id] trong DB
        public int Id { get; set; }

        public int Nhap_Kho_ID { get; set; } // Liên kết đến Header

        [Required(ErrorMessage = "Sản phẩm không được để trống")]
        public int San_Pham_ID { get; set; }

        // === SỬA ===
        // Đổi kiểu "int" thành "decimal" để khớp với [SL_Nhap] [decimal](18, 2)
        [Required(ErrorMessage = "Số lượng không được để trống")]
        // Sửa Range: Đổi từ int range (1) sang decimal range (0.01)
        [Range(0.01, double.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public decimal SL_Nhap { get; set; }

        [Required(ErrorMessage = "Đơn giá không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn 0")]
        public decimal Don_Gia_Nhap { get; set; }

        // Thuộc tính hiển thị (không có trong DB)
        public string Ten_San_Pham { get; set; }
        public string Ten_Don_Vi_Tinh { get; set; }
    }
}