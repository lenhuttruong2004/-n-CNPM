using System.ComponentModel.DataAnnotations;

namespace QuanLyDonViTinh.Models // Tên Namespace phải là QuanLyDonViTinh.Models
{
    public class LoaiSanPham
    {
        public int Ma_LSP { get; set; }

        [Required(ErrorMessage = "Tên loại sản phẩm không được để trống")]
        public string Ten_LSP { get; set; }

        public string Ghi_Chu { get; set; }
    }
}
