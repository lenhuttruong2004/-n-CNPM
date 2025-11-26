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
    public class NhapKhoService
    {
        private readonly string _connectionString;

        public NhapKhoService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // ===================================
        // HÀM HỖ TRỢ KIỂM TRA TRÙNG SỐ PHIẾU (Case-Insensitive & Trim-Insensitive)
        // ===================================
        private async Task<bool> SoPhieu_DaTonTai(string soPhieu, int id = 0)
        {
            string sql = @"
                SELECT COUNT(*)
                FROM tbl_DM_Nhap_Kho
                WHERE UPPER(LTRIM(RTRIM(So_Phieu_Nhap_Kho))) = @SoPhieu_Cleaned
                AND Id <> @Id";

            using (var connection = new SqlConnection(_connectionString))
            {
                int count = await connection.ExecuteScalarAsync<int>(sql,
                    new { SoPhieu_Cleaned = soPhieu.Trim().ToUpper(), Id = id });
                return count > 0;
            }
        }

        /* 1. LẤY DANH SÁCH (READ) */
        public async Task<IEnumerable<NhapKho>> GetDanhSach()
        {
            string sql = @"
                SELECT 
                    nk.Id, 
                    nk.So_Phieu_Nhap_Kho, 
                    nk.Ngay_Nhap_Kho, 
                    nk.Ghi_Chu,
                    k.Ten_Kho, 
                    ncc.Ten_NCC,
                    ISNULL(SUM(nkr.SL_Nhap * nkr.Don_Gia_Nhap), 0) AS Tong_Tien
                FROM tbl_DM_Nhap_Kho nk
                LEFT JOIN tbl_DM_Kho k ON nk.Kho_ID = k.Id
                LEFT JOIN tbl_DM_NCC ncc ON nk.NCC_ID = ncc.Id
                LEFT JOIN tbl_DM_Nhap_Kho_Raw_Data nkr ON nk.Id = nkr.Nhap_Kho_ID
                GROUP BY 
                    nk.Id, nk.So_Phieu_Nhap_Kho, nk.Ngay_Nhap_Kho, nk.Ghi_Chu,
                    k.Ten_Kho, ncc.Ten_NCC
                ORDER BY nk.Id DESC";

            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryAsync<NhapKho>(sql);
            }
        }

        /* 2. LẤY 1 PHIẾU THEO ID (READ) */
        public async Task<NhapKho> GetPhieuNhapById(int id)
        {
            string sql = @"
                SELECT Id, So_Phieu_Nhap_Kho, Kho_ID, NCC_ID, Ngay_Nhap_Kho, Ghi_Chu 
                FROM tbl_DM_Nhap_Kho 
                WHERE Id = @Id";

            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QuerySingleOrDefaultAsync<NhapKho>(sql, new { Id = id });
            }
        }

        /* 3. THÊM MỚI PHIẾU (CREATE) - Đã sửa lỗi FK Violation */
        public async Task AddPhieuNhap(NhapKhoFull phieuNhapFull)
        {
            if (phieuNhapFull.Details == null || !phieuNhapFull.Details.Any())
                throw new Exception("Phiếu nhập phải có ít nhất một sản phẩm chi tiết.");

            phieuNhapFull.Header.So_Phieu_Nhap_Kho = phieuNhapFull.Header.So_Phieu_Nhap_Kho?.Trim().ToUpper();

            if (await SoPhieu_DaTonTai(phieuNhapFull.Header.So_Phieu_Nhap_Kho))
            {
                throw new Exception($"Lỗi: Số phiếu nhập '{phieuNhapFull.Header.So_Phieu_Nhap_Kho}' đã tồn tại.");
            }

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

                        int newId = await connection.QuerySingleAsync<int>(headerSql, phieuNhapFull.Header, transaction: transaction);

                        string detailSql = @"
                            INSERT INTO tbl_DM_Nhap_Kho_Raw_Data (Nhap_Kho_ID, San_Pham_ID, SL_Nhap, Don_Gia_Nhap) 
                            VALUES (@Nhap_Kho_ID, @San_Pham_ID, @SL_Nhap, @Don_Gia_Nhap)";
                        foreach (var detail in phieuNhapFull.Details)
                        {
                            detail.Nhap_Kho_ID = newId;
                            await connection.ExecuteAsync(detailSql, detail, transaction: transaction);
                        }
                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        // Lỗi trùng lặp khóa chính hoặc unique index
                        if (ex.Number == 2627 || ex.Number == 2601)
                        {
                            throw new Exception("Lỗi: Số phiếu nhập này đã tồn tại (DB check).");
                        }
                        // Lỗi vi phạm khóa ngoại (Foreign Key) - 547
                        else if (ex.Number == 547)
                        {
                            if (ex.Message.Contains("FK_tbl_DM_Nhap_Kho_tbl_DM_Kho"))
                                throw new Exception("Kho hàng bạn chọn không còn tồn tại (đã bị xóa). Vui lòng tải lại trang.");
                            if (ex.Message.Contains("FK_tbl_DM_Nhap_Kho_tbl_DM_NCC"))
                                throw new Exception("Nhà cung cấp bạn chọn không còn tồn tại. Vui lòng tải lại trang.");
                            if (ex.Message.Contains("FK_tbl_DM_Nhap_Kho_Raw_Data_tbl_DM_San_Pham"))
                                throw new Exception("Có sản phẩm trong danh sách không còn tồn tại (đã bị xóa).");

                            throw new Exception("Lỗi dữ liệu tham chiếu không tồn tại (Kho, NCC hoặc Sản phẩm).");
                        }
                        throw;
                    }
                    catch (Exception) { transaction.Rollback(); throw; }
                }
            }
        }

        /* 4. CẬP NHẬT PHIẾU (UPDATE) - Đã sửa lỗi FK Violation */
        public async Task UpdatePhieuNhap(NhapKho nhapKho)
        {
            nhapKho.So_Phieu_Nhap_Kho = nhapKho.So_Phieu_Nhap_Kho?.Trim().ToUpper();

            if (await SoPhieu_DaTonTai(nhapKho.So_Phieu_Nhap_Kho, nhapKho.Id))
            {
                throw new Exception($"Lỗi: Số phiếu nhập '{nhapKho.So_Phieu_Nhap_Kho}' đã tồn tại.");
            }

            string sql = @"
                UPDATE tbl_DM_Nhap_Kho 
                SET So_Phieu_Nhap_Kho = @So_Phieu_Nhap_Kho,
                    Kho_ID = @Kho_ID,
                    NCC_ID = @NCC_ID,
                    Ngay_Nhap_Kho = @Ngay_Nhap_Kho,
                    Ghi_Chu = @Ghi_Chu
                WHERE Id = @Id";

            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.ExecuteAsync(sql, nhapKho);
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627 || ex.Number == 2601)
                    {
                        throw new Exception("Lỗi: Số phiếu nhập này đã tồn tại (DB check).");
                    }
                    else if (ex.Number == 547)
                    {
                        if (ex.Message.Contains("FK_tbl_DM_Nhap_Kho_tbl_DM_Kho"))
                            throw new Exception("Kho hàng bạn chọn không còn tồn tại.");
                        if (ex.Message.Contains("FK_tbl_DM_Nhap_Kho_tbl_DM_NCC"))
                            throw new Exception("Nhà cung cấp bạn chọn không còn tồn tại.");

                        throw new Exception("Lỗi dữ liệu tham chiếu không tồn tại.");
                    }
                    throw;
                }
            }
        }

        /* 5. LẤY CHI TIẾT (READ) */
        public async Task<List<NhapKhoRawData>> GetChiTiet(int nhapKhoId)
        {
            string sql = @"
                SELECT 
                    nkr.Id, nkr.Nhap_Kho_ID, nkr.San_Pham_ID, nkr.SL_Nhap, nkr.Don_Gia_Nhap,
                    sp.Ma_San_Pham, 
                    sp.Ten_San_Pham,
                    dvt.Ten_Don_Vi_Tinh 
                FROM tbl_DM_Nhap_Kho_Raw_Data nkr
                LEFT JOIN tbl_DM_San_Pham sp ON nkr.San_Pham_ID = sp.Id
                LEFT JOIN tbl_DM_Don_Vi_Tinh dvt ON sp.Don_Vi_Tinh_ID = dvt.Id
                WHERE nkr.Nhap_Kho_ID = @NhapKhoId";

            using (var connection = new SqlConnection(_connectionString))
            {
                var result = await connection.QueryAsync<NhapKhoRawData>(sql, new { NhapKhoId = nhapKhoId });
                return result.ToList();
            }
        }

        /* 6. XÓA PHIẾU (DELETE) */
        public async Task DeletePhieuNhap(int id)
        {
            // Sử dụng Transaction để đảm bảo xóa sạch Chi tiết rồi mới xóa Phiếu
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Bước 1: Xóa các dòng chi tiết (Raw Data) thuộc phiếu này trước
                        string deleteDetailSql = "DELETE FROM tbl_DM_Nhap_Kho_Raw_Data WHERE Nhap_Kho_ID = @Id";
                        await connection.ExecuteAsync(deleteDetailSql, new { Id = id }, transaction: transaction);

                        // Bước 2: Xóa phiếu nhập (Header)
                        string deleteHeaderSql = "DELETE FROM tbl_DM_Nhap_Kho WHERE Id = @Id";
                        await connection.ExecuteAsync(deleteHeaderSql, new { Id = id }, transaction: transaction);

                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        // Nếu vẫn bị lỗi FK (do phiếu nhập này đã được dùng để tính tồn kho/xuất kho ở bảng khác)
                        if (ex.Number == 547)
                        {
                            throw new Exception("Không thể xóa phiếu nhập này vì dữ liệu đã phát sinh liên quan (Báo cáo/Tồn kho).");
                        }
                        throw;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /* 7. CÁC HÀM CRUD CHI TIẾT - Đã sửa lỗi FK Violation cho Sản phẩm */
        public async Task UpdateChiTiet(NhapKhoRawData detail)
        {
            string sql = "UPDATE tbl_DM_Nhap_Kho_Raw_Data SET SL_Nhap = @SL_Nhap, Don_Gia_Nhap = @Don_Gia_Nhap WHERE Id = @ID";
            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.ExecuteAsync(sql, detail);
                }
                catch (SqlException ex)
                {
                    // Update thì ít khi lỗi FK trừ khi đổi San_Pham_ID (thường không đổi ở giao diện list)
                    // Nhưng cứ thêm cho an toàn
                    if (ex.Number == 547) throw new Exception("Sản phẩm không tồn tại.");
                    throw;
                }
            }
        }

        public async Task AddChiTiet(NhapKhoRawData detail)
        {
            string sql = "INSERT INTO tbl_DM_Nhap_Kho_Raw_Data (Nhap_Kho_ID, San_Pham_ID, SL_Nhap, Don_Gia_Nhap) VALUES (@Nhap_Kho_ID, @San_Pham_ID, @SL_Nhap, @Don_Gia_Nhap)";
            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.ExecuteAsync(sql, detail);
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 547)
                        throw new Exception("Sản phẩm bạn chọn thêm vào chi tiết không còn tồn tại (đã bị xóa).");
                    throw;
                }
            }
        }

        public async Task DeleteChiTiet(int id)
        {
            string sql = "DELETE FROM tbl_DM_Nhap_Kho_Raw_Data WHERE Id = @Id";
            using (var connection = new SqlConnection(_connectionString)) { await connection.ExecuteAsync(sql, new { Id = id }); }
        }

        /* 8. CÁC HÀM VIEW MODEL & REPORT (Giữ nguyên) */
        public async Task<PhieuNhapViewModel> GetPhieuNhapView(int id)
        {
            var header = await GetPhieuNhapById(id);
            if (header == null) return null;
            var details = await GetChiTiet(id);
            decimal tongTien = details.Sum(d => d.SL_Nhap * d.Don_Gia_Nhap);

            return new PhieuNhapViewModel
            {
                Header = header,
                Details = details,
                TongTienSo = tongTien,
                TongTienVietChu = ""
            };
        }

        public async Task<IEnumerable<BaoCaoChiTietHangNhapViewModel>> GetBaoCaoChiTietHangNhap(DateTime tuNgay, DateTime denNgay)
        {
            string sql = @"
                SELECT 
                    nk.Ngay_Nhap_Kho, nk.So_Phieu_Nhap_Kho, ncc.Ten_NCC, 
                    nkr.San_Pham_ID, sp.Ma_San_Pham, sp.Ten_San_Pham, 
                    nkr.SL_Nhap, nkr.Don_Gia_Nhap
                FROM tbl_DM_Nhap_Kho_Raw_Data nkr
                INNER JOIN tbl_DM_Nhap_Kho nk ON nkr.Nhap_Kho_ID = nk.Id
                INNER JOIN tbl_DM_NCC ncc ON nk.NCC_ID = ncc.Id
                INNER JOIN tbl_DM_San_Pham sp ON nkr.San_Pham_ID = sp.Id
                WHERE nk.Ngay_Nhap_Kho >= @TuNgay AND nk.Ngay_Nhap_Kho <= @DenNgay
                ORDER BY nk.Ngay_Nhap_Kho, nk.So_Phieu_Nhap_Kho, sp.Ten_San_Pham;
            ";
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryAsync<BaoCaoChiTietHangNhapViewModel>(sql, new { TuNgay = tuNgay, DenNgay = denNgay });
            }
        }
    }
}