using QuanLyDonViTinh.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace QuanLyDonViTinh.Services
{
    public class XuatKhoService
    {
        private readonly string _connectionString;

        public XuatKhoService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        /* HÀM LẤY DANH SÁCH PHIẾU (Đã đúng) */
        public async Task<IEnumerable<XuatKho>> GetDanhSach()
        {
            // (Code này đã được sửa đúng ở các bước trước)
            string sql = @"
                WITH LatestEdits AS (
                    SELECT *, ROW_NUMBER() OVER(PARTITION BY Ma_XK_Goc ORDER BY Ngay_Hieu_Chinh DESC) as rn
                    FROM tbl_XNK_Xuat_Kho
                ),
                EditedData AS (
                    SELECT 
                        xnk.Ma_XK_Goc AS Id,
                        xnk.So_Phieu_Xuat_Kho, xnk.Ngay_Xuat_Kho, xnk.Ghi_Chu,
                        k.Ten_Kho,
                        ISNULL(SUM(xkr.SL_Xuat * xkr.Don_Gia_Xuat), 0) AS Tong_Tien
                    FROM LatestEdits xnk
                    LEFT JOIN tbl_DM_Kho k ON xnk.Kho_ID = k.Id
                    LEFT JOIN tbl_DM_Xuat_Kho_Raw_Data xkr ON xnk.Ma_XK_Goc = xkr.Xuat_Kho_ID 
                    WHERE xnk.rn = 1
                    GROUP BY 
                        xnk.Ma_XK_Goc, xnk.So_Phieu_Xuat_Kho, xnk.Ngay_Xuat_Kho, xnk.Ghi_Chu, k.Ten_Kho
                ),
                OriginalData AS (
                    SELECT 
                        xk.Id, xk.So_Phieu_Xuat_Kho, xk.Ngay_Xuat_Kho, xk.Ghi_Chu,
                        k.Ten_Kho,
                        ISNULL(SUM(xkr.SL_Xuat * xkr.Don_Gia_Xuat), 0) AS Tong_Tien
                    FROM tbl_DM_Xuat_Kho xk
                    LEFT JOIN tbl_DM_Kho k ON xk.Kho_ID = k.Id
                    LEFT JOIN tbl_DM_Xuat_Kho_Raw_Data xkr ON xk.Id = xkr.Xuat_Kho_ID
                    WHERE xk.Id NOT IN (SELECT Id FROM EditedData)
                    GROUP BY 
                        xk.Id, xk.So_Phieu_Xuat_Kho, xk.Ngay_Xuat_Kho, xk.Ghi_Chu, k.Ten_Kho
                )
                SELECT * FROM EditedData
                UNION ALL
                SELECT * FROM OriginalData
                ORDER BY Id DESC;
            ";
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryAsync<XuatKho>(sql);
            }
        }

        /* HÀM LẤY 1 PHIẾU THEO ID (Đã đúng) */
        public async Task<XuatKho> GetPhieuXuatById(int id)
        {
            // (Code này đã được sửa đúng ở các bước trước)
            string sqlXNK = @"
                SELECT TOP 1 
                    Ma_XK_Goc AS Id, So_Phieu_Xuat_Kho, Kho_ID, Ngay_Xuat_Kho, Ghi_Chu 
                FROM tbl_XNK_Xuat_Kho 
                WHERE Ma_XK_Goc = @Id
                ORDER BY Ngay_Hieu_Chinh DESC";

            using (var connection = new SqlConnection(_connectionString))
            {
                var editedVersion = await connection.QuerySingleOrDefaultAsync<XuatKho>(sqlXNK, new { Id = id });
                if (editedVersion != null) { return editedVersion; }

                string sqlDM = @"
                    SELECT Id, So_Phieu_Xuat_Kho, Kho_ID, Ngay_Xuat_Kho, Ghi_Chu 
                    FROM tbl_DM_Xuat_Kho 
                    WHERE Id = @Id";
                return await connection.QuerySingleOrDefaultAsync<XuatKho>(sqlDM, new { Id = id });
            }
        }

        /* === SỬA CÂU SQL CỦA HÀM NÀY === */
        public async Task<List<XuatKhoRawData>> GetChiTiet(int id)
        {
            // Sửa: Thêm sp.Ma_San_Pham và dvt.Ten_Don_Vi_Tinh
            string sql = @"
                SELECT 
                    xkr.Id, xkr.Xuat_Kho_ID, xkr.San_Pham_ID, xkr.SL_Xuat, xkr.Don_Gia_Xuat,
                    sp.Ma_San_Pham,
                    sp.Ten_San_Pham,
                    dvt.Ten_Don_Vi_Tinh
                FROM tbl_DM_Xuat_Kho_Raw_Data xkr
                LEFT JOIN tbl_DM_San_Pham sp ON xkr.San_Pham_ID = sp.Id
                LEFT JOIN tbl_DM_Don_Vi_Tinh dvt ON sp.Don_Vi_Tinh_ID = dvt.Id
                WHERE xkr.Xuat_Kho_ID = @Id
            ";
            using (var connection = new SqlConnection(_connectionString))
            {
                // (Sửa ID thành Id trong Dapper)
                var result = await connection.QueryAsync<XuatKhoRawData>(sql, new { Id = id });
                return result.ToList();
            }
        }

        /* HÀM THÊM MỚI PHIẾU (Đã đúng) */
        public async Task AddPhieuXuat(XuatKhoFull phieuXuatFull)
        {
            // (Code này đã được sửa đúng ở các bước trước)
            if (phieuXuatFull.Details == null || !phieuXuatFull.Details.Any())
                throw new Exception("Phiếu xuất phải có ít nhất một sản phẩm chi tiết.");

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string headerSql = @"
                            INSERT INTO tbl_DM_Xuat_Kho (So_Phieu_Xuat_Kho, Kho_ID, Ngay_Xuat_Kho, Ghi_Chu) 
                            VALUES (@So_Phieu_Xuat_Kho, @Kho_ID, @Ngay_Xuat_Kho, @Ghi_Chu);
                            SELECT CAST(SCOPE_IDENTITY() as int);";
                        int newId = await connection.QuerySingleAsync<int>(headerSql, phieuXuatFull.Header, transaction: transaction);

                        string detailSql = @"
                            INSERT INTO tbl_DM_Xuat_Kho_Raw_Data (Xuat_Kho_ID, San_Pham_ID, SL_Xuat, Don_Gia_Xuat) 
                            VALUES (@Xuat_Kho_ID, @San_Pham_ID, @SL_Xuat, @Don_Gia_Xuat)";
                        foreach (var detail in phieuXuatFull.Details)
                        {
                            detail.Xuat_Kho_ID = newId;
                            await connection.ExecuteAsync(detailSql, detail, transaction: transaction);
                        }
                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 2627 || ex.Number == 2601) { transaction.Rollback(); throw new Exception("Lỗi: Số phiếu xuất này đã tồn tại."); }
                        transaction.Rollback(); throw;
                    }
                    catch (Exception) { transaction.Rollback(); throw; }
                }
            }
        }

        /* HÀM CẬP NHẬT PHIẾU (Đã đúng) */
        public async Task UpdatePhieuXuat(XuatKho xuatKho)
        {
            // (Code này đã được sửa đúng ở các bước trước)
            string sql = @"
                INSERT INTO tbl_XNK_Xuat_Kho 
                    (Ma_XK_Goc, So_Phieu_Xuat_Kho, Kho_ID, Ngay_Xuat_Kho, Ghi_Chu) 
                VALUES 
                    (@Id, @So_Phieu_Xuat_Kho, @Kho_ID, @Ngay_Xuat_Kho, @Ghi_Chu)";
            using (var connection = new SqlConnection(_connectionString))
            {
                try { await connection.ExecuteAsync(sql, xuatKho); }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627 || ex.Number == 2601) { throw new Exception("Lỗi: Số phiếu xuất (hiệu chỉnh) này đã tồn tại."); }
                    throw;
                }
            }
        }

        /* HÀM XÓA PHIẾU (Đã đúng) */
        public async Task DeletePhieuXuat(int id)
        {
            // (Code này đã được sửa đúng ở các bước trước)
            string sql = "DELETE FROM tbl_DM_Xuat_Kho WHERE Id = @Id";
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(sql, new { Id = id });
            }
        }

        /* HÀM CRUD CHI TIẾT (Đã đúng) */
        public async Task UpdateChiTiet(XuatKhoRawData detail)
        {
            // (Code này đã đúng, chỉ sửa ID)
            string sql = "UPDATE tbl_DM_Xuat_Kho_Raw_Data SET SL_Xuat = @SL_Xuat, Don_Gia_Xuat = @Don_Gia_Xuat WHERE Id = @Id";
            using (var connection = new SqlConnection(_connectionString)) { await connection.ExecuteAsync(sql, detail); }
        }
        public async Task AddChiTiet(XuatKhoRawData detail)
        {
            // (Code này đã đúng)
            string sql = "INSERT INTO tbl_DM_Xuat_Kho_Raw_Data (Xuat_Kho_ID, San_Pham_ID, SL_Xuat, Don_Gia_Xuat) VALUES (@Xuat_Kho_ID, @San_Pham_ID, @SL_Xuat, @Don_Gia_Xuat)";
            using (var connection = new SqlConnection(_connectionString)) { await connection.ExecuteAsync(sql, detail); }
        }
        public async Task DeleteChiTiet(int id)
        {
            // (Code này đã đúng)
            string sql = "DELETE FROM tbl_DM_Xuat_Kho_Raw_Data WHERE Id = @Id";
            using (var connection = new SqlConnection(_connectionString)) { await connection.ExecuteAsync(sql, new { Id = id }); }
        }

        /* HÀM LẤY DỮ LIỆU ĐỂ IN (Đã đúng) */
        public async Task<PhieuXuatViewModel> GetPhieuXuatView(int id)
        {
            // (Code này đã được sửa đúng ở các bước trước)
            var header = await GetPhieuXuatById(id);
            if (header == null) return null;
            var details = await GetChiTiet(id);
            var viewModel = new PhieuXuatViewModel
            {
                Header = header,
                Details = details,
                TongSoLuongVietSo = details.Sum(d => d.SL_Xuat).ToString("N2"), // Sửa N0 -> N2
                TongSoLuongVietChu = "..."
            };
            return viewModel;
        }

        /* === SỬA CÂU SQL CỦA HÀM NÀY === */
        public async Task<IEnumerable<BaoCaoChiTietHangXuatViewModel>> GetBaoCaoChiTietHangXuat(DateTime tuNgay, DateTime denNgay)
        {
            // Sửa: Thêm sp.Ma_San_Pham
            string sql = @"
                SELECT 
                    xk.Ngay_Xuat_Kho, xk.So_Phieu_Xuat_Kho, 
                    xkr.San_Pham_ID, sp.Ma_San_Pham, sp.Ten_San_Pham, 
                    xkr.SL_Xuat, xkr.Don_Gia_Xuat
                FROM tbl_DM_Xuat_Kho_Raw_Data xkr
                INNER JOIN tbl_DM_Xuat_Kho xk ON xkr.Xuat_Kho_ID = xk.Id
                INNER JOIN tbl_DM_San_Pham sp ON xkr.San_Pham_ID = sp.Id
                WHERE xk.Ngay_Xuat_Kho >= @TuNgay AND xk.Ngay_Xuat_Kho <= @DenNgay
                ORDER BY xk.Ngay_Xuat_Kho, xk.So_Phieu_Xuat_Kho, sp.Ten_San_Pham;
            ";
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryAsync<BaoCaoChiTietHangXuatViewModel>(sql, new { TuNgay = tuNgay, DenNgay = denNgay });
            }
        }
    }
}