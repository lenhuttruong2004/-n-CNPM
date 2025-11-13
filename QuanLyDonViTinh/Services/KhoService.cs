using QuanLyDonViTinh.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Configuration;

namespace QuanLyDonViTinh.Services
{
    public class KhoService
    {
        private readonly string _connectionString;

        public KhoService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        /* HÀM LẤY DANH SÁCH (READ) */
        public async Task<IEnumerable<Kho>> GetDanhSach()
        {
            // "SELECT *" sẽ tự động map "Id" (DB) sang "Id" (Model)
            string sql = "SELECT * FROM tbl_DM_Kho";
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryAsync<Kho>(sql);
            }
        }

        /* HÀM THÊM MỚI (CREATE) */
        public async Task AddKho(Kho kho)
        {
            // Câu lệnh SQL này đã đúng vì nó không chèn Id (Id là tự tăng)
            string sql = "INSERT INTO tbl_DM_Kho (Ten_Kho, Ghi_Chu) VALUES (@Ten_Kho, @Ghi_Chu)";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.ExecuteAsync(sql, kho);
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    throw new Exception("Tên kho này đã tồn tại. Vui lòng nhập tên khác.");
                }
                else { throw; }
            }
        }

        /* HÀM CẬP NHẬT (UPDATE / SỬA) */
        public async Task UpdateKho(Kho kho)
        {
            // === SỬA ===
            // Đổi "WHERE Ma_Kho = @Ma_Kho" thành "WHERE Id = @Id"
            // Dapper sẽ tự động map "kho.Id" vào tham số "@Id"
            string sql = "UPDATE tbl_DM_Kho SET Ten_Kho = @Ten_Kho, Ghi_Chu = @Ghi_Chu WHERE Id = @Id";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.ExecuteAsync(sql, kho);
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    throw new Exception("Tên kho này đã tồn tại. Vui lòng nhập tên khác.");
                }
                else { throw; }
            }
        }

        /* HÀM XÓA (DELETE) */
        public async Task DeleteKho(int id)
        {
            // === SỬA ===
            // Đổi "WHERE Ma_Kho = @Id" thành "WHERE Id = @Id"
            string sql = "DELETE FROM tbl_DM_Kho WHERE Id = @Id";
            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.ExecuteAsync(sql, new { Id = id });
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 547)
                    {
                        throw new Exception("Không thể xóa kho này vì nó đang được sử dụng ở Phân quyền User hoặc Phiếu Nhập/Xuất.");
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        public async Task<Kho> GetKhoById(int id)
        {
            // === SỬA ===
            // Đổi "WHERE Ma_Kho = @Id" thành "WHERE Id = @Id"
            string sql = "SELECT * FROM tbl_DM_Kho WHERE Id = @Id";
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QuerySingleOrDefaultAsync<Kho>(sql, new { Id = id });
            }
        }
    }
}