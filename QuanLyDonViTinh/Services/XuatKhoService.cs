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

        // ===================================
        // HÀM HỖ TRỢ KIỂM TRA TRÙNG SỐ PHIẾU (Case-Insensitive & Trim-Insensitive)
        // ===================================
        private async Task<bool> SoPhieuXuat_DaTonTai(string soPhieu, int id = 0)
        {
            // Kiểm tra trùng Số phiếu trong tbl_DM_Xuat_Kho
            string sql = @"
                SELECT COUNT(*)
                FROM tbl_DM_Xuat_Kho
                WHERE UPPER(LTRIM(RTRIM(So_Phieu_Xuat_Kho))) = @SoPhieu_Cleaned
                AND Id <> @Id";

            using (var connection = new SqlConnection(_connectionString))
            {
                int count = await connection.ExecuteScalarAsync<int>(sql,
                    new { SoPhieu_Cleaned = soPhieu.Trim().ToUpper(), Id = id });
                return count > 0;
            }
        }

        /* 1. LẤY DANH SÁCH PHIẾU */
        public async Task<IEnumerable<XuatKho>> GetDanhSach()
        {
            string sql = @"
                SELECT 
                    xk.Id, 
                    xk.So_Phieu_Xuat_Kho, 
                    xk.Ngay_Xuat_Kho, 
                    xk.Ghi_Chu,
                    k.Ten_Kho, 
                    ISNULL(SUM(xkr.SL_Xuat * xkr.Don_Gia_Xuat), 0) AS Tong_Tien
                FROM tbl_DM_Xuat_Kho xk
                LEFT JOIN tbl_DM_Kho k ON xk.Kho_ID = k.Id
                LEFT JOIN tbl_DM_Xuat_Kho_Raw_Data xkr ON xk.Id = xkr.Xuat_Kho_ID
                GROUP BY 
                    xk.Id, xk.So_Phieu_Xuat_Kho, xk.Ngay_Xuat_Kho, xk.Ghi_Chu, k.Ten_Kho
                ORDER BY xk.Id DESC";

            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryAsync<XuatKho>(sql);
            }
        }

        /* 2. LẤY 1 PHIẾU THEO ID */
        public async Task<XuatKho> GetPhieuXuatById(int id)
        {
            string sql = @"
                SELECT Id, So_Phieu_Xuat_Kho, Kho_ID, Ngay_Xuat_Kho, Ghi_Chu 
                FROM tbl_DM_Xuat_Kho 
                WHERE Id = @Id";

            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QuerySingleOrDefaultAsync<XuatKho>(sql, new { Id = id });
            }
        }

        /* 3. LẤY CHI TIẾT */
        public async Task<List<XuatKhoRawData>> GetChiTiet(int id)
        {
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
                var result = await connection.QueryAsync<XuatKhoRawData>(sql, new { Id = id });
                return result.ToList();
            }
        }

        /* 4. THÊM MỚI PHIẾU - Đã xử lý lỗi FK 547 */
        public async Task AddPhieuXuat(XuatKhoFull phieuXuatFull)
        {
            if (phieuXuatFull.Details == null || !phieuXuatFull.Details.Any())
                throw new Exception("Phiếu xuất phải có ít nhất một sản phẩm chi tiết.");

            // Chuẩn hóa Số phiếu xuất
            phieuXuatFull.Header.So_Phieu_Xuat_Kho = phieuXuatFull.Header.So_Phieu_Xuat_Kho?.Trim().ToUpper();

            // === KIỂM TRA TRÙNG SỐ PHIẾU TRƯỚC KHI THÊM ===
            if (await SoPhieuXuat_DaTonTai(phieuXuatFull.Header.So_Phieu_Xuat_Kho))
            {
                throw new Exception($" Số phiếu xuất '{phieuXuatFull.Header.So_Phieu_Xuat_Kho}' đã tồn tại.");
            }

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
                        transaction.Rollback();
                        // Lỗi Duplicate Key
                        if (ex.Number == 2627 || ex.Number == 2601)
                        {
                            throw new Exception("Lỗi: Số phiếu xuất này đã tồn tại.");
                        }
                        // Lỗi Foreign Key (547)
                        else if (ex.Number == 547)
                        {
                            if (ex.Message.Contains("FK_tbl_DM_Xuat_Kho_tbl_DM_Kho"))
                                throw new Exception("Kho hàng bạn chọn không còn tồn tại (đã bị xóa). Vui lòng chọn lại.");
                            if (ex.Message.Contains("FK_tbl_DM_Xuat_Kho_Raw_Data_tbl_DM_San_Pham"))
                                throw new Exception("Có sản phẩm trong phiếu không còn tồn tại (đã bị xóa).");

                            throw new Exception("Lỗi dữ liệu tham chiếu (Kho hoặc Sản phẩm) không tồn tại.");
                        }

                        throw;
                    }
                    catch (Exception) { transaction.Rollback(); throw; }
                }
            }
        }

        /* 5. CẬP NHẬT PHIẾU - Đã xử lý lỗi FK 547 */
        public async Task UpdatePhieuXuat(XuatKho xuatKho)
        {
            // Chuẩn hóa Số phiếu xuất
            xuatKho.So_Phieu_Xuat_Kho = xuatKho.So_Phieu_Xuat_Kho?.Trim().ToUpper();

            // === KIỂM TRA TRÙNG SỐ PHIẾU TRƯỚC KHI SỬA (Bỏ qua chính nó) ===
            if (await SoPhieuXuat_DaTonTai(xuatKho.So_Phieu_Xuat_Kho, xuatKho.Id))
            {
                throw new Exception($"Lỗi: Số phiếu xuất '{xuatKho.So_Phieu_Xuat_Kho}' đã tồn tại.");
            }

            string sql = @"
                UPDATE tbl_DM_Xuat_Kho 
                SET So_Phieu_Xuat_Kho = @So_Phieu_Xuat_Kho,
                    Kho_ID = @Kho_ID,
                    Ngay_Xuat_Kho = @Ngay_Xuat_Kho,
                    Ghi_Chu = @Ghi_Chu
                WHERE Id = @Id";

            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.ExecuteAsync(sql, xuatKho);
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627 || ex.Number == 2601)
                    {
                        throw new Exception("Lỗi: Số phiếu xuất này đã tồn tại (DB check).");
                    }
                    else if (ex.Number == 547)
                    {
                        if (ex.Message.Contains("FK_tbl_DM_Xuat_Kho_tbl_DM_Kho"))
                            throw new Exception("Kho hàng bạn chọn không còn tồn tại.");

                        throw new Exception("Lỗi dữ liệu tham chiếu không tồn tại.");
                    }
                    throw;
                }
            }
        }

        /* 6. XÓA PHIẾU */
        public async Task DeletePhieuXuat(int id)
        {
            string sql = "DELETE FROM tbl_DM_Xuat_Kho WHERE Id = @Id";
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(sql, new { Id = id });
            }
        }

        /* 7. CRUD CHI TIẾT - Đã xử lý lỗi FK 547 cho Sản phẩm */
        public async Task UpdateChiTiet(XuatKhoRawData detail)
        {
            string sql = "UPDATE tbl_DM_Xuat_Kho_Raw_Data SET SL_Xuat = @SL_Xuat, Don_Gia_Xuat = @Don_Gia_Xuat WHERE Id = @Id";
            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.ExecuteAsync(sql, detail);
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 547) throw new Exception("Sản phẩm không tồn tại.");
                    throw;
                }
            }
        }

        public async Task AddChiTiet(XuatKhoRawData detail)
        {
            string sql = "INSERT INTO tbl_DM_Xuat_Kho_Raw_Data (Xuat_Kho_ID, San_Pham_ID, SL_Xuat, Don_Gia_Xuat) VALUES (@Xuat_Kho_ID, @San_Pham_ID, @SL_Xuat, @Don_Gia_Xuat)";
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
            string sql = "DELETE FROM tbl_DM_Xuat_Kho_Raw_Data WHERE Id = @Id";
            using (var connection = new SqlConnection(_connectionString)) { await connection.ExecuteAsync(sql, new { Id = id }); }
        }

        /* 8. LẤY DỮ LIỆU ĐỂ IN */
        public async Task<PhieuXuatViewModel> GetPhieuXuatView(int id)
        {
            var header = await GetPhieuXuatById(id);
            if (header == null) return null;
            var details = await GetChiTiet(id);
            var viewModel = new PhieuXuatViewModel
            {
                Header = header,
                Details = details,
                TongSoLuongVietSo = details.Sum(d => d.SL_Xuat).ToString("N2"),
                TongSoLuongVietChu = "..."
            };
            return viewModel;
        }

        /* 9. BÁO CÁO CHI TIẾT HÀNG XUẤT */
        public async Task<IEnumerable<BaoCaoChiTietHangXuatViewModel>> GetBaoCaoChiTietHangXuat(DateTime tuNgay, DateTime denNgay)
        {
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
}XmlConfigurationExtensions