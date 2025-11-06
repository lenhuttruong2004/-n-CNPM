using System.ComponentModel.DataAnnotations;

namespace QuanLyDonViTinh.Models
{
    public class DonViTinh
    {
        public int Ma_Don_Vi_Tinh { get; set; }

        [Required(ErrorMessage = "Tên đơn vị tính không được để trống")]
        [StringLength(50, ErrorMessage = "Tên đơn vị tính không được quá 50 ký tự")]
        public string Ten_Don_Vi_Tinh { get; set; }

        public string Ghi_Chu { get; set; }
    }
}