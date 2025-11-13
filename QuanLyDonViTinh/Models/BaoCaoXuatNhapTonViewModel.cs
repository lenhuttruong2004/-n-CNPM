namespace QuanLyDonViTinh.Models
{
    public class BaoCaoXuatNhapTonViewModel
    {
        public int San_Pham_ID { get; set; }
        public string Ma_San_Pham { get; set; }
        public string Ten_San_Pham { get; set; }

        // === SỬA === (Đổi tất cả "int" thành "decimal")
        public decimal SL_Dau_Ky { get; set; }
        public decimal SL_Nhap { get; set; }
        public decimal SL_Xuat { get; set; }
        public decimal SL_Cuoi_Ky { get; set; }
    }
}