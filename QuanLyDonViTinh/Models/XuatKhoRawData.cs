using System.ComponentModel.DataAnnotations;

namespace QuanLyDonViTinh.Models
{
    public class XuatKhoRawData
    {
        // === SỬA === (Đổi "ID" thành "Id")
        public int Id { get; set; }
        public int Xuat_Kho_ID { get; set; }

        [Required(ErrorMessage = "Sản phẩm không được để trống")]
        public int San_Pham_ID { get; set; }

        // === SỬA === (Đổi "int" thành "decimal")
        [Required(ErrorMessage = "Số lượng không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public decimal SL_Xuat { get; set; }

        [Required(ErrorMessage = "Đơn giá không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn 0")]
        public decimal Don_Gia_Xuat { get; set; }

        // === THÊM CÁC THUỘC TÍNH HIỂN THỊ ===
        public string Ma_San_Pham { get; set; }
        public string Ten_San_Pham { get; set; }
        public string Ten_Don_Vi_Tinh { get; set; }
    }
}