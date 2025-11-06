using System.ComponentModel.DataAnnotations;
using System;

namespace QuanLyDonViTinh.Models
{
    public class XuatKho
    {
        public int Ma_XK { get; set; }

        [Required(ErrorMessage = "Số phiếu không được để trống")]
        public string So_Phieu_Xuat_Kho { get; set; }

        [Required(ErrorMessage = "Kho không được để trống")]
        public int Kho_ID { get; set; }

        [Required(ErrorMessage = "Ngày xuất không được để trống")]
        public DateTime Ngay_Xuat_Kho { get; set; } = DateTime.Today;

        public string Ghi_Chu { get; set; }

        // Thuộc tính hiển thị
        public string Ten_Kho { get; set; }
        public decimal Tong_Tien { get; set; }
    }
}