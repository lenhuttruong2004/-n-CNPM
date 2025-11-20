using System.ComponentModel.DataAnnotations;

namespace QuanLyDonViTinh.Models
{
    public class NhaCungCap
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã nhà cung cấp không được để trống")]
        [StringLength(50, ErrorMessage = "Mã nhà cung cấp tối đa 50 ký tự")]
        // === BỔ SUNG RANG BUỘC REGEX ===
        [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "Mã NCC chỉ được chứa chữ cái (không dấu) và số.")]
        public string Ma_NCC { get; set; }

        [Required(ErrorMessage = "Tên nhà cung cấp không được để trống")]
        [StringLength(200, ErrorMessage = "Tên nhà cung cấp tối đa 200 ký tự")]
        public string Ten_NCC { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
        public string Ghi_Chu { get; set; }
    }
}