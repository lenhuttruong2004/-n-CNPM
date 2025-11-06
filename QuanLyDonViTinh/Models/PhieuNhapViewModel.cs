using System.Collections.Generic;

namespace QuanLyDonViTinh.Models
{
    // Model này dùng để chứa tất cả dữ liệu đã được xử lý để in
    public class PhieuNhapViewModel
    {
        public NhapKho Header { get; set; } = new NhapKho();
        public List<NhapKhoRawData> Details { get; set; } = new List<NhapKhoRawData>();

        public decimal TongTienSo { get; set; }
        public string TongTienVietChu { get; set; }

        // Thông tin giả lập (theo mẫu)
        public string TenNguoiGiaoHang { get; set; } = "MAGNUSSEN HOME FURNISHINGS INC.";
        public string HoTenNguoiGiaoHang { get; set; } = "MHF";
        public string TheoTKSo { get; set; } = "KNQ Tân Uyên";
    }
}