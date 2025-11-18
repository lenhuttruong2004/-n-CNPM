using System.ComponentModel.DataAnnotations;

namespace QuanLyDonViTinh.Models
{
    public class NhaCungCap
    {
        // === KHÓA CHÍNH (Primary Key) ===
        public int Id { get; set; }

        // === Mã nhà cung cấp (Nghiệp vụ) ===
        [Required(ErrorMessage = "Mã nhà cung cấp không được để trống")]
        [StringLength(50, ErrorMessage = "Mã nhà cung cấp tối đa 50 ký tự")]
        public string Ma_NCC { get; set; }

        // === Tên nhà cung cấp ===
        [Required(ErrorMessage = "Tên nhà cung cấp không được để trống")]
        [StringLength(200, ErrorMessage = "Tên nhà cung cấp tối đa 200 ký tự")]
        public string Ten_NCC { get; set; }

        // === Ghi chú ===
        [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
        public string Ghi_Chu { get; set; }
    }
}
