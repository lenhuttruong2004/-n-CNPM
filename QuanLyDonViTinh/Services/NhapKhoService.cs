using QuanLyDonViTinh.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace QuanLyDonViTinh.Services
{
    public class NhapKhoService
    {
        private readonly string _connectionString;

        public NhapKhoService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        /* HÀM LẤY DANH SÁCH PHIẾU (Logic 12 Bảng) */
        public async Task<IEnumerable<NhapKho>> GetDanhSach()
        {
            string sql = @"
                WITH LatestEdits AS (
                    SELECT *,
                           ROW_NUMBER() OVER(PARTITION BY Ma_NK_Goc ORDER BY Ngay_Hieu_Chinh DESC) as rn
                    FROM tbl_XNK_Nhap_Kho
                ),
                EditedData AS (
                    SELECT 
                        xnk.Ma_NK_Goc AS Ma_NK,
                        xnk.So_Phieu_Nhap_Kho, xnk.Ngay_Nhap_Kho, xnk.Ghi_Chu,
                        k.Ten_Kho, ncc.Ten_NCC,
                        ISNULL(SUM(nkr.SL_Nhap * nkr.Don_Gia_Nhap), 0) AS Tong_Tien
                    FROM LatestEdits xnk
                    LEFT JOIN tbl_DM_Kho k ON xnk.Kho_ID = k.Ma_Kho
                    LEFT JOIN tbl_DM_NCC ncc ON xnk.NCC_ID = ncc.Ma_NCC
                    LEFT JOIN tbl_DM_Nhap_Kho_Raw_Data nkr ON xnk.Ma_NK_Goc = nkr.Nhap_Kho_ID 
                    WHERE xnk.rn = 1
                    GROUP BY 
                        xnk.Ma_NK_Goc, xnk.So_Phieu_Nhap_Kho, xnk.Ngay_Nhap_Kho, xnk.Ghi_Chu,
                        k.Ten_Kho, ncc.Ten_NCC
                ),
                OriginalData AS (
                    SELECT 
                        nk.Ma_NK, nk.So_Phieu_Nhap_Kho, nk.Ngay_Nhap_Kho, nk.Ghi_Chu,
                        k.Ten_Kho, ncc.Ten_NCC,
                        ISNULL(SUM(nkr.SL_Nhap * nkr.Don_Gia_Nhap), 0) AS Tong_Tien
                    FROM tbl_DM_Nhap_Kho nk
                    LEFT JOIN tbl_DM_Kho k ON nk.Kho_ID = k.Ma_Kho
                    LEFT JOIN tbl_DM_NCC ncc ON nk.NCC_ID = ncc.Ma_NCC
                    LEFT JOIN tbl_DM_Nhap_Kho_Raw_Data nkr ON nk.Ma_NK = nkr.Nhap_Kho_ID
                    WHERE nk.Ma_NK NOT IN (SELECT Ma_NK FROM EditedData)
                    GROUP BY 
                        nk.Ma_NK, nk.So_Phieu_Nhap_Kho, nk.Ngay_Nhap_Kho, nk.Ghi_Chu,
                        k.Ten_Kho, ncc.Ten_NCC
                )
                SELECT * FROM EditedData
                UNION ALL
                SELECT * FROM OriginalData
                ORDER BY Ma_NK DESC;
            ";
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryAsync<NhapKho>(sql);
            }
        }

        /* HÀM LẤY 1 PHIẾU THEO ID (Logic 12 Bảng) */
        public async Task<NhapKho> GetPhieuNhapById(int maNK)
        {
            string sqlXNK = @"
                SELECT TOP 1 
                    Ma_NK_Goc AS Ma_NK, So_Phieu_Nhap_Kho, Kho_ID, NCC_ID, Ngay_Nhap_Kho, Ghi_Chu 
                FROM tbl_XNK_Nhap_Kho 
                WHERE Ma_NK_Goc = @Ma_NK
                ORDER BY Ngay_Hieu_Chinh DESC";

            using (var connection = new SqlConnection(_connectionString))
            {
                var editedVersion = await connection.QuerySingleOrDefaultAsync<NhapKho>(sqlXNK, new { Ma_NK = maNK });
                if (editedVersion != null) { return editedVersion; }

                string sqlDM = @"
                    SELECT Ma_NK, So_Phieu_Nhap_Kho, Kho_ID, NCC_ID, Ngay_Nhap_Kho, Ghi_Chu 
                    FROM tbl_DM_Nhap_Kho 
                    WHERE Ma_NK = @Ma_NK";
                return await connection.QuerySingleOrDefaultAsync<NhapKho>(sqlDM, new { Ma_NK = maNK });
            }
        }

        /* HÀM LẤY CHI TIẾT (Bài 9) */
        public async Task<List<NhapKhoRawData>> GetChiTiet(int maNK)
        {
            string sql = @"
                SELECT 
                    nkr.ID, nkr.Nhap_Kho_ID, nkr.San_Pham_ID, nkr.SL_Nhap, nkr.Don_Gia_Nhap,
                    sp.Ten_San_Pham
                FROM tbl_DM_Nhap_Kho_Raw_Data nkr
                LEFT JOIN tbl_DM_San_Pham sp ON nkr.San_Pham_ID = sp.Ma_San_Pham
                WHERE nkr.Nhap_Kho_ID = @Ma_NK
            ";
            using (var connection = new SqlConnection(_connectionString))
            {
                var result = await connection.QueryAsync<NhapKhoRawData>(sql, new { Ma_NK = maNK });
                return result.ToList();
            }
        }

        /* HÀM THÊM MỚI PHIẾU (Bài 7) */
        public async Task AddPhieuNhap(NhapKhoFull phieuNhapFull)
        {
            if (phieuNhapFull.Details == null || !phieuNhapFull.Details.Any())
                throw new Exception("Phiếu nhập phải có ít nhất một sản phẩm chi tiết.");

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string headerSql = @"
                            INSERT INTO tbl_DM_Nhap_Kho (So_Phieu_Nhap_Kho, Kho_ID, NCC_ID, Ngay_Nhap_Kho, Ghi_Chu) 
                            VALUES (@So_Phieu_Nhap_Kho, @Kho_ID, @NCC_ID, @Ngay_Nhap_Kho, @Ghi_Chu);
                            SELECT CAST(SCOPE_IDENTITY() as int);";
                        int maNkMoi = await connection.QuerySingleAsync<int>(headerSql, phieuNhapFull.Header, transaction: transaction);

                        string detailSql = @"
                            INSERT INTO tbl_DM_Nhap_Kho_Raw_Data (Nhap_Kho_ID, San_Pham_ID, SL_Nhap, Don_Gia_Nhap) 
                            VALUES (@Nhap_Kho_ID, @San_Pham_ID, @SL_Nhap, @Don_Gia_Nhap)";
                        foreach (var detail in phieuNhapFull.Details)
                        {
                            detail.Nhap_Kho_ID = maNkMoi;
                            await connection.ExecuteAsync(detailSql, detail, transaction: transaction);
                        }
                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 2627 || ex.Number == 2601) { transaction.Rollback(); throw new Exception("Lỗi: Số phiếu nhập này đã tồn tại."); }
                        transaction.Rollback(); throw;
                    }
                    catch (Exception) { transaction.Rollback(); throw; }
                }
            }
        }

        /* HÀM CẬP NHẬT PHIẾU (Bài 8 - Logic 12 Bảng) */
        public async Task UpdatePhieuNhap(NhapKho nhapKho)
        {
            string sql = @"
                INSERT INTO tbl_XNK_Nhap_Kho 
                    (Ma_NK_Goc, So_Phieu_Nhap_Kho, Kho_ID, NCC_ID, Ngay_Nhap_Kho, Ghi_Chu) 
                VALUES 
                    (@Ma_NK, @So_Phieu_Nhap_Kho, @Kho_ID, @NCC_ID, @Ngay_Nhap_Kho, @Ghi_Chu)";
            using (var connection = new SqlConnection(_connectionString))
            {
                try { await connection.ExecuteAsync(sql, nhapKho); }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627 || ex.Number == 2601) { throw new Exception("Lỗi: Số phiếu nhập (hiệu chỉnh) này đã tồn tại."); }
                    throw;
                }
            }
        }

        /* HÀM XÓA PHIẾU (Bài 7) */
        public async Task DeletePhieuNhap(int maNK)
        {
            string sql = "DELETE FROM tbl_DM_Nhap_Kho WHERE Ma_NK = @Ma_NK";
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(sql, new { Ma_NK = maNK });
            }
        }

        /* HÀM CRUD CHI TIẾT (Bài 9) */
        public async Task UpdateChiTiet(NhapKhoRawData detail)
        {
            string sql = "UPDATE tbl_DM_Nhap_Kho_Raw_Data SET SL_Nhap = @SL_Nhap, Don_Gia_Nhap = @Don_Gia_Nhap WHERE ID = @ID";
            using (var connection = new SqlConnection(_connectionString)) { await connection.ExecuteAsync(sql, detail); }
        }
        public async Task AddChiTiet(NhapKhoRawData detail)
        {
            string sql = "INSERT INTO tbl_DM_Nhap_Kho_Raw_Data (Nhap_Kho_ID, San_Pham_ID, SL_Nhap, Don_Gia_Nhap) VALUES (@Nhap_Kho_ID, @San_Pham_ID, @SL_Nhap, @Don_Gia_Nhap)";
            using (var connection = new SqlConnection(_connectionString)) { await connection.ExecuteAsync(sql, detail); }
        }
        public async Task DeleteChiTiet(int id)
        {
            string sql = "DELETE FROM tbl_DM_Nhap_Kho_Raw_Data WHERE ID = @Id";
            using (var connection = new SqlConnection(_connectionString)) { await connection.ExecuteAsync(sql, new { Id = id }); }
        }

        /* HÀM LẤY DỮ LIỆU ĐỂ IN (Bài 10) */
        public async Task<PhieuNhapViewModel> GetPhieuNhapView(int maNK)
        {
            var header = await GetPhieuNhapById(maNK);
            if (header == null) return null;
            var details = await GetChiTiet(maNK);
            decimal tongTien = details.Sum(d => d.SL_Nhap * d.Don_Gia_Nhap);
            var viewModel = new PhieuNhapViewModel
            {
                Header = header,
                Details = details,
                TongTienSo = tongTien,
                TongTienVietChu = "..."
            };
            return viewModel;
        }

        /* HÀM LẤY DỮ LIỆU BÁO CÁO (Bài 15) */
        public async Task<IEnumerable<BaoCaoChiTietHangNhapViewModel>> GetBaoCaoChiTietHangNhap(DateTime tuNgay, DateTime denNgay)
        {
            string sql = @"
                SELECT nk.Ngay_Nhap_Kho, nk.So_Phieu_Nhap_Kho, ncc.Ten_NCC, nkr.San_Pham_ID, sp.Ten_San_Pham, nkr.SL_Nhap, nkr.Don_Gia_Nhap
                FROM tbl_DM_Nhap_Kho_Raw_Data nkr
                INNER JOIN tbl_DM_Nhap_Kho nk ON nkr.Nhap_Kho_ID = nk.Ma_NK
                INNER JOIN tbl_DM_NCC ncc ON nk.NCC_ID = ncc.Ma_NCC
                INNER JOIN tbl_DM_San_Pham sp ON nkr.San_Pham_ID = sp.Ma_San_Pham
                WHERE nk.Ngay_Nhap_Kho >= @TuNgay AND nk.Ngay_Nhap_Kho <= @DenNgay
                ORDER BY nk.Ngay_Nhap_Kho, nk.So_Phieu_Nhap_Kho, sp.Ten_San_Pham;
            ";
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryAsync<BaoCaoChiTietHangNhapViewModel>(sql, new { TuNgay = tuNgay, DenNgay = denNgay });
            }
        }
        /* HÀM MỚI DÙNG CHO PHIẾU IN (LẤY THÊM TÊN ĐVT) */
        public async Task<List<NhapKhoRawData>> GetChiTietFull(int nhapKhoId)
        {
            // SỬA LỖI: Đổi 'tbl_NK_Chi_Tiet' thành 'tbl_NK_CT'
            string sql = @"
        SELECT 
            nkct.ID, nkct.Nhap_Kho_ID, nkct.San_Pham_ID, 
            sp.Ma_SP, sp.Ten_San_Pham, dvt.Ten_Don_Vi_Tinh,
            nkct.SL_Nhap, nkct.Don_Gia_Nhap
        FROM tbl_NK_CT nkct  -- <-- ĐÃ SỬA TẠI ĐÂY
        JOIN tbl_DM_San_Pham sp ON nkct.San_Pham_ID = sp.Ma_San_Pham
        JOIN tbl_DM_Don_Vi_Tinh dvt ON sp.Don_Vi_Tinh_ID = dvt.Ma_Don_Vi_Tinh
        WHERE nkct.Nhap_Kho_ID = @Id;
    ";
            using (var connection = new SqlConnection(_connectionString))
            {
                var result = await connection.QueryAsync<NhapKhoRawData>(sql, new { Id = nhapKhoId });
                return result.ToList();
            }
        }
    }
}