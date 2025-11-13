using System.ComponentModel.DataAnnotations;

namespace QuanLyDonViTinh.Models
{
    public class NhapKhoRawData
    {
        public int Id { get; set; } // Sửa từ ID
        public int Nhap_Kho_ID { get; set; }

        [Required(ErrorMessage = "Sản phẩm không được để trống")]
        public int San_Pham_ID { get; set; }

        [Required(ErrorMessage = "Số lượng không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public decimal SL_Nhap { get; set; } // Sửa từ int

        [Required(ErrorMessage = "Đơn giá không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn 0")]
        public decimal Don_Gia_Nhap { get; set; }

        // Thuộc tính hiển thị (phải được SELECT trong Service)
        public string Ma_San_Pham { get; set; } // === THÊM ===
        public string Ten_San_Pham { get; set; }
        public string Ten_Don_Vi_Tinh { get; set; }
    }
}