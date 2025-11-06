using System.Collections.Generic;

namespace QuanLyDonViTinh.Models
{
    public class NhapKhoFull
    {
        // Chứa dữ liệu Header
        public NhapKho Header { get; set; } = new NhapKho();

        // Chứa danh sách Chi tiết
        public List<NhapKhoRawData> Details { get; set; } = new List<NhapKhoRawData>();
    }
}
