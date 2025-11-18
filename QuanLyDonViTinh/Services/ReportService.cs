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

        public async Task<IEnumerable<BaoCaoXuatNhapTonViewModel>> GetBaoCaoXuatNhapTon(DateTime tuNgay, DateTime denNgay)
        {
            string sql = @"
    WITH Nhap AS (
        -- Sửa: JOIN bảng Nhập Kho bằng cột Id
        SELECT 
            nkr.San_Pham_ID, 
            nk.Ngay_Nhap_Kho,
            SUM(nkr.SL_Nhap) AS SL_Nhap
        FROM tbl_DM_Nhap_Kho_Raw_Data nkr
        INNER JOIN tbl_DM_Nhap_Kho nk ON nkr.Nhap_Kho_ID = nk.Id 
        GROUP BY nkr.San_Pham_ID, nk.Ngay_Nhap_Kho
    ),
    Xuat AS (
        -- Sửa: JOIN bảng Xuất Kho bằng cột Id
        SELECT 
            xkr.San_Pham_ID, 
            xk.Ngay_Xuat_Kho,
            SUM(xkr.SL_Xuat) AS SL_Xuat
        FROM tbl_DM_Xuat_Kho_Raw_Data xkr
        INNER JOIN tbl_DM_Xuat_Kho xk ON xkr.Xuat_Kho_ID = xk.Id
        GROUP BY xkr.San_Pham_ID, xk.Ngay_Xuat_Kho
    ),
    DauKy AS (
        SELECT 
            San_Pham_ID,
            SUM(ISNULL(SL_Nhap, 0)) - SUM(ISNULL(SL_Xuat, 0)) AS SL_Dau_Ky
        FROM (
            SELECT San_Pham_ID, SL_Nhap, 0 AS SL_Xuat FROM Nhap WHERE Ngay_Nhap_Kho < @TuNgay
            UNION ALL
            SELECT San_Pham_ID, 0 AS SL_Nhap, SL_Xuat FROM Xuat WHERE Ngay_Xuat_Kho < @TuNgay
        ) AS PhatSinhTruocKy
        GROUP BY San_Pham_ID
    ),
    TrongKy AS (
        SELECT 
            San_Pham_ID,
            SUM(ISNULL(SL_Nhap, 0)) AS SL_Nhap,
            SUM(ISNULL(SL_Xuat, 0)) AS SL_Xuat
        FROM (
            SELECT San_Pham_ID, SL_Nhap, 0 AS SL_Xuat FROM Nhap WHERE Ngay_Nhap_Kho >= @TuNgay AND Ngay_Nhap_Kho <= @DenNgay
            UNION ALL
            SELECT San_Pham_ID, 0 AS SL_Nhap, SL_Xuat FROM Xuat WHERE Ngay_Xuat_Kho >= @TuNgay AND Ngay_Xuat_Kho <= @DenNgay
        ) AS PhatSinhTrongKy
        GROUP BY San_Pham_ID
    )
    SELECT 
        sp.Id AS San_Pham_ID,          -- Sửa: Lấy đúng cột Id số
        sp.Ma_San_Pham,                -- Lấy thêm Mã sản phẩm chữ
        sp.Ten_San_Pham,
        ISNULL(dk.SL_Dau_Ky, 0) AS SL_Dau_Ky,
        ISNULL(tk.SL_Nhap, 0) AS SL_Nhap,
        ISNULL(tk.SL_Xuat, 0) AS SL_Xuat,
        (ISNULL(dk.SL_Dau_Ky, 0) + ISNULL(tk.SL_Nhap, 0) - ISNULL(tk.SL_Xuat, 0)) AS SL_Cuoi_Ky
    FROM tbl_DM_San_Pham sp
    LEFT JOIN DauKy dk ON sp.Id = dk.San_Pham_ID
    LEFT JOIN TrongKy tk ON sp.Id = tk.San_Pham_ID
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
