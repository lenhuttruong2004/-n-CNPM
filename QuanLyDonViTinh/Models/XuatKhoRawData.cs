using System.ComponentModel.DataAnnotations;

namespace QuanLyDonViTinh.Models
{
    public class XuatKhoRawData
    {
        public int ID { get; set; }
        public int Xuat_Kho_ID { get; set; }

        [Required(ErrorMessage = "Sản phẩm không được để trống")]
        public int San_Pham_ID { get; set; }

        [Required(ErrorMessage = "Số lượng không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int SL_Xuat { get; set; }

        [Required(ErrorMessage = "Đơn giá không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn 0")]
        public decimal Don_Gia_Xuat { get; set; }

        public string Ten_San_Pham { get; set; } // Dùng để hiển thị
    }
}