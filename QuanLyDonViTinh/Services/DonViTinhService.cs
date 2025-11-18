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
            string sql = "SELECT * FROM tbl_DM_Don_Vi_Tinh";
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
        // HÀM KIỂM TRA TRÙNG TÊN (IGNORE TRIM + LOWERCASE)
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
        // VALIDATE
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
        // CREATE
        // =============================
        public async Task AddDonViTinh(DonViTinh donViTinh)
        {
            ValidateDonViTinh(donViTinh);

            // KIỂM TRA TRÙNG
            if (await TenDVT_DaTonTai(donViTinh.Ten_Don_Vi_Tinh))
                throw new Exception("Tên đơn vị tính này đã tồn tại.");

            string sql = "INSERT INTO tbl_DM_Don_Vi_Tinh (Ten_Don_Vi_Tinh, Ghi_Chu) VALUES (@Ten_Don_Vi_Tinh, @Ghi_Chu)";
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(sql, donViTinh);
            }
        }

        // =============================
        // UPDATE
        // =============================
        public async Task UpdateDonViTinh(DonViTinh donViTinh)
        {
            if (donViTinh.Id <= 0)
                throw new Exception("ID không hợp lệ.");

            ValidateDonViTinh(donViTinh);

            // KIỂM TRA TRÙNG (IGNORE current ID)
            if (await TenDVT_DaTonTai(donViTinh.Ten_Don_Vi_Tinh, donViTinh.Id))
                throw new Exception("Tên đơn vị tính này đã tồn tại.");

            string sql = @"UPDATE tbl_DM_Don_Vi_Tinh 
                           SET Ten_Don_Vi_Tinh = @Ten_Don_Vi_Tinh, 
                               Ghi_Chu = @Ghi_Chu 
                           WHERE Id = @Id";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(sql, donViTinh);
            }
        }

        // =============================
        // DELETE
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
                    if (ex.Number == 547)
                        throw new Exception("Đơn vị tính này đang được sử dụng, không thể xóa.");
                    else
                        throw;
                }
            }
        }
    }
}
