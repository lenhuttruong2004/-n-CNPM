using System;

namespace QuanLyDonViTinh.Models
{
    public class BaoCaoChiTietHangXuatViewModel
    {
        public DateTime Ngay_Xuat_Kho { get; set; }
        public string So_Phieu_Xuat_Kho { get; set; }
        public int San_Pham_ID { get; set; }

        // === THÊM === (Để hiển thị mã nghiệp vụ)
        public string Ma_San_Pham { get; set; }

        public string Ten_San_Pham { get; set; }

        // === SỬA === (Đổi "int" thành "decimal")
        public decimal SL_Xuat { get; set; }

        public decimal Don_Gia_Xuat { get; set; }
        public decimal Tri_Gia => SL_Xuat * Don_Gia_Xuat;
    }
}