using QuanLyDonViTinh.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Configuration; // Đảm bảo có dòng này

namespace QuanLyDonViTinh.Services
{
    public class ReportService
    {
        private readonly string _connectionString;

        public ReportService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        /* HÀM LẤY DỮ LIỆU BÁO CÁO XUẤT NHẬP TỒN (MỚI) */
        public async Task<IEnumerable<BaoCaoXuatNhapTonViewModel>> GetBaoCaoXuatNhapTon(DateTime tuNgay, DateTime denNgay)
        {
            // SQL Query phức tạp để tính tồn kho
            string sql = @"
WITH Nhap AS (
    -- Tổng hợp nhập kho theo sản phẩm và ngày
    SELECT 
        nkr.San_Pham_ID, 
        nk.Ngay_Nhap_Kho,
        SUM(nkr.SL_Nhap) AS SL_Nhap
    FROM tbl_DM_Nhap_Kho_Raw_Data nkr
    INNER JOIN tbl_DM_Nhap_Kho nk ON nkr.Nhap_Kho_ID = nk.Ma_NK
    GROUP BY nkr.San_Pham_ID, nk.Ngay_Nhap_Kho
),
Xuat AS (
    -- Tổng hợp xuất kho theo sản phẩm và ngày
    SELECT 
        xkr.San_Pham_ID, 
        xk.Ngay_Xuat_Kho,
        SUM(xkr.SL_Xuat) AS SL_Xuat
    FROM tbl_DM_Xuat_Kho_Raw_Data xkr
    INNER JOIN tbl_DM_Xuat_Kho xk ON xkr.Xuat_Kho_ID = xk.Ma_XK
    GROUP BY xkr.San_Pham_ID, xk.Ngay_Xuat_Kho
),
DauKy AS (
    -- Tính tồn đầu kỳ (trước @TuNgay)
    SELECT 
        San_Pham_ID,
        SUM(ISNULL(SL_Nhap, 0)) - SUM(ISNULL(SL_Xuat, 0)) AS SL_Dau_Ky
    FROM (
        SELECT San_Pham_ID, Ngay_Nhap_Kho AS Ngay, SL_Nhap, NULL AS SL_Xuat FROM Nhap WHERE Ngay_Nhap_Kho < @TuNgay
        UNION ALL
        SELECT San_Pham_ID, Ngay_Xuat_Kho AS Ngay, NULL AS SL_Nhap, SL_Xuat FROM Xuat WHERE Ngay_Xuat_Kho < @TuNgay
    ) AS PhatSinhTruocKy
    GROUP BY San_Pham_ID
),
TrongKy AS (
    -- Tính phát sinh trong kỳ (@TuNgay đến @DenNgay)
    SELECT 
        San_Pham_ID,
        SUM(ISNULL(SL_Nhap, 0)) AS SL_Nhap,
        SUM(ISNULL(SL_Xuat, 0)) AS SL_Xuat
    FROM (
        SELECT San_Pham_ID, Ngay_Nhap_Kho AS Ngay, SL_Nhap, NULL AS SL_Xuat FROM Nhap WHERE Ngay_Nhap_Kho >= @TuNgay AND Ngay_Nhap_Kho <= @DenNgay
        UNION ALL
        SELECT San_Pham_ID, Ngay_Xuat_Kho AS Ngay, NULL AS SL_Nhap, SL_Xuat FROM Xuat WHERE Ngay_Xuat_Kho >= @TuNgay AND Ngay_Xuat_Kho <= @DenNgay
    ) AS PhatSinhTrongKy
    GROUP BY San_Pham_ID
)
-- Kết hợp Đầu kỳ, Trong kỳ và Thông tin Sản phẩm
SELECT 
    sp.Ma_San_Pham AS San_Pham_ID, -- Lấy ID gốc từ bảng SP
    sp.Ma_San_Pham AS Ma_San_Pham_Code, -- Lấy Mã SP để hiển thị (giả định cột này tồn tại)
    sp.Ten_San_Pham,
    ISNULL(dk.SL_Dau_Ky, 0) AS SL_Dau_Ky,
    ISNULL(tk.SL_Nhap, 0) AS SL_Nhap,
    ISNULL(tk.SL_Xuat, 0) AS SL_Xuat,
    (ISNULL(dk.SL_Dau_Ky, 0) + ISNULL(tk.SL_Nhap, 0) - ISNULL(tk.SL_Xuat, 0)) AS SL_Cuoi_Ky
FROM tbl_DM_San_Pham sp -- Bắt đầu từ bảng Sản phẩm để lấy tất cả SP
LEFT JOIN DauKy dk ON sp.Ma_San_Pham = dk.San_Pham_ID
LEFT JOIN TrongKy tk ON sp.Ma_San_Pham = tk.San_Pham_ID
-- Chỉ hiển thị những sản phẩm có phát sinh hoặc tồn đầu kỳ
WHERE ISNULL(dk.SL_Dau_Ky, 0) != 0 OR ISNULL(tk.SL_Nhap, 0) != 0 OR ISNULL(tk.SL_Xuat, 0) != 0 
ORDER BY sp.Ten_San_Pham;
            ";

            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryAsync<BaoCaoXuatNhapTonViewModel>(sql, new { TuNgay = tuNgay, DenNgay = denNgay });
            }
        }
    }
}
