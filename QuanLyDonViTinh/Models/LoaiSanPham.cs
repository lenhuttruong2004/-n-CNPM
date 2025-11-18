using System.ComponentModel.DataAnnotations;

namespace QuanLyDonViTinh.Models
{
    public class LoaiSanPham
    {
        // Khóa chính
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã loại sản phẩm không được để trống")]
        [StringLength(50, ErrorMessage = "Mã loại sản phẩm tối đa 50 ký tự")]
        public string Ma_LSP { get; set; }

        [Required(ErrorMessage = "Tên loại sản phẩm không được để trống")]
        [StringLength(200, ErrorMessage = "Tên loại sản phẩm tối đa 200 ký tự")]
        public string Ten_LSP { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
        public string Ghi_Chu { get; set; }
    }
}
