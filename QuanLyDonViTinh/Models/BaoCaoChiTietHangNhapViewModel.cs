using System;

namespace QuanLyDonViTinh.Models // Quan trọng: Phải đúng namespace này
{
    public class BaoCaoChiTietHangNhapViewModel
    {
        public DateTime Ngay_Nhap_Kho { get; set; }
        public string So_Phieu_Nhap_Kho { get; set; }
        public string Ten_NCC { get; set; }
        public int San_Pham_ID { get; set; }

        // Mã sản phẩm (hiển thị mã người dùng nhìn thấy)
        public string Ma_San_Pham { get; set; }

        public string Ten_San_Pham { get; set; }

        // Số lượng và Đơn giá dùng decimal để tính toán tiền tệ chính xác
        public decimal SL_Nhap { get; set; }
        public decimal Don_Gia_Nhap { get; set; }

        // Thuộc tính tính toán tự động (không cần lưu DB)
        public decimal Tri_Gia => SL_Nhap * Don_Gia_Nhap;
        
    }
}