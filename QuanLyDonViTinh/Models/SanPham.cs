using System.ComponentModel.DataAnnotations;

namespace QuanLyDonViTinh.Models
{
    public class SanPham
    {
        public int Ma_San_Pham { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        [StringLength(100, ErrorMessage = "Tên sản phẩm tối đa 100 ký tự")] // Thêm giới hạn độ dài
        public string Ten_San_Pham { get; set; }

        /* Dùng [Range] để bắt buộc chọn giá trị > 0 */
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn Loại sản phẩm")]
        public int Loai_San_Pham_ID { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn Đơn vị tính")]
        public int Don_Vi_Tinh_ID { get; set; }

        public string Ghi_Chu { get; set; }

        // Thuộc tính hiển thị (không lưu DB)
        public string Ten_Loai_San_Pham { get; set; }
        public string Ten_Don_Vi_Tinh { get; set; }
    }
}