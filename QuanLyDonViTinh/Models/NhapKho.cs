using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace QuanLyDonViTinh.Models
{
    public class NhapKho
    {
        public int Ma_NK { get; set; } // PK nội bộ

        [Required(ErrorMessage = "Số phiếu không được để trống")]
        public string So_Phieu_Nhap_Kho { get; set; } // Key nghiệp vụ, Unique

        [Required(ErrorMessage = "Kho không được để trống")]
        public int Kho_ID { get; set; }

        [Required(ErrorMessage = "Nhà cung cấp không được để trống")]
        public int NCC_ID { get; set; }

        [Required(ErrorMessage = "Ngày nhập không được để trống")]
        public DateTime Ngay_Nhap_Kho { get; set; } = DateTime.Today; // Gán giá trị mặc định

        public string Ghi_Chu { get; set; }

        // Thuộc tính hiển thị (không có trong DB)
        public string Ten_Kho { get; set; }
        public string Ten_NCC { get; set; }

        // Tính tổng tiền (không có trong DB)
        public decimal Tong_Tien { get; set; }
    }
}