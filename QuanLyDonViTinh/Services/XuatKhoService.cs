using QuanLyDonViTinh.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace QuanLyDonViTinh.Services
{
    public class XuatKhoService
    {
        private readonly string _connectionString;

        public XuatKhoService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        /* HÀM LẤY DANH SÁCH PHIẾU (Logic 12 Bảng) */
        public async Task<IEnumerable<XuatKho>> GetDanhSach()
        {
            string sql = @"
                WITH LatestEdits AS (
                    SELECT *, ROW_NUMBER() OVER(PARTITION BY Ma_XK_Goc ORDER BY Ngay_Hieu_Chinh DESC) as rn
                    FROM tbl_XNK_Xuat_Kho
                ),
                EditedData AS (
                    SELECT 
                        xnk.Ma_XK_Goc AS Ma_XK,
                        xnk.So_Phieu_Xuat_Kho, xnk.Ngay_Xuat_Kho, xnk.Ghi_Chu,
                        k.Ten_Kho,
                        ISNULL(SUM(xkr.SL_Xuat * xkr.Don_Gia_Xuat), 0) AS Tong_Tien
                    FROM LatestEdits xnk
                    LEFT JOIN tbl_DM_Kho k ON xnk.Kho_ID = k.Ma_Kho
                    LEFT JOIN tbl_DM_Xuat_Kho_Raw_Data xkr ON xnk.Ma_XK_Goc = xkr.Xuat_Kho_ID 
                    WHERE xnk.rn = 1
                    GROUP BY 
                        xnk.Ma_XK_Goc, xnk.So_Phieu_Xuat_Kho, xnk.Ngay_Xuat_Kho, xnk.Ghi_Chu, k.Ten_Kho
                ),
                OriginalData AS (
                    SELECT 
                        xk.Ma_XK, xk.So_Phieu_Xuat_Kho, xk.Ngay_Xuat_Kho, xk.Ghi_Chu,
                        k.Ten_Kho,
                        ISNULL(SUM(xkr.SL_Xuat * xkr.Don_Gia_Xuat), 0) AS Tong_Tien
                    FROM tbl_DM_Xuat_Kho xk
                    LEFT JOIN tbl_DM_Kho k ON xk.Kho_ID = k.Ma_Kho
                    LEFT JOIN tbl_DM_Xuat_Kho_Raw_Data xkr ON xk.Ma_XK = xkr.Xuat_Kho_ID
                    WHERE xk.Ma_XK NOT IN (SELECT Ma_XK FROM EditedData)
                    GROUP BY 
                        xk.Ma_XK, xk.So_Phieu_Xuat_Kho, xk.Ngay_Xuat_Kho, xk.Ghi_Chu, k.Ten_Kho
                )
                SELECT * FROM EditedData
                UNION ALL
                SELECT * FROM OriginalData
                ORDER BY Ma_XK DESC;
            ";
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryAsync<XuatKho>(sql);
            }
        }

        /* HÀM LẤY 1 PHIẾU THEO ID (Logic 12 Bảng) */
        public async Task<XuatKho> GetPhieuXuatById(int maXK)
        {
            string sqlXNK = @"
                SELECT TOP 1 
                    Ma_XK_Goc AS Ma_XK, So_Phieu_Xuat_Kho, Kho_ID, Ngay_Xuat_Kho, Ghi_Chu 
                FROM tbl_XNK_Xuat_Kho 
                WHERE Ma_XK_Goc = @Ma_XK
                ORDER BY Ngay_Hieu_Chinh DESC";

            using (var connection = new SqlConnection(_connectionString))
            {
                var editedVersion = await connection.QuerySingleOrDefaultAsync<XuatKho>(sqlXNK, new { Ma_XK = maXK });
                if (editedVersion != null) { return editedVersion; }

                string sqlDM = @"
                    SELECT Ma_XK, So_Phieu_Xuat_Kho, Kho_ID, Ngay_Xuat_Kho, Ghi_Chu 
                    FROM tbl_DM_Xuat_Kho 
                    WHERE Ma_XK = @Ma_XK";
                return await connection.QuerySingleOrDefaultAsync<XuatKho>(sqlDM, new { Ma_XK = maXK });
            }
        }

        /* HÀM LẤY CHI TIẾT (Bài 13) */
        public async Task<List<XuatKhoRawData>> GetChiTiet(int maXK)
        {
            string sql = @"
                SELECT 
                    xkr.ID, xkr.Xuat_Kho_ID, xkr.San_Pham_ID, xkr.SL_Xuat, xkr.Don_Gia_Xuat,
                    sp.Ten_San_Pham
                FROM tbl_DM_Xuat_Kho_Raw_Data xkr
                LEFT JOIN tbl_DM_San_Pham sp ON xkr.San_Pham_ID = sp.Ma_San_Pham
                WHERE xkr.Xuat_Kho_ID = @Ma_XK
            ";
            using (var connection = new SqlConnection(_connectionString))
            {
                var result = await connection.QueryAsync<XuatKhoRawData>(sql, new { Ma_XK = maXK });
                return result.ToList();
            }
        }

        /* HÀM THÊM MỚI PHIẾU (Bài 11) */
        public async Task AddPhieuXuat(XuatKhoFull phieuXuatFull)
        {
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
                        int maXkMoi = await connection.QuerySingleAsync<int>(headerSql, phieuXuatFull.Header, transaction: transaction);

                        string detailSql = @"
                            INSERT INTO tbl_DM_Xuat_Kho_Raw_Data (Xuat_Kho_ID, San_Pham_ID, SL_Xuat, Don_Gia_Xuat) 
                            VALUES (@Xuat_Kho_ID, @San_Pham_ID, @SL_Xuat, @Don_Gia_Xuat)";
                        foreach (var detail in phieuXuatFull.Details)
                        {
                            detail.Xuat_Kho_ID = maXkMoi;
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

        /* HÀM CẬP NHẬT PHIẾU (Bài 12 - Logic 12 Bảng) */
        public async Task UpdatePhieuXuat(XuatKho xuatKho)
        {
            string sql = @"
                INSERT INTO tbl_XNK_Xuat_Kho 
                    (Ma_XK_Goc, So_Phieu_Xuat_Kho, Kho_ID, Ngay_Xuat_Kho, Ghi_Chu) 
                VALUES 
                    (@Ma_XK, @So_Phieu_Xuat_Kho, @Kho_ID, @Ngay_Xuat_Kho, @Ghi_Chu)";
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

        /* HÀM XÓA PHIẾU (Bài 11) */
        public async Task DeletePhieuXuat(int maXK)
        {
            string sql = "DELETE FROM tbl_DM_Xuat_Kho WHERE Ma_XK = @Ma_XK";
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(sql, new { Ma_XK = maXK });
            }
        }

        /* HÀM CRUD CHI TIẾT (Bài 13) */
        public async Task UpdateChiTiet(XuatKhoRawData detail)
        {
            string sql = "UPDATE tbl_DM_Xuat_Kho_Raw_Data SET SL_Xuat = @SL_Xuat, Don_Gia_Xuat = @Don_Gia_Xuat WHERE ID = @ID";
            using (var connection = new SqlConnection(_connectionString)) { await connection.ExecuteAsync(sql, detail); }
        }
        public async Task AddChiTiet(XuatKhoRawData detail)
        {
            string sql = "INSERT INTO tbl_DM_Xuat_Kho_Raw_Data (Xuat_Kho_ID, San_Pham_ID, SL_Xuat, Don_Gia_Xuat) VALUES (@Xuat_Kho_ID, @San_Pham_ID, @SL_Xuat, @Don_Gia_Xuat)";
            using (var connection = new SqlConnection(_connectionString)) { await connection.ExecuteAsync(sql, detail); }
        }
        public async Task DeleteChiTiet(int id)
        {
            string sql = "DELETE FROM tbl_DM_Xuat_Kho_Raw_Data WHERE ID = @Id";
            using (var connection = new SqlConnection(_connectionString)) { await connection.ExecuteAsync(sql, new { Id = id }); }
        }

        /* HÀM LẤY DỮ LIỆU ĐỂ IN (Bài 14) */
        public async Task<PhieuXuatViewModel> GetPhieuXuatView(int maXK)
        {
            var header = await GetPhieuXuatById(maXK);
            if (header == null) return null;
            var details = await GetChiTiet(maXK);
            var viewModel = new PhieuXuatViewModel
            {
                Header = header,
                Details = details,
                TongSoLuongVietSo = details.Sum(d => d.SL_Xuat).ToString("N0"),
                TongSoLuongVietChu = "..." // Cần hàm đọc số
            };
            return viewModel;
        }

        /* HÀM BÁO CÁO (Bài 16) */
        public async Task<IEnumerable<BaoCaoChiTietHangXuatViewModel>> GetBaoCaoChiTietHangXuat(DateTime tuNgay, DateTime denNgay)
        {
            string sql = @"
                SELECT xk.Ngay_Xuat_Kho, xk.So_Phieu_Xuat_Kho, xkr.San_Pham_ID, sp.Ten_San_Pham, xkr.SL_Xuat, xkr.Don_Gia_Xuat
                FROM tbl_DM_Xuat_Kho_Raw_Data xkr
                INNER JOIN tbl_DM_Xuat_Kho xk ON xkr.Xuat_Kho_ID = xk.Ma_XK
                INNER JOIN tbl_DM_San_Pham sp ON xkr.San_Pham_ID = sp.Ma_San_Pham
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