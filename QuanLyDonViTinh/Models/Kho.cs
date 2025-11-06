using System.ComponentModel.DataAnnotations;

namespace QuanLyDonViTinh.Models
{
    public class Kho
    {
        public int Ma_Kho { get; set; }

        [Required(ErrorMessage = "Tên kho không được để trống")]
        public string Ten_Kho { get; set; }

        public string Ghi_Chu { get; set; }
    }
}