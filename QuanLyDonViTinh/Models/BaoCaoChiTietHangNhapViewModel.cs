using System;

namespace QuanLyDonViTinh.Models // Đảm bảo đúng Namespace
{
    // Model chứa dữ liệu cho Báo cáo Chi tiết Hàng Nhập
    public class BaoCaoChiTietHangNhapViewModel
    {
        public DateTime Ngay_Nhap_Kho { get; set; }
        public string So_Phieu_Nhap_Kho { get; set; }
        public string Ten_NCC { get; set; } // Lấy từ JOIN
        public int San_Pham_ID { get; set; }
        public string Ten_San_Pham { get; set; } // Lấy từ JOIN
        public int SL_Nhap { get; set; }
        public decimal Don_Gia_Nhap { get; set; }

        // Tính toán Trị giá (Thành tiền)
        public decimal Tri_Gia => SL_Nhap * Don_Gia_Nhap;
    }
}
