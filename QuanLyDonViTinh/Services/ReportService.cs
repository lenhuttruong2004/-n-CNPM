using QuanLyDonViTinh.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Configuration;

namespace QuanLyDonViTinh.Services
{
    public class ReportService
    {
        private readonly string _connectionString;

        public ReportService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<BaoCaoXuatNhapTonViewModel>> GetBaoCaoXuatNhapTon(DateTime tuNgay, DateTime denNgay)
        {
            // QUAN TRỌNG: Chỉnh 'Đến ngày' thành cuối ngày (23:59:59) để lấy hết dữ liệu trong ngày đó
            var denNgayCuoiNgay = denNgay.Date.AddDays(1).AddTicks(-1);

            string sql = @"
            WITH Nhap AS (
                SELECT 
                    nkr.San_Pham_ID, 
                    nk.Ngay_Nhap_Kho,
                    SUM(nkr.SL_Nhap) AS SL_Nhap
                FROM tbl_DM_Nhap_Kho_Raw_Data nkr
                INNER JOIN tbl_DM_Nhap_Kho nk ON nkr.Nhap_Kho_ID = nk.Id 
                GROUP BY nkr.San_Pham_ID, nk.Ngay_Nhap_Kho
            ),
            Xuat AS (
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
                    SELECT San_Pham_ID, SL_Nhap, CAST(0 AS decimal(18,2)) AS SL_Xuat FROM Nhap WHERE Ngay_Nhap_Kho < @TuNgay
                    UNION ALL
                    SELECT San_Pham_ID, CAST(0 AS decimal(18,2)) AS SL_Nhap, SL_Xuat FROM Xuat WHERE Ngay_Xuat_Kho < @TuNgay
                ) AS PhatSinhTruocKy
                GROUP BY San_Pham_ID
            ),
            TrongKy AS (
                SELECT 
                    San_Pham_ID,
                    SUM(ISNULL(SL_Nhap, 0)) AS SL_Nhap,
                    SUM(ISNULL(SL_Xuat, 0)) AS SL_Xuat
                FROM (
                    SELECT San_Pham_ID, SL_Nhap, CAST(0 AS decimal(18,2)) AS SL_Xuat FROM Nhap WHERE Ngay_Nhap_Kho >= @TuNgay AND Ngay_Nhap_Kho <= @DenNgay
                    UNION ALL
                    SELECT San_Pham_ID, CAST(0 AS decimal(18,2)) AS SL_Nhap, SL_Xuat FROM Xuat WHERE Ngay_Xuat_Kho >= @TuNgay AND Ngay_Xuat_Kho <= @DenNgay
                ) AS PhatSinhTrongKy
                GROUP BY San_Pham_ID
            )
            SELECT 
                sp.Id AS San_Pham_ID,
                sp.Ma_San_Pham,
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
                // Truyền biến denNgayCuoiNgay vào tham số @DenNgay
                return await connection.QueryAsync<BaoCaoXuatNhapTonViewModel>(sql, new { TuNgay = tuNgay, DenNgay = denNgayCuoiNgay });
            }
        }
    }
}