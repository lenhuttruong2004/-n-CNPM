using System.ComponentModel.DataAnnotations;
using System;

namespace QuanLyDonViTinh.Models
{
    public class XuatKho
    {
        // === SỬA === (Đổi "Ma_XK" thành "Id")
        public int Id { get; set; }

        [Required(ErrorMessage = "Số phiếu không được để trống")]
        [StringLength(50, ErrorMessage = "Số phiếu tối đa 50 ký tự")] // Thêm
        public string So_Phieu_Xuat_Kho { get; set; }

        [Required(ErrorMessage = "Kho không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn Kho")] // Thêm
        public int Kho_ID { get; set; }

        [Required(ErrorMessage = "Ngày xuất không được để trống")]
        public DateTime Ngay_Xuat_Kho { get; set; } = DateTime.Today;

        [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")] // Thêm
        public string Ghi_Chu { get; set; }

        // Thuộc tính hiển thị
        public string Ten_Kho { get; set; }
        public decimal Tong_Tien { get; set; }
    }
}