using System;

namespace QuanLyDonViTinh.Models
{
    public class BaoCaoChiTietHangNhapViewModel
    {
        public DateTime Ngay_Nhap_Kho { get; set; }
        public string So_Phieu_Nhap_Kho { get; set; }
        public string Ten_NCC { get; set; }
        public int San_Pham_ID { get; set; }

        // === THÊM === (Để hiển thị mã nghiệp vụ)
        public string Ma_San_Pham { get; set; }

        public string Ten_San_Pham { get; set; }

        // === SỬA === (Đổi "int" thành "decimal")
        public decimal SL_Nhap { get; set; }

        public decimal Don_Gia_Nhap { get; set; }
        public decimal Tri_Gia => SL_Nhap * Don_Gia_Nhap;
    }
}