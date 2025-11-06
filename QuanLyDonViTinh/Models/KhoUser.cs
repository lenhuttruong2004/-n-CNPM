using System.ComponentModel.DataAnnotations;

namespace QuanLyDonViTinh.Models
{
    public class KhoUser
    {
        // Hai thuộc tính này tạo thành khóa chính tổng hợp, nên bắt buộc
        [Required(ErrorMessage = "Mã đăng nhập không được để trống")]
        public string Ma_Dang_Nhap { get; set; }

        [Required(ErrorMessage = "Kho không được để trống")]
        public int Kho_ID { get; set; }

        // Thuộc tính để hiển thị Tên Kho (dùng JOIN)
        public string Ten_Kho { get; set; }
    }
}