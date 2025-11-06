using System.Collections.Generic;

namespace QuanLyDonViTinh.Models
{
    public class XuatKhoFull
    {
        public XuatKho Header { get; set; } = new XuatKho();
        public List<XuatKhoRawData> Details { get; set; } = new List<XuatKhoRawData>();
    }
}