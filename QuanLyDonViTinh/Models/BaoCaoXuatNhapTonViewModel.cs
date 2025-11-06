namespace QuanLyDonViTinh.Models // Đảm bảo đúng Namespace
{
    // Model chứa dữ liệu cho Báo cáo Xuất Nhập Tồn
    public class BaoCaoXuatNhapTonViewModel
    {
        public int San_Pham_ID { get; set; } // Giữ lại ID để tham chiếu
        public string Ma_San_Pham { get; set; } // Cần lấy mã từ bảng SP
        public string Ten_San_Pham { get; set; } // Lấy từ JOIN

        public int SL_Dau_Ky { get; set; }
        public int SL_Nhap { get; set; }
        public int SL_Xuat { get; set; }
        public int SL_Cuoi_Ky { get; set; }
    }
}
