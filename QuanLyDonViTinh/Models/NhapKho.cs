using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace QuanLyDonViTinh.Models
{
    public class NhapKho
    {
        // === SỬA === (Đổi Ma_NK thành Id để khớp với PK của DB)
        public int Id { get; set; }

        [Required(ErrorMessage = "Số phiếu không được để trống")]
        public string So_Phieu_Nhap_Kho { get; set; }

        [Required(ErrorMessage = "Kho không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn Kho")] // Thêm
        public int Kho_ID { get; set; }

        [Required(ErrorMessage = "Nhà cung cấp không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn NCC")] // Thêm
        public int NCC_ID { get; set; }

        [Required(ErrorMessage = "Ngày nhập không được để trống")]
        public DateTime Ngay_Nhap_Kho { get; set; } = DateTime.Today;

        [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")] // Thêm
        public string Ghi_Chu { get; set; }

        // Thuộc tính hiển thị (không có trong DB)
        public string Ten_Kho { get; set; }
        public string Ten_NCC { get; set; }

        // Tính tổng tiền (không có trong DB)
        public decimal Tong_Tien { get; set; }
    }
}