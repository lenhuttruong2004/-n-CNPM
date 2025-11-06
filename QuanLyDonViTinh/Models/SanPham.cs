using System.ComponentModel.DataAnnotations;

namespace QuanLyDonViTinh.Models
{
    public class SanPham
    {
        public int Ma_San_Pham { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        public string Ten_San_Pham { get; set; }

        /* Khóa ngoại (bắt buộc nhập) */
        [Required(ErrorMessage = "Loại sản phẩm không được để trống")]
        public int Loai_San_Pham_ID { get; set; }

        /* Khóa ngoại (bắt buộc nhập) */
        [Required(ErrorMessage = "Đơn vị tính không được để trống")]
        public int Don_Vi_Tinh_ID { get; set; }

        public string Ghi_Chu { get; set; }

        // ----------- Thuộc tính hiển thị (không có trong DB) -----------
        // Chúng ta cần 2 thuộc tính này để hiển thị Tên thay vì ID số
        public string Ten_Loai_San_Pham { get; set; }
        public string Ten_Don_Vi_Tinh { get; set; }
    }
}