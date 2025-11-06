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
            string sql = "SELECT * FROM tbl_DM_Don_Vi_Tinh WHERE Ma_Don_Vi_Tinh = @Id";
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QuerySingleOrDefaultAsync<DonViTinh>(sql, new { Id = id });
            }
        }

        // === HÀM HỖ TRỢ VALIDATE DỮ LIỆU ===
        private void ValidateDonViTinh(DonViTinh donViTinh)
        {
            // 1. Chuẩn hóa dữ liệu (Trim)
            donViTinh.Ten_Don_Vi_Tinh = donViTinh.Ten_Don_Vi_Tinh?.Trim();
            donViTinh.Ghi_Chu = donViTinh.Ghi_Chu?.Trim();

            // 2. Kiểm tra rỗng sau khi Trim
            if (string.IsNullOrEmpty(donViTinh.Ten_Don_Vi_Tinh))
            {
                throw new Exception("Tên đơn vị tính không được để trống.");
            }

            // 3. Kiểm tra độ dài (Ví dụ: giả sử DB thiết lập NVARCHAR(50))
            if (donViTinh.Ten_Don_Vi_Tinh.Length > 50)
            {
                throw new Exception("Tên đơn vị tính không được vượt quá 50 ký tự.");
            }
        }

        /* HÀM THÊM MỚI (CREATE) */
        public async Task AddDonViTinh(DonViTinh donViTinh)
        {
            ValidateDonViTinh(donViTinh); // Gọi hàm kiểm tra

            string sql = "INSERT INTO tbl_DM_Don_Vi_Tinh (Ten_Don_Vi_Tinh, Ghi_Chu) VALUES (@Ten_Don_Vi_Tinh, @Ghi_Chu)";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.ExecuteAsync(sql, donViTinh);
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                    throw new Exception("Tên đơn vị tính này đã tồn tại.");
                else
                    throw;
            }
        }

        /* HÀM CẬP NHẬT (UPDATE) */
        public async Task UpdateDonViTinh(DonViTinh donViTinh)
        {
            if (donViTinh.Ma_Don_Vi_Tinh <= 0) throw new Exception("ID không hợp lệ.");
            ValidateDonViTinh(donViTinh); // Gọi hàm kiểm tra

            string sql = "UPDATE tbl_DM_Don_Vi_Tinh SET Ten_Don_Vi_Tinh = @Ten_Don_Vi_Tinh, Ghi_Chu = @Ghi_Chu WHERE Ma_Don_Vi_Tinh = @Ma_Don_Vi_Tinh";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.ExecuteAsync(sql, donViTinh);
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                    throw new Exception("Tên đơn vị tính này đã tồn tại.");
                else throw;
            }
        }

        /* HÀM XÓA (DELETE) */
        public async Task DeleteDonViTinh(int id)
        {
            if (id <= 0) throw new Exception("ID không hợp lệ.");

            string sql = "DELETE FROM tbl_DM_Don_Vi_Tinh WHERE Ma_Don_Vi_Tinh = @Id";
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