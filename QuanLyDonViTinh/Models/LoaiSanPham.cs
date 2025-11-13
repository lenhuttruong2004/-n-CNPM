using System.ComponentModel.DataAnnotations;

namespace QuanLyDonViTinh.Models
{
    public class LoaiSanPham
    {
        public int Ma_LSP { get; set; }
        [Required(ErrorMessage = "Mã loại sản phẩm không được để trống")]
        [StringLength(20, ErrorMessage = "Mã loại sản phẩm tối đa 20 ký tự")]
        public string Ma_Loai_SP { get; set; }

        [Required(ErrorMessage = "Tên loại sản phẩm không được để trống")]
        // Dòng này quyết định thông báo lỗi màu đỏ bạn thấy trong ảnh
        [StringLength(50, ErrorMessage = "Tên loại sản phẩm tối đa 50 ký tự")]

        public string Ten_LSP { get; set; }

        public string Ghi_Chu { get; set; }
    }
}