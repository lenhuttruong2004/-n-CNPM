using System;

namespace QuanLyDonViTinh.Models // Đảm bảo đúng Namespace
{
    // Model chứa dữ liệu cho Báo cáo Chi tiết Hàng Xuất
    public class BaoCaoChiTietHangXuatViewModel
    {
        public DateTime Ngay_Xuat_Kho { get; set; }
        public string So_Phieu_Xuat_Kho { get; set; }
        // Không có NCC trong phiếu xuất
        public int San_Pham_ID { get; set; }
        public string Ten_San_Pham { get; set; } // Lấy từ JOIN
        public int SL_Xuat { get; set; }
        public decimal Don_Gia_Xuat { get; set; }

        // Tính toán Trị giá (Thành tiền)
        public decimal Tri_Gia => SL_Xuat * Don_Gia_Xuat;
    }
}