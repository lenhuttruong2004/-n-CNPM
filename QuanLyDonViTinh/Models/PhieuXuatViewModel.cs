using System.Collections.Generic;

namespace QuanLyDonViTinh.Models // Đảm bảo đúng Namespace
{
    // Model chứa dữ liệu đã xử lý để in Phiếu Xuất
    public class PhieuXuatViewModel
    {
        public XuatKho Header { get; set; } = new XuatKho();
        public List<XuatKhoRawData> Details { get; set; } = new List<XuatKhoRawData>();

        // Dữ liệu giả lập hoặc cần tính toán thêm
        public string LenhGiaoHang { get; set; } = "425/07"; // Dữ liệu giả
        public string CuaHang { get; set; } = "Khách lẻ"; // Dữ liệu giả
        public string TongSoLuongVietSo { get; set; }
        public string TongSoLuongVietChu { get; set; }
        public decimal TongTienSo { get; set; }      // Tổng tiền bằng số
        public string TongTienVietChu { get; set; }  // Tổng tiền bằng chữ
    }
}
