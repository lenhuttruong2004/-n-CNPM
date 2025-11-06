using System.ComponentModel.DataAnnotations;

namespace QuanLyDonViTinh.Models
{
    public class NhaCungCap
    {
        public int Ma_NCC { get; set; }

        [Required(ErrorMessage = "Tên nhà cung cấp không được để trống")]
        public string Ten_NCC { get; set; }

        public string Ghi_Chu { get; set; }
    }
}