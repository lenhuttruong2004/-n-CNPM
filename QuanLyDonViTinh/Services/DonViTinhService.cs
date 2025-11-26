using QuanLyDonViTinh.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Configuration;

namespace QuanLyDonViTinh.Services
{
    public class DonViTinhService
    {
        private readonly string _connectionString;

        public DonViTinhService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<DonViTinh>> GetDanhSach()
        {
            string sql = "SELECT * FROM tbl_DM_Don_Vi_Tinh ORDER BY Id DESC";
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryAsync<DonViTinh>(sql);
            }
        }

        public async Task<DonViTinh> GetDonViTinhById(int id)
        {
            string sql = "SELECT * FROM tbl_DM_Don_Vi_Tinh WHERE Id = @Id";
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QuerySingleOrDefaultAsync<DonViTinh>(sql, new { Id = id });
            }
        }

        // =============================
        // HÀM KIỂM TRA TRÙNG TÊN (GIỮ NGUYÊN)
        // =============================
        private async Task<bool> TenDVT_DaTonTai(string tenDVT, int id = 0)
        {
            string sql = @"
                SELECT COUNT(*) 
                FROM tbl_DM_Don_Vi_Tinh
                WHERE LOWER(LTRIM(RTRIM(Ten_Don_Vi_Tinh))) = LOWER(@Ten)
                AND Id <> @Id";

            using (var connection = new SqlConnection(_connectionString))
            {
                int count = await connection.ExecuteScalarAsync<int>(sql,
                    new { Ten = tenDVT.Trim().ToLower(), Id = id });

                return count > 0;
            }
        }

        // =============================
        // VALIDATE (GIỮ NGUYÊN)
        // =============================
        private void ValidateDonViTinh(DonViTinh donViTinh)
        {
            donViTinh.Ten_Don_Vi_Tinh = donViTinh.Ten_Don_Vi_Tinh?.Trim();
            donViTinh.Ghi_Chu = donViTinh.Ghi_Chu?.Trim();

            if (string.IsNullOrEmpty(donViTinh.Ten_Don_Vi_Tinh))
                throw new Exception("Tên đơn vị tính không được để trống.");

            if (donViTinh.Ten_Don_Vi_Tinh.Length > 100)
                throw new Exception("Tên đơn vị tính không được vượt quá 100 ký tự.");

            if (donViTinh.Ghi_Chu?.Length > 500)
                throw new Exception("Ghi chú không được vượt quá 500 ký tự.");
        }

        // =============================
        // CREATE (ĐÃ BỔ SUNG TRY-CATCH)
        // =============================
        public async Task AddDonViTinh(DonViTinh donViTinh)
        {
            // 1. Giữ nguyên logic Validate và Check trùng thủ công của bạn
            ValidateDonViTinh(donViTinh);

            if (await TenDVT_DaTonTai(donViTinh.Ten_Don_Vi_Tinh))
                throw new Exception("Tên đơn vị tính này đã tồn tại.");

            string sql = "INSERT INTO tbl_DM_Don_Vi_Tinh (Ten_Don_Vi_Tinh, Ghi_Chu) VALUES (@Ten_Don_Vi_Tinh, @Ghi_Chu)";

            // 2. Bọc ExecuteAsync trong try-catch để chặn lỗi SQL (Race Condition)
            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.ExecuteAsync(sql, donViTinh);
                }
                catch (SqlException ex)
                {
                    // Bắt lỗi trùng lặp (nếu check ở trên bị lọt lưới do 2 người thêm cùng lúc)
                    if (ex.Number == 2627 || ex.Number == 2601)
                        throw new Exception("Tên đơn vị tính này đã tồn tại (Lỗi hệ thống).");
                    else
                        throw;
                }
            }
        }

        // =============================
        // UPDATE (ĐÃ BỔ SUNG TRY-CATCH)
        // =============================
        public async Task UpdateDonViTinh(DonViTinh donViTinh)
        {
            if (donViTinh.Id <= 0)
                throw new Exception("ID không hợp lệ.");

            // 1. Giữ nguyên logic cũ
            ValidateDonViTinh(donViTinh);

            if (await TenDVT_DaTonTai(donViTinh.Ten_Don_Vi_Tinh, donViTinh.Id))
                throw new Exception("Tên đơn vị tính này đã tồn tại.");

            string sql = @"UPDATE tbl_DM_Don_Vi_Tinh 
                           SET Ten_Don_Vi_Tinh = @Ten_Don_Vi_Tinh, 
                               Ghi_Chu = @Ghi_Chu 
                           WHERE Id = @Id";

            // 2. Bọc ExecuteAsync trong try-catch
            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.ExecuteAsync(sql, donViTinh);
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627 || ex.Number == 2601)
                        throw new Exception("Tên đơn vị tính này đã tồn tại (Lỗi hệ thống).");
                    else
                        throw;
                }
            }
        }

        // =============================
        // DELETE (GIỮ NGUYÊN - VÌ ĐÃ CÓ TRY-CATCH)
        // =============================
        public async Task DeleteDonViTinh(int id)
        {
            if (id <= 0)
                throw new Exception("ID không hợp lệ.");

            string sql = "DELETE FROM tbl_DM_Don_Vi_Tinh WHERE Id = @Id";

            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.ExecuteAsync(sql, new { Id = id });
                }
                catch (SqlException ex)
                {
                    // Lỗi 547: Đang được sử dụng ở bảng Sản Phẩm
                    if (ex.Number == 547)
                        throw new Exception("Đơn vị tính này đang được sử dụng cho sản phẩm, không thể xóa.");
                    else
                        throw;
                }
            }
        }
    }
}